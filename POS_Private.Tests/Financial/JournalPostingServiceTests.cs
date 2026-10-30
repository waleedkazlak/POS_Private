using POS_Private.Application.Services;
using POS_Private.Domain.Entities;
using POS_Private.Domain.Enums;
using POS_Private.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace POS_Private.Tests.Financial;

public class JournalPostingServiceTests
{
    [Fact]
    public async Task PostVendorBillAsync_ShouldCreateBalancedJournalEntry_AndApplyBalances()
    {
        // Arrange
        var (context, uow) = await TestDatabaseFactory.CreateSeededContextAsync();
        var logger = NullLogger<JournalPostingService>.Instance;
        var service = new JournalPostingService(uow, logger);

        var vendor = await context.Vendors.FirstAsync();
        var branch = await context.Branches.FirstAsync();

        var bill = new VendorBill
        {
            BillNumber = "BILL-TEST-001",
            VendorId = vendor.Id,
            BranchId = branch.Id,
            BillDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            SubTotal = 10000.00m,
            DiscountAmount = 1000.00m, // Net purchases = 9000
            TaxAmount = 1260.00m,     // 14% on 9000
            TotalAmount = 10260.00m,
            RemainingAmount = 10260.00m,
            Status = VendorBillStatus.Approved
        };
        await context.VendorBills.AddAsync(bill);
        await context.SaveChangesAsync();

        var initialInventoryBalance = (await context.Accounts.FirstAsync(a => a.Code == "1104")).CurrentBalance;
        var initialVatInputBalance = (await context.Accounts.FirstAsync(a => a.Code == "1106")).CurrentBalance;
        var initialVendorPayableBalance = (await context.Accounts.FirstAsync(a => a.Code == "2101")).CurrentBalance;

        // Act
        var entry = await service.PostVendorBillAsync(bill, vendor, userId: 1);

        // Assert
        entry.Should().NotBeNull();
        entry!.Status.Should().Be(JournalEntryStatus.Posted);
        entry.SourceType.Should().Be(JournalSourceType.Vendor_Bill);
        entry.ReferenceNumber.Should().Be("BILL-TEST-001");
        entry.TotalDebit.Should().Be(10260.00m);
        entry.TotalCredit.Should().Be(10260.00m);
        entry.Lines.Should().HaveCount(3);

        // Line 1: Dr. Inventory (9,000)
        var invLine = entry.Lines.First(l => l.Account.Code == "1104");
        invLine.Debit.Should().Be(9000.00m);
        invLine.Credit.Should().Be(0m);

        // Line 2: Dr. Input VAT (1,260)
        var vatLine = entry.Lines.First(l => l.Account.Code == "1106");
        vatLine.Debit.Should().Be(1260.00m);
        vatLine.Credit.Should().Be(0m);
        vatLine.ETATaxType.Should().Be(ETATaxType.T1_VAT);

        // Line 3: Cr. Vendor Payable (10,260)
        var payableLine = entry.Lines.First(l => l.Account.Code == "2101");
        payableLine.Debit.Should().Be(0m);
        payableLine.Credit.Should().Be(10260.00m);

        // Check account balance updates
        var updatedInventory = await context.Accounts.FirstAsync(a => a.Code == "1104");
        var updatedVatInput = await context.Accounts.FirstAsync(a => a.Code == "1106");
        var updatedVendorPayable = await context.Accounts.FirstAsync(a => a.Code == "2101");

        updatedInventory.CurrentBalance.Should().Be(initialInventoryBalance + 9000.00m);
        updatedVatInput.CurrentBalance.Should().Be(initialVatInputBalance + 1260.00m);
        // 2101 is Credit nature -> Credit increases balance
        updatedVendorPayable.CurrentBalance.Should().Be(initialVendorPayableBalance + 10260.00m);
    }

    [Fact]
    public async Task PostVendorPaymentAsync_Cash_ShouldDecreasePayableAndCash_Balanced()
    {
        // Arrange
        var (context, uow) = await TestDatabaseFactory.CreateSeededContextAsync();
        var logger = NullLogger<JournalPostingService>.Instance;
        var service = new JournalPostingService(uow, logger);

        var vendor = await context.Vendors.FirstAsync();
        var branch = await context.Branches.FirstAsync();

        var payment = new VendorPayment
        {
            PaymentNumber = "VPAY-TEST-001",
            VendorId = vendor.Id,
            BranchId = branch.Id,
            Amount = 4500.00m,
            PaymentMethod = ExpensePaymentMethod.Cash,
            PaymentDate = DateTime.UtcNow,
            ReferenceNo = "REC-CASH-991"
        };
        await context.VendorPayments.AddAsync(payment);
        await context.SaveChangesAsync();

        var initialCash = (await context.Accounts.FirstAsync(a => a.Code == "1101")).CurrentBalance;
        var initialPayable = (await context.Accounts.FirstAsync(a => a.Code == "2101")).CurrentBalance;

        // Act
        var entry = await service.PostVendorPaymentAsync(payment, vendor, userId: 1);

        // Assert
        entry.Should().NotBeNull();
        entry!.TotalDebit.Should().Be(4500.00m);
        entry.TotalCredit.Should().Be(4500.00m);
        entry.Lines.Should().HaveCount(2);

        // Dr. Vendor Payable (decreases Credit nature account)
        var drLine = entry.Lines.First(l => l.Account.Code == "2101");
        drLine.Debit.Should().Be(4500.00m);

        // Cr. Cash on Hand (decreases Debit nature account)
        var crLine = entry.Lines.First(l => l.Account.Code == "1101");
        crLine.Credit.Should().Be(4500.00m);

        var updatedCash = await context.Accounts.FirstAsync(a => a.Code == "1101");
        var updatedPayable = await context.Accounts.FirstAsync(a => a.Code == "2101");

        updatedCash.CurrentBalance.Should().Be(initialCash - 4500.00m);
        updatedPayable.CurrentBalance.Should().Be(initialPayable - 4500.00m);
    }

