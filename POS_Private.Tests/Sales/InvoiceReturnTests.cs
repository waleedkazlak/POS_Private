using POS_Private.Application.Commands.Invoice;
using POS_Private.Application.DTOs;
using POS_Private.Application.Exceptions;
using POS_Private.Domain.Entities;
using POS_Private.Domain.Enums;
using POS_Private.Tests.Common;
using Microsoft.EntityFrameworkCore;

namespace POS_Private.Tests.Sales;

public class InvoiceReturnTests
{
    [Fact]
    public async Task ProcessReturn_ValidReturnWithRestock_ShouldCalculateRefundAndRestockInventory()
    {
        // Arrange
        var (context, uow) = await TestDatabaseFactory.CreateSeededContextAsync();
        var currentUser = new MockCurrentUserService { UserId = 1 };
        var handler = new ProcessReturnCommandHandler(uow, currentUser);

        var branch = await context.Branches.FirstAsync();
        var inventory = await context.Inventories.FirstAsync(i => i.BranchId == branch.Id);
        var product = await context.Products.Include(p => p.Variants).FirstAsync();
        var variant = product.Variants.First();

        // Create original invoice with 5 items sold
        decimal unitPrice = 200m;
        int qtySold = 5;
        decimal taxAmount = 140m; // 14% on 1000
        decimal totalAmount = 1140m;

        var originalInvoice = new Invoice
        {
            InvoiceNumber = "INV-ORIG-001",
            InvoiceType = InvoiceType.Sale,
            Status = InvoiceStatus.Completed,
            BranchId = branch.Id,
            CashierUserId = 1,
            InventoryId = inventory.Id,
            SubTotal = 1000m,
            TaxAmount = taxAmount,
            TotalAmount = totalAmount,
            PaidAmount = totalAmount,
            Lines = new List<InvoiceLine>
            {
                new()
                {
                    ProductId = product.Id,
                    VariantId = variant.Id,
                    ProductName = product.NameEn,
                    Quantity = qtySold,
                    UnitPrice = unitPrice,
                    TaxAmount = taxAmount,
                    TotalLineAmount = 1000m,
                    InventoryId = inventory.Id
                }
            }
        };
        await context.Invoices.AddAsync(originalInvoice);

        // Ensure stock starts at 20
        var stock = await context.BranchStocks.FirstOrDefaultAsync(s => s.InventoryId == inventory.Id && s.ProductId == product.Id && s.VariantId == variant.Id);
        if (stock == null)
        {
            stock = new BranchStock
            {
                BranchId = branch.Id,
                InventoryId = inventory.Id,
                ProductId = product.Id,
                VariantId = variant.Id,
                QuantityOnHand = 20
            };
            await context.BranchStocks.AddAsync(stock);
        }
        else
        {
            stock.QuantityOnHand = 20;
        }
        await context.SaveChangesAsync();

        int initialStock = stock.QuantityOnHand;

        // Return 2 items out of 5
        int returnQty = 2;
        var originalLine = originalInvoice.Lines.First();
        var returnDto = new CreateReturnDto
        {
            OriginalInvoiceId = originalInvoice.Id,
            ReturnReason = "Wrong size requested by customer",
            RefundMethod = PaymentMethod.Cash,
            Lines = new List<CreateReturnLineDto>
            {
                new()
                {
                    InvoiceLineId = originalLine.Id,
                    Quantity = returnQty,
                    Restock = true
                }
            }
        };

        // Expected refund = (UnitPrice + Tax/SoldQty) * returnQty = (200 + 140/5) * 2 = 228 * 2 = 456 EGP
        decimal expectedRefund = (200m + (140m / 5m)) * returnQty;

        // Act
        var result = await handler.Handle(new ProcessReturnCommand(returnDto), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var returnResult = result.Data!;
        returnResult.RefundAmount.Should().Be(expectedRefund);
        returnResult.Lines.Should().HaveCount(1);
        returnResult.Lines[0].Quantity.Should().Be(returnQty);

        // Inventory restocked by returnQty
        var updatedStock = await context.BranchStocks.FirstAsync(s => s.Id == stock.Id);
        updatedStock.QuantityOnHand.Should().Be(initialStock + returnQty);

        // Stock movement created
        var movement = await context.StockMovements.FirstOrDefaultAsync(m => m.ReferenceNumber == returnResult.ReturnNumber);
        movement.Should().NotBeNull();
        movement!.MovementType.Should().Be(StockMovementType.Return);
        movement.QuantityChange.Should().Be(returnQty);
    }

    [Fact]
    public async Task ProcessReturn_QuantityExceedsSoldQuantity_ShouldThrowBadRequest()
    {
        // Arrange
        var (context, uow) = await TestDatabaseFactory.CreateSeededContextAsync();
        var currentUser = new MockCurrentUserService { UserId = 1 };
        var handler = new ProcessReturnCommandHandler(uow, currentUser);

        var product = await context.Products.FirstAsync();
        var invoice = new Invoice
        {
            InvoiceNumber = "INV-EXCEED-01",
            InvoiceType = InvoiceType.Sale,
            Status = InvoiceStatus.Completed,
            BranchId = 1,
            CashierUserId = 1,
            TotalAmount = 500m,
            Lines = new List<InvoiceLine>
            {
                new() { ProductId = product.Id, ProductName = "Jeans", Quantity = 2, UnitPrice = 250m, TotalLineAmount = 500m }
            }
        };
        await context.Invoices.AddAsync(invoice);
        await context.SaveChangesAsync();

        var line = invoice.Lines.First();
        var returnDto = new CreateReturnDto
        {
            OriginalInvoiceId = invoice.Id,
            ReturnReason = "Customer wants to return 5 items",
            RefundMethod = PaymentMethod.Cash,
            Lines = new List<CreateReturnLineDto>
            {
                new() { InvoiceLineId = line.Id, Quantity = 5, Restock = true } // Exceeds sold 2
            }
        };

        // Act & Assert
        var act = async () => await handler.Handle(new ProcessReturnCommand(returnDto), CancellationToken.None);
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*cannot exceed sold quantity*");
    }
}
