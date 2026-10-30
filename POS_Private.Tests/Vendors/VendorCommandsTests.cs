using POS_Private.Application.Commands.Vendor;
using POS_Private.Application.DTOs;
using POS_Private.Application.Services;
using POS_Private.Domain.Entities;
using POS_Private.Domain.Enums;
using POS_Private.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace POS_Private.Tests.Vendors;

public class VendorCommandsTests
{
    [Fact]
    public async Task CreateVendorBill_WithAutoReceiveStock_ShouldUpdateStockBalanceAndPostGL()
    {
        // Arrange
        var (context, uow) = await TestDatabaseFactory.CreateSeededContextAsync();
        var currentUser = new MockCurrentUserService { UserId = 1 };
        var journalPostingService = new JournalPostingService(uow, NullLogger<JournalPostingService>.Instance);
        var handler = new CreateVendorBillCommandHandler(uow, currentUser, journalPostingService);

        var vendor = await context.Vendors.FirstAsync();
        var branch = await context.Branches.FirstAsync();
        var inventory = await context.Inventories.FirstAsync(i => i.BranchId == branch.Id);
        var product = await context.Products.Include(p => p.Variants).FirstAsync();
        var variant = product.Variants.First();

        var existingStock = await context.BranchStocks.FirstAsync(s => s.BranchId == branch.Id && s.ProductId == product.Id && s.VariantId == variant.Id);
        int initialStockQty = existingStock.QuantityOnHand;
        decimal initialVendorBalance = vendor.CurrentBalance;

        // Line 1: 50 units at 100 EGP, 14% tax = 5,000 subtotal, 700 tax, 5,700 total
        var dto = new CreateVendorBillDto
        {
            VendorId = vendor.Id,
            BranchId = branch.Id,
            InventoryId = inventory.Id,
            BillDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            DiscountAmount = 200.00m,
            TaxAmount = 700.00m,
            AutoReceiveStock = true,
            VendorInvoiceNumber = "VINV-998811",
            Lines = new List<CreateVendorBillLineDto>
            {
                new()
                {
                    ProductId = product.Id,
                    VariantId = variant.Id,
                    Quantity = 50,
                    UnitPrice = 100.00m,
                    TaxRate = 14m,
                    Notes = "Summer batch fabric"
                }
            }
        };

        // Expected: SubTotal = 5,000, Tax = 700, Discount = 200, Total = 5,500
        decimal expectedTotal = 5000.00m + 700.00m - 200.00m;

        // Act
        var result = await handler.Handle(new CreateVendorBillCommand(dto), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        int billId = result.Data;

        var bill = await context.VendorBills.Include(b => b.Lines).FirstAsync(b => b.Id == billId);
        bill.SubTotal.Should().Be(5000.00m);
        bill.TaxAmount.Should().Be(700.00m);
        bill.DiscountAmount.Should().Be(200.00m);
        bill.TotalAmount.Should().Be(expectedTotal);
        bill.RemainingAmount.Should().Be(expectedTotal);
        bill.Status.Should().Be(VendorBillStatus.Approved);

        // Vendor balance updated
        var updatedVendor = await context.Vendors.FirstAsync(v => v.Id == vendor.Id);
        updatedVendor.CurrentBalance.Should().Be(initialVendorBalance + expectedTotal);

        // Vendor transaction recorded
        var vendorTx = await context.VendorTransactions.FirstOrDefaultAsync(t => t.ReferenceNumber == bill.BillNumber);
        vendorTx.Should().NotBeNull();
        vendorTx!.CreditAmount.Should().Be(expectedTotal);
        vendorTx.TransactionType.Should().Be(VendorTransactionType.PurchaseBill);

        // Stock updated because AutoReceiveStock == true
        var stock = await context.BranchStocks.FirstAsync(s => s.BranchId == branch.Id && s.ProductId == product.Id && s.VariantId == variant.Id);
        stock.QuantityOnHand.Should().Be(initialStockQty + 50);

        // Automated GL Journal Entry recorded
        var journalEntry = await context.JournalEntries.Include(j => j.Lines).FirstOrDefaultAsync(j => j.ReferenceNumber == bill.BillNumber);
        journalEntry.Should().NotBeNull();
        journalEntry!.SourceType.Should().Be(JournalSourceType.Vendor_Bill);
        journalEntry.TotalDebit.Should().Be(expectedTotal);
        journalEntry.TotalCredit.Should().Be(expectedTotal);
    }

    [Fact]
    public async Task CreateVendorPayment_PartialAndFullPayments_ShouldUpdateBillStatusAndBalance()
    {
        // Arrange
        var (context, uow) = await TestDatabaseFactory.CreateSeededContextAsync();
        var currentUser = new MockCurrentUserService { UserId = 1 };
        var journalPostingService = new JournalPostingService(uow, NullLogger<JournalPostingService>.Instance);
        var paymentHandler = new CreateVendorPaymentCommandHandler(uow, currentUser, journalPostingService);

        var vendor = await context.Vendors.FirstAsync();
        var branch = await context.Branches.FirstAsync();

        // Create a bill with total 10,000 EGP
        var bill = new VendorBill
        {
            BillNumber = "BILL-PAY-01",
            VendorId = vendor.Id,
            BranchId = branch.Id,
            BillDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(15),
            SubTotal = 10000m,
            TotalAmount = 10000m,
            RemainingAmount = 10000m,
            PaidAmount = 0m,
            Status = VendorBillStatus.Approved
        };
        await context.VendorBills.AddAsync(bill);
        vendor.CurrentBalance += 10000m;
        await context.SaveChangesAsync();

        decimal initialVendorBalance = vendor.CurrentBalance;

        // Act 1: Partial payment of 4,000 EGP
        var payDto1 = new CreateVendorPaymentDto
        {
            VendorId = vendor.Id,
            BranchId = branch.Id,
            VendorBillId = bill.Id,
            Amount = 4000.00m,
            PaymentMethod = ExpensePaymentMethod.BankTransfer,
            PaymentDate = DateTime.UtcNow,
            ReferenceNo = "TRF-PARTIAL-1"
        };

        var payResult1 = await paymentHandler.Handle(new CreateVendorPaymentCommand(payDto1), CancellationToken.None);
        payResult1.IsSuccess.Should().BeTrue();

        var billAfterPay1 = await context.VendorBills.FirstAsync(b => b.Id == bill.Id);
        billAfterPay1.PaidAmount.Should().Be(4000.00m);
        billAfterPay1.RemainingAmount.Should().Be(6000.00m);
        billAfterPay1.Status.Should().Be(VendorBillStatus.PartiallyPaid);

        var vendorAfterPay1 = await context.Vendors.FirstAsync(v => v.Id == vendor.Id);
        vendorAfterPay1.CurrentBalance.Should().Be(initialVendorBalance - 4000.00m);

        // Act 2: Final payment of remaining 6,000 EGP
        var payDto2 = new CreateVendorPaymentDto
        {
            VendorId = vendor.Id,
            BranchId = branch.Id,
            VendorBillId = bill.Id,
            Amount = 6000.00m,
            PaymentMethod = ExpensePaymentMethod.Cash,
            PaymentDate = DateTime.UtcNow,
            ReferenceNo = "CASH-FINAL-2"
        };

        var payResult2 = await paymentHandler.Handle(new CreateVendorPaymentCommand(payDto2), CancellationToken.None);
        payResult2.IsSuccess.Should().BeTrue();

        var billAfterPay2 = await context.VendorBills.FirstAsync(b => b.Id == bill.Id);
        billAfterPay2.PaidAmount.Should().Be(10000.00m);
        billAfterPay2.RemainingAmount.Should().Be(0m);
        billAfterPay2.Status.Should().Be(VendorBillStatus.Paid);

        var vendorAfterPay2 = await context.Vendors.FirstAsync(v => v.Id == vendor.Id);
        vendorAfterPay2.CurrentBalance.Should().Be(initialVendorBalance - 10000.00m);
    }
}
