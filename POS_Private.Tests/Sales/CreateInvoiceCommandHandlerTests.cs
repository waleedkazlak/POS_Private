using POS_Private.Application.Commands.Invoice;
using POS_Private.Application.DTOs;
using POS_Private.Application.Exceptions;
using POS_Private.Domain.Entities;
using POS_Private.Domain.Enums;
using POS_Private.Tests.Common;
using Microsoft.EntityFrameworkCore;

namespace POS_Private.Tests.Sales;

public class CreateInvoiceCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidInvoice_ShouldCalculateTaxDiscountsAndStockCorrectly()
    {
        // Arrange
        var (context, uow) = await TestDatabaseFactory.CreateSeededContextAsync();
        var currentUser = new MockCurrentUserService { UserId = 1 };
        var handler = new CreateInvoiceCommandHandler(uow, currentUser);

        var branch = await context.Branches.FirstAsync();
        var inventory = await context.Inventories.FirstAsync(i => i.BranchId == branch.Id);
        var product = await context.Products.Include(p => p.Variants).FirstAsync();
        var variant = product.Variants.First();
        var customer = await context.Customers.FirstAsync();

        // Ensure stock exists for the product variant
        var branchStock = await context.BranchStocks.FirstOrDefaultAsync(bs => bs.InventoryId == inventory.Id && bs.ProductId == product.Id && bs.VariantId == variant.Id);
        if (branchStock == null)
        {
            branchStock = new BranchStock
            {
                BranchId = branch.Id,
                InventoryId = inventory.Id,
                ProductId = product.Id,
                VariantId = variant.Id,
                QuantityOnHand = 100,
                MinAlertLevel = 5
            };
            await context.BranchStocks.AddAsync(branchStock);
            await context.SaveChangesAsync();
        }
        else
        {
            branchStock.QuantityOnHand = 100;
            await context.SaveChangesAsync();
        }

        int initialQty = branchStock.QuantityOnHand;
        decimal initialCustomerSpent = customer.TotalSpent;
        int initialLoyaltyPoints = customer.LoyaltyPoints;

        // Line: 2 items at product.BasePrice
        decimal unitPrice = product.BasePrice;
        int qty = 2;
        decimal lineDiscount = 50.00m;
        decimal expectedSubTotal = (unitPrice * qty) - lineDiscount;
        decimal expectedTax = Math.Round(expectedSubTotal * 0.14m, 2);
        decimal expectedGrandTotal = expectedSubTotal + expectedTax;
        decimal paymentAmount = expectedGrandTotal + 100.00m; // Overpay by 100 -> change should be 100

        var dto = new CreateInvoiceDto
        {
            BranchId = branch.Id,
            InventoryId = inventory.Id,
            CustomerId = customer.Id,
            DiscountAmount = 0m,
            TaxPercentage = 14m,
            Lines = new List<CreateInvoiceLineDto>
            {
                new()
                {
                    ProductId = product.Id,
                    VariantId = variant.Id,
                    Quantity = qty,
                    DiscountAmount = lineDiscount,
                    Notes = "Item 1"
                }
            },
            Payments = new List<CreatePaymentDto>
            {
                new()
                {
                    PaymentMethod = PaymentMethod.Cash,
                    Amount = paymentAmount
                }
            }
        };

        // Act
        var response = await handler.Handle(new CreateInvoiceCommand(dto), CancellationToken.None);

        // Assert
        response.IsSuccess.Should().BeTrue();
        var invoiceDto = response.Data!;

        invoiceDto.SubTotal.Should().Be(expectedSubTotal);
        invoiceDto.TaxAmount.Should().Be(expectedTax);
        invoiceDto.TotalAmount.Should().Be(expectedGrandTotal);
        invoiceDto.PaidAmount.Should().Be(paymentAmount);
        invoiceDto.ChangeAmount.Should().Be(100.00m);
        invoiceDto.PaymentStatus.Should().Be(PaymentStatus.Paid);

        // Inventory check
        var updatedStock = await context.BranchStocks.FirstAsync(bs => bs.Id == branchStock.Id);
        updatedStock.QuantityOnHand.Should().Be(initialQty - qty);

        // Stock movement check
        var movement = await context.StockMovements.FirstOrDefaultAsync(m => m.ReferenceNumber == invoiceDto.InvoiceNumber);
        movement.Should().NotBeNull();
        movement!.QuantityChange.Should().Be(-qty);
        movement.MovementType.Should().Be(StockMovementType.Sale);

        // Customer loyalty points & spent check
        var updatedCustomer = await context.Customers.FirstAsync(c => c.Id == customer.Id);
        updatedCustomer.TotalSpent.Should().Be(initialCustomerSpent + expectedGrandTotal);
        int expectedPointsEarned = (int)(expectedGrandTotal / 50m);
        updatedCustomer.LoyaltyPoints.Should().Be(initialLoyaltyPoints + expectedPointsEarned);
    }

    [Fact]
    public async Task Handle_PercentageCouponWithMaxCap_ShouldCalculateDiscountAccurately()
    {
        // Arrange
        var (context, uow) = await TestDatabaseFactory.CreateSeededContextAsync();
        var currentUser = new MockCurrentUserService { UserId = 1 };
        var handler = new CreateInvoiceCommandHandler(uow, currentUser);

        var branch = await context.Branches.FirstAsync();
        var inventory = await context.Inventories.FirstAsync(i => i.BranchId == branch.Id);
        var product = await context.Products.FirstAsync();

        // Create coupon: 20% discount with 100 EGP max cap, min order 500 EGP
        var coupon = new Coupon
        {
            Code = "SUMMER20",
            DiscountType = DiscountType.Percentage,
            DiscountValue = 20m,
            MaxDiscountAmount = 100m,
            MinOrderAmount = 500m,
            IsActive = true,
            UsageLimit = 50,
            UsageCount = 0
        };
        await context.Coupons.AddAsync(coupon);
        await context.SaveChangesAsync();

        // 2 items at 600 EGP each = 1,200 EGP subtotal
        // 20% of 1,200 is 240, but capped at 100
        decimal unitPrice = 600.00m;
        int qty = 2;
        decimal subTotal = 1200.00m;
        decimal expectedCouponDiscount = 100.00m;
        decimal netAfterDiscount = subTotal - expectedCouponDiscount; // 1,100
        decimal expectedTax = Math.Round(netAfterDiscount * 0.14m, 2); // 154
        decimal expectedGrandTotal = netAfterDiscount + expectedTax;   // 1,254

        var dto = new CreateInvoiceDto
        {
            BranchId = branch.Id,
            InventoryId = inventory.Id,
            CouponCode = "SUMMER20",
            TaxPercentage = 14m,
            Lines = new List<CreateInvoiceLineDto>
            {
                new()
                {
                    ProductId = product.Id,
                    CustomUnitPrice = unitPrice,
                    Quantity = qty
                }
            },
            Payments = new List<CreatePaymentDto>
            {
                new()
                {
                    PaymentMethod = PaymentMethod.Visa,
                    Amount = expectedGrandTotal
                }
            }
        };

        // Act
        var response = await handler.Handle(new CreateInvoiceCommand(dto), CancellationToken.None);

        // Assert
        response.IsSuccess.Should().BeTrue();
        var result = response.Data!;
        result.SubTotal.Should().Be(subTotal);
        result.DiscountAmount.Should().Be(expectedCouponDiscount);
        result.TaxAmount.Should().Be(expectedTax);
        result.TotalAmount.Should().Be(expectedGrandTotal);

        // Coupon usage count should increment
        var updatedCoupon = await context.Coupons.FirstAsync(c => c.Code == "SUMMER20");
        updatedCoupon.UsageCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_InsufficientPayment_ShouldThrowBadRequestException()
    {
        // Arrange
        var (context, uow) = await TestDatabaseFactory.CreateSeededContextAsync();
        var currentUser = new MockCurrentUserService { UserId = 1 };
        var handler = new CreateInvoiceCommandHandler(uow, currentUser);

        var branch = await context.Branches.FirstAsync();
        var product = await context.Products.FirstAsync();

        var dto = new CreateInvoiceDto
        {
            BranchId = branch.Id,
            Lines = new List<CreateInvoiceLineDto>
            {
                new()
                {
                    ProductId = product.Id,
                    CustomUnitPrice = 1000m,
                    Quantity = 1
                }
            },
            Payments = new List<CreatePaymentDto>
            {
                new()
                {
                    PaymentMethod = PaymentMethod.Cash,
                    Amount = 500m // Grand total is 1,000 + 14% = 1,140, paying only 500
                }
            }
        };

        // Act & Assert
        var act = async () => await handler.Handle(new CreateInvoiceCommand(dto), CancellationToken.None);
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*Insufficient payment*");
    }

    [Fact]
    public async Task Handle_EmptyLines_ShouldThrowBadRequestException()
    {
        // Arrange
        var (context, uow) = await TestDatabaseFactory.CreateSeededContextAsync();
        var currentUser = new MockCurrentUserService { UserId = 1 };
        var handler = new CreateInvoiceCommandHandler(uow, currentUser);

        var dto = new CreateInvoiceDto
        {
            BranchId = 1,
            Lines = new List<CreateInvoiceLineDto>(),
            Payments = new List<CreatePaymentDto> { new() { PaymentMethod = PaymentMethod.Cash, Amount = 100 } }
        };

        // Act & Assert
        var act = async () => await handler.Handle(new CreateInvoiceCommand(dto), CancellationToken.None);
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*must contain at least one item*");
    }
}