    [Fact]
    public async Task PostVendorPaymentAsync_BankTransfer_ShouldCreditBank_Balanced()
    {
        // Arrange
        var (context, uow) = await TestDatabaseFactory.CreateSeededContextAsync();
        var logger = NullLogger<JournalPostingService>.Instance;
        var service = new JournalPostingService(uow, logger);

        var vendor = await context.Vendors.FirstAsync();

        var payment = new VendorPayment
        {
            PaymentNumber = "VPAY-BANK-002",
            VendorId = vendor.Id,
            Amount = 12000.00m,
            PaymentMethod = ExpensePaymentMethod.BankTransfer,
            PaymentDate = DateTime.UtcNow,
            ReferenceNo = "CIB-TRF-88219"
        };
        await context.VendorPayments.AddAsync(payment);
        await context.SaveChangesAsync();

        var initialBank = (await context.Accounts.FirstAsync(a => a.Code == "1102")).CurrentBalance;

        // Act
        var entry = await service.PostVendorPaymentAsync(payment, vendor, userId: 1);

        // Assert
        entry.Should().NotBeNull();
        entry!.Lines.First(l => l.Account.Code == "1102").Credit.Should().Be(12000.00m);

        var updatedBank = await context.Accounts.FirstAsync(a => a.Code == "1102");
        updatedBank.CurrentBalance.Should().Be(initialBank - 12000.00m);
    }

