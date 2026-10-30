using POS_Private.Application.Commands.Expense;
using POS_Private.Application.Commands.Shift;
using POS_Private.Application.DTOs;
using POS_Private.Application.Exceptions;
using POS_Private.Application.Services;
using POS_Private.Domain.Entities;
using POS_Private.Domain.Enums;
using POS_Private.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace POS_Private.Tests.Shifts;

public class ShiftAndExpenseCommandsTests
{
    [Fact]
    public async Task OpenShift_WhenRegisterAlreadyHasOpenShift_ShouldThrowBadRequest()
    {
        // Arrange
        var (context, uow) = await TestDatabaseFactory.CreateSeededContextAsync();
        var currentUser = new MockCurrentUserService { UserId = 1 };
        var openHandler = new OpenShiftCommandHandler(uow, currentUser);

        var register = await context.PosRegisters.FirstAsync();

        // First open succeeds
        var openDto1 = new OpenShiftDto { RegisterId = register.Id, StartingCash = 1000m };
        var res1 = await openHandler.Handle(new OpenShiftCommand(openDto1), CancellationToken.None);
        res1.IsSuccess.Should().BeTrue();

        // Second open on same register should fail
        var openDto2 = new OpenShiftDto { RegisterId = register.Id, StartingCash = 500m };
        var act = async () => await openHandler.Handle(new OpenShiftCommand(openDto2), CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*already has an open shift*");
    }

    [Fact]
    public async Task CloseShift_ShouldCalculateZReportCashExpectationsAndVariance()
    {
        // Arrange
        var (context, uow) = await TestDatabaseFactory.CreateSeededContextAsync();
        var currentUser = new MockCurrentUserService { UserId = 1 };
        var journalPostingService = new JournalPostingService(uow, NullLogger<JournalPostingService>.Instance);
        var openHandler = new OpenShiftCommandHandler(uow, currentUser);
        var closeHandler = new CloseShiftCommandHandler(uow, currentUser, journalPostingService);

        var register = await context.PosRegisters.Include(r => r.Branch).FirstAsync();

        // 1. Open shift with 500 EGP starting cash
        var openRes = await openHandler.Handle(new OpenShiftCommand(new OpenShiftDto
        {
            RegisterId = register.Id,
            StartingCash = 500.00m,
            Notes = "Morning shift opening"
        }), CancellationToken.None);
        int shiftId = openRes.Data;

        // 2. Add Invoices to shift:
        // Cash Sale: 1,500 EGP
        var invoice1 = new Invoice
        {
            InvoiceNumber = "INV-SH-01",
            ShiftId = shiftId,
            BranchId = register.BranchId,
            InvoiceType = InvoiceType.Sale,
            Status = InvoiceStatus.Completed,
            TotalAmount = 1500.00m,
            PaidAmount = 1500.00m,
            Lines = new List<InvoiceLine> { new() { ProductName = "Item A", Quantity = 3, TotalLineAmount = 1500m } },
            Payments = new List<Payment>
            {
                new() { PaymentMethod = PaymentMethod.Cash, Amount = 1500.00m, PaidAt = DateTime.UtcNow }
            }
        };

        // Visa Sale: 2,500 EGP
        var invoice2 = new Invoice
        {
            InvoiceNumber = "INV-SH-02",
            ShiftId = shiftId,
            BranchId = register.BranchId,
            InvoiceType = InvoiceType.Sale,
            Status = InvoiceStatus.Completed,
            TotalAmount = 2500.00m,
            PaidAmount = 2500.00m,
            Lines = new List<InvoiceLine> { new() { ProductName = "Item B", Quantity = 5, TotalLineAmount = 2500m } },
            Payments = new List<Payment>
            {
                new() { PaymentMethod = PaymentMethod.Visa, Amount = 2500.00m, PaidAt = DateTime.UtcNow }
            }
        };

        // Cash Return: 300 EGP
        var invoice3 = new Invoice
        {
            InvoiceNumber = "INV-SH-03",
            ShiftId = shiftId,
            BranchId = register.BranchId,
            InvoiceType = InvoiceType.Return,
            Status = InvoiceStatus.Completed,
            TotalAmount = 300.00m,
            PaidAmount = 300.00m,
            Lines = new List<InvoiceLine> { new() { ProductName = "Item C", Quantity = 1, TotalLineAmount = 300m } },
            Payments = new List<Payment>
            {
                new() { PaymentMethod = PaymentMethod.Cash, Amount = 300.00m, PaidAt = DateTime.UtcNow }
            }
        };

        await context.Invoices.AddRangeAsync(invoice1, invoice2, invoice3);

        // 3. Add manual shift transactions:
        // CashIn: 200 EGP float addition
        // CashOut: 150 EGP courier payment
        var txIn = new ShiftTransaction
        {
            ShiftId = shiftId,
            Type = ShiftTransactionType.CashIn,
            Amount = 200.00m,
            Reason = "Extra change float",
            CreatedAt = DateTime.UtcNow
        };
        var txOut = new ShiftTransaction
        {
            ShiftId = shiftId,
            Type = ShiftTransactionType.CashOut,
            Amount = 150.00m,
            Reason = "Delivery boy fuel allowance",
            CreatedAt = DateTime.UtcNow
        };
        await context.ShiftTransactions.AddRangeAsync(txIn, txOut);
        await context.SaveChangesAsync();

        // Calculations:
        // Starting Cash = 500
        // Cash Sales = 1500
        // Cash Returns = 300
        // Net Cash Sales = 1200
        // Cash In = 200
        // Cash Out = 150
        // Expected Cash = 500 + 1500 - 300 + 200 - 150 = 1,750 EGP
        // Cashier counts 1,730 EGP -> difference = -20 EGP (shortage)
        decimal actualCountedCash = 1730.00m;

        // Act
        var closeResult = await closeHandler.Handle(new CloseShiftCommand(new CloseShiftDto
        {
            ShiftId = shiftId,
            ActualCash = actualCountedCash,
            Notes = "Reconciled with 20 EGP shortage"
        }), CancellationToken.None);

        // Assert
        closeResult.IsSuccess.Should().BeTrue();
        var report = closeResult.Data!;

        report.StartingCash.Should().Be(500.00m);
        report.TotalSales.Should().Be(4000.00m);
        report.TotalReturns.Should().Be(300.00m);
        report.TotalCashSales.Should().Be(1500.00m);
        report.TotalVisaSales.Should().Be(2500.00m);
        report.CashInTotal.Should().Be(200.00m);
        report.CashOutTotal.Should().Be(150.00m);
        report.ActualDrawerCash.Should().Be(1730.00m);
        report.Difference.Should().Be(-20.00m);
        report.ItemsSoldCount.Should().Be(8); // 3 + 5

        // Check shift status in DB
        var updatedShift = await context.CashierShifts.FirstAsync(s => s.Id == shiftId);
        updatedShift.Status.Should().Be(ShiftStatus.Closed);
        updatedShift.ExpectedCash.Should().Be(1750.00m);
        updatedShift.Difference.Should().Be(-20.00m);
    }

    [Fact]
    public async Task CreateExpense_FromDrawer_ShouldRecordShiftCashOutAndPostGL()
    {
        // Arrange
        var (context, uow) = await TestDatabaseFactory.CreateSeededContextAsync();
        var currentUser = new MockCurrentUserService { UserId = 1 };
        var journalPostingService = new JournalPostingService(uow, NullLogger<JournalPostingService>.Instance);
        var openHandler = new OpenShiftCommandHandler(uow, currentUser);
        var expenseHandler = new CreateExpenseCommandHandler(uow, currentUser, journalPostingService);

        var register = await context.PosRegisters.FirstAsync();
        var category = await context.ExpenseCategories.FirstAsync();

        // Open an active shift for the user
        var openRes = await openHandler.Handle(new OpenShiftCommand(new OpenShiftDto
        {
            RegisterId = register.Id,
            StartingCash = 1000m
        }), CancellationToken.None);
        int shiftId = openRes.Data;

        var expenseDto = new CreateExpenseDto
        {
            ExpenseCategoryId = category.Id,
            BranchId = register.BranchId,
            Amount = 180.00m,
            ExpenseDate = DateTime.UtcNow,
            PaymentMethod = ExpensePaymentMethod.Cash,
            IsDrawerExpense = true,
            Description = "Office tea & coffee hospitality",
            PaidTo = "Supermarket cashier"
        };

        // Act
        var expenseResult = await expenseHandler.Handle(new CreateExpenseCommand(expenseDto), CancellationToken.None);

        // Assert
        expenseResult.IsSuccess.Should().BeTrue();
        int expenseId = expenseResult.Data;

        var expense = await context.Expenses.FirstAsync(e => e.Id == expenseId);
        expense.Amount.Should().Be(180.00m);
        expense.ShiftId.Should().Be(shiftId);

        // Shift transaction should have been added automatically as CashOut
        var shiftTx = await context.ShiftTransactions.FirstOrDefaultAsync(t => t.ShiftId == shiftId && t.Type == ShiftTransactionType.CashOut && t.Amount == 180m);
        shiftTx.Should().NotBeNull();
        shiftTx!.Reason.Should().Contain(expense.ExpenseNumber);

        // Automated GL Journal Entry recorded
        var journalEntry = await context.JournalEntries.FirstOrDefaultAsync(j => j.ReferenceNumber == expense.ExpenseNumber);
        journalEntry.Should().NotBeNull();
        journalEntry!.SourceType.Should().Be(JournalSourceType.OperationalExpense);
        journalEntry.TotalDebit.Should().Be(180.00m);
        journalEntry.TotalCredit.Should().Be(180.00m);
    }
}