    [Fact]
    public async Task PostShiftSalesSummaryAsync_WithSalesReturnsAndShortage_ShouldBePerfectlyBalanced()
    {
        // Arrange
        var (context, uow) = await TestDatabaseFactory.CreateSeededContextAsync();
        var logger = NullLogger<JournalPostingService>.Instance;
        var service = new JournalPostingService(uow, logger);

        var register = await context.PosRegisters.Include(r => r.Branch).FirstAsync();
        var shift = new CashierShift
        {
            RegisterId = register.Id,
            Register = register,
            CashierUserId = 1,
            OpenedAt = DateTime.UtcNow.AddHours(-8),
            StartingCash = 1000.00m,
            Status = ShiftStatus.Closed
        };
        await context.CashierShifts.AddAsync(shift);
        await context.SaveChangesAsync();

        // Invoices:
        // Invoice 1: Cash sale 1,000 net + 140 VAT = 1,140 total
        var inv1 = new Invoice
        {
            InvoiceNumber = "INV-S1",
            InvoiceType = InvoiceType.Sale,
            Status = InvoiceStatus.Completed,
            SubTotal = 1000.00m,
            DiscountAmount = 0m,
            TaxAmount = 140.00m,
            TotalAmount = 1140.00m,
            PaidAmount = 1140.00m,
            Payments = new List<Payment>
            {
                new() { PaymentMethod = PaymentMethod.Cash, Amount = 1140.00m, PaidAt = DateTime.UtcNow }
            }
        };

        // Invoice 2: Visa sale 2,000 net + 280 VAT = 2,280 total
        var inv2 = new Invoice
        {
            InvoiceNumber = "INV-S2",
            InvoiceType = InvoiceType.Sale,
            Status = InvoiceStatus.Completed,
            SubTotal = 2000.00m,
            DiscountAmount = 0m,
            TaxAmount = 280.00m,
            TotalAmount = 2280.00m,
            PaidAmount = 2280.00m,
            Payments = new List<Payment>
            {
                new() { PaymentMethod = PaymentMethod.Visa, Amount = 2280.00m, PaidAt = DateTime.UtcNow }
            }
        };

        // Invoice 3: Cash return 200 net + 28 VAT = 228 total
        var inv3 = new Invoice
        {
            InvoiceNumber = "INV-R1",
            InvoiceType = InvoiceType.Return,
            Status = InvoiceStatus.Completed,
            SubTotal = 200.00m,
            DiscountAmount = 0m,
            TaxAmount = 28.00m,
            TotalAmount = 228.00m,
            PaidAmount = 228.00m,
            Payments = new List<Payment>
            {
                new() { PaymentMethod = PaymentMethod.Cash, Amount = 228.00m, PaidAt = DateTime.UtcNow }
            }
        };

        var completedInvoices = new List<Invoice> { inv1, inv2, inv3 };

        // Net cash from sales = 1140 - 228 = 912.
        // Expected cash in drawer = 1000 (starting) + 912 = 1912.
        // Actual counted cash = 1862 (diff = -50 shortage).
        decimal actualCash = 1862.00m;
        decimal diff = -50.00m;

        // Act
        var entry = await service.PostShiftSalesSummaryAsync(shift, completedInvoices, actualCash, diff, userId: 1);

        // Assert
        entry.Should().NotBeNull();
        entry!.Status.Should().Be(JournalEntryStatus.Posted);
        entry.SourceType.Should().Be(JournalSourceType.POS_Sale);
        entry.ReferenceNumber.Should().Be($"SHIFT-Z-{shift.Id:D5}");

        // Mathematical balance assertion
        entry.TotalDebit.Should().Be(entry.TotalCredit, "Debits and Credits MUST be precisely balanced in General Ledger");
        entry.TotalDebit.Should().BeGreaterThan(0);

        // Check lines presence:
        // Cash net = 912 (Dr)
        entry.Lines.Should().Contain(l => l.Account.Code == "1101" && l.Debit == 912.00m);
        // Electronic card net = 2280 (Dr)
        entry.Lines.Should().Contain(l => l.Account.Code == "1102" && l.Debit == 2280.00m);
        // Returns gross = 200 (Dr)
        entry.Lines.Should().Contain(l => l.Account.Code == "4103" && l.Debit == 200.00m);
        // VAT reversal on returns = 28 (Dr)
        entry.Lines.Should().Contain(l => l.Account.Code == "2102" && l.Debit == 28.00m);
        // Shortage expense = 50 (Dr)
        entry.Lines.Should().Contain(l => l.Account.Code == "5302" && l.Debit == 50.00m);
        // Shortage cash deduction = 50 (Cr)
        entry.Lines.Should().Contain(l => l.Account.Code == "1101" && l.Credit == 50.00m);
        // Gross sales revenue = 3000 (Cr)
        entry.Lines.Should().Contain(l => l.Account.Code == "4101" && l.Credit == 3000.00m);
        // Sales VAT payable = 420 (Cr)
        entry.Lines.Should().Contain(l => l.Account.Code == "2102" && l.Credit == 420.00m);
    }

    [Theory]
    [InlineData("EXP-RENT", "5201", ExpensePaymentMethod.BankTransfer, "1102")] // Rent -> 5201, Bank -> 1102
    [InlineData("EXP-UTIL", "5203", ExpensePaymentMethod.Cash, "1101")]         // Utilities -> 5203, Cash -> 1101
    [InlineData("EXP-MARKETING", "5202", ExpensePaymentMethod.CreditCard, "1102")] // Marketing -> 5202, Card -> 1102
    [InlineData("EXP-MISC", "5302", ExpensePaymentMethod.DrawerCash, "1101")]   // General -> 5302, Drawer -> 1101
    public async Task PostExpenseAsync_ShouldMapCategoriesAndDisbursementCorrectly(
        string categoryCode,
        string expectedExpenseAccount,
        ExpensePaymentMethod paymentMethod,
        string expectedCreditAccount)
    {
        // Arrange
        var (context, uow) = await TestDatabaseFactory.CreateSeededContextAsync();
        var logger = NullLogger<JournalPostingService>.Instance;
        var service = new JournalPostingService(uow, logger);

        var category = new ExpenseCategory
        {
            Code = categoryCode,
            NameEn = "Category " + categoryCode,
            NameAr = "تصنيف " + categoryCode,
            IsActive = true
        };
        await context.ExpenseCategories.AddAsync(category);
        await context.SaveChangesAsync();

        var expense = new Expense
        {
            ExpenseNumber = "EXP-TEST-100",
            ExpenseCategoryId = category.Id,
            Amount = 1500.00m,
            PaymentMethod = paymentMethod,
            ExpenseDate = DateTime.UtcNow,
            Description = "Test expense posting"
        };
        await context.Expenses.AddAsync(expense);
        await context.SaveChangesAsync();

        // Act
        var entry = await service.PostExpenseAsync(expense, category, userId: 1);

        // Assert
        entry.Should().NotBeNull();
        entry!.TotalDebit.Should().Be(1500.00m);
        entry.TotalCredit.Should().Be(1500.00m);

        var drLine = entry.Lines.First(l => l.Debit == 1500.00m);
        drLine.Account.Code.Should().Be(expectedExpenseAccount);

        var crLine = entry.Lines.First(l => l.Credit == 1500.00m);
        crLine.Account.Code.Should().Be(expectedCreditAccount);
    }
}
