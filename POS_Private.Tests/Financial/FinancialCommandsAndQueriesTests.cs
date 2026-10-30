using POS_Private.Application.Commands.Financial;
using POS_Private.Application.DTOs;
using POS_Private.Application.Queries.Financial;
using POS_Private.Domain.Entities;
using POS_Private.Domain.Enums;
using POS_Private.Tests.Common;
using Microsoft.EntityFrameworkCore;

namespace POS_Private.Tests.Financial;

public class FinancialCommandsAndQueriesTests
{
    [Fact]
    public async Task CreateJournalEntry_UnbalancedDebitsAndCredits_ShouldReturnFailure()
    {
        // Arrange
        var (context, uow) = await TestDatabaseFactory.CreateSeededContextAsync();
        var currentUser = new MockCurrentUserService();
        var handler = new CreateJournalEntryCommandHandler(uow, currentUser);

        var accCash = await context.Accounts.FirstAsync(a => a.Code == "1101");
        var accSales = await context.Accounts.FirstAsync(a => a.Code == "4101");

        var dto = new CreateJournalEntryDto
        {
            EntryDate = DateTime.UtcNow,
            NarrationEn = "Unbalanced test entry",
            NarrationAr = "قيد غير متوازن للاختبار",
            Lines = new List<CreateJournalEntryLineDto>
            {
                new() { AccountId = accCash.Id, Debit = 500.00m, Credit = 0m, Description = "Debit line" },
                new() { AccountId = accSales.Id, Debit = 0m, Credit = 450.00m, Description = "Unbalanced credit line" }
            }
        };

        // Act
        var result = await handler.Handle(new CreateJournalEntryCommand(dto), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorList.Should().Contain(e => e.Contains("out of balance"));
    }

    [Fact]
    public async Task CreateJournalEntry_WithNonPostableControlAccount_ShouldReturnFailure()
    {
        // Arrange
        var (context, uow) = await TestDatabaseFactory.CreateSeededContextAsync();
        var currentUser = new MockCurrentUserService();
        var handler = new CreateJournalEntryCommandHandler(uow, currentUser);

        // "1" Assets is Level 1 non-postable
        var controlAccount = await context.Accounts.FirstAsync(a => a.Code == "1");
        controlAccount.IsPostable.Should().BeFalse();

        var accCash = await context.Accounts.FirstAsync(a => a.Code == "1101");

        var dto = new CreateJournalEntryDto
        {
            EntryDate = DateTime.UtcNow,
            NarrationEn = "Control account test",
            NarrationAr = "اختبار حساب تحكمي",
            Lines = new List<CreateJournalEntryLineDto>
            {
                new() { AccountId = controlAccount.Id, Debit = 1000m, Credit = 0m, Description = "Control account line" },
                new() { AccountId = accCash.Id, Debit = 0m, Credit = 1000m, Description = "Cash line" }
            }
        };

        // Act
        var result = await handler.Handle(new CreateJournalEntryCommand(dto), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorList.Should().Contain(e => e.Contains("control/header account and cannot accept transactions directly"));
    }

    [Fact]
    public async Task CreateJournalEntry_Balanced_WithAutoPost_ShouldPostAndApplyBalances()
    {
        // Arrange
        var (context, uow) = await TestDatabaseFactory.CreateSeededContextAsync();
        var currentUser = new MockCurrentUserService();
        var handler = new CreateJournalEntryCommandHandler(uow, currentUser);

        var accRent = await context.Accounts.FirstAsync(a => a.Code == "5201"); // Nature = Debit
        var accBank = await context.Accounts.FirstAsync(a => a.Code == "1102"); // Nature = Debit

        var initialRent = accRent.CurrentBalance;
        var initialBank = accBank.CurrentBalance;

        var dto = new CreateJournalEntryDto
        {
            EntryDate = DateTime.UtcNow,
            NarrationEn = "Store rent payment",
            NarrationAr = "سداد إيجار الفرع",
            AutoPost = true,
            Lines = new List<CreateJournalEntryLineDto>
            {
                new() { AccountId = accRent.Id, Debit = 8000m, Credit = 0m, Description = "Rent expense" },
                new() { AccountId = accBank.Id, Debit = 0m, Credit = 8000m, Description = "Bank payment" }
            }
        };

        // Act
        var result = await handler.Handle(new CreateJournalEntryCommand(dto), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Status.Should().Be(JournalEntryStatus.Posted);
        result.Data.TotalDebit.Should().Be(8000m);
        result.Data.TotalCredit.Should().Be(8000m);

        // Check balances
        var updatedRent = await context.Accounts.FirstAsync(a => a.Code == "5201");
        var updatedBank = await context.Accounts.FirstAsync(a => a.Code == "1102");

        // Rent (Debit nature) + 8000
        updatedRent.CurrentBalance.Should().Be(initialRent + 8000m);
        // Bank (Debit nature) - 8000
        updatedBank.CurrentBalance.Should().Be(initialBank - 8000m);
    }

    [Fact]
    public async Task VoidJournalEntry_PreviouslyPosted_ShouldAccuratelyReverseBalances()
    {
        // Arrange
        var (context, uow) = await TestDatabaseFactory.CreateSeededContextAsync();
        var currentUser = new MockCurrentUserService();
        var createHandler = new CreateJournalEntryCommandHandler(uow, currentUser);
        var voidHandler = new VoidJournalEntryCommandHandler(uow, currentUser);

        var accRent = await context.Accounts.FirstAsync(a => a.Code == "5201");
        var accBank = await context.Accounts.FirstAsync(a => a.Code == "1102");

        var initialRent = accRent.CurrentBalance;
        var initialBank = accBank.CurrentBalance;

        // Post entry
        var createDto = new CreateJournalEntryDto
        {
            EntryDate = DateTime.UtcNow,
            NarrationEn = "Rent to be voided",
            NarrationAr = "إيجار سيتم إلغاؤه",
            AutoPost = true,
            Lines = new List<CreateJournalEntryLineDto>
            {
                new() { AccountId = accRent.Id, Debit = 5000m, Credit = 0m, Description = "Rent line" },
                new() { AccountId = accBank.Id, Debit = 0m, Credit = 5000m, Description = "Bank line" }
            }
        };
        var createResult = await createHandler.Handle(new CreateJournalEntryCommand(createDto), CancellationToken.None);
        createResult.IsSuccess.Should().BeTrue();
        int entryId = createResult.Data!.Id;

        // Act - Void the posted entry
        var voidResult = await voidHandler.Handle(new VoidJournalEntryCommand(entryId, new VoidJournalEntryDto { Reason = "Payment duplicated by mistake" }), CancellationToken.None);

        // Assert
        voidResult.IsSuccess.Should().BeTrue();

        var voidedEntry = await context.JournalEntries.FirstAsync(j => j.Id == entryId);
        voidedEntry.Status.Should().Be(JournalEntryStatus.Voided);
        voidedEntry.VoidReason.Should().Be("Payment duplicated by mistake");

        // Verify balances returned to exact initial values
        var finalRent = await context.Accounts.FirstAsync(a => a.Code == "5201");
        var finalBank = await context.Accounts.FirstAsync(a => a.Code == "1102");

        finalRent.CurrentBalance.Should().Be(initialRent);
        finalBank.CurrentBalance.Should().Be(initialBank);
    }

    [Fact]
    public async Task GetTrialBalanceQuery_TotalDebitsMustEqualTotalCredits()
    {
        // Arrange
        var (context, uow) = await TestDatabaseFactory.CreateSeededContextAsync();
        var handler = new GetTrialBalanceQueryHandler(uow);

        // Act
        var result = await handler.Handle(new GetTrialBalanceQuery(FromDate: null, ToDate: DateTime.UtcNow, CostCenterId: null), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.IsBalanced.Should().BeTrue("A valid double-entry accounting trial balance MUST be balanced");
        result.Data.TotalPeriodDebit.Should().Be(result.Data.TotalPeriodCredit);
        result.Data.TotalEndingDebit.Should().Be(result.Data.TotalEndingCredit);
        result.Data.Rows.Should().NotBeEmpty();

        // Verify each row's net calculation
        foreach (var row in result.Data.Rows)
        {
            if (row.EndingDebit > 0)
            {
                row.EndingCredit.Should().Be(0);
            }
            if (row.EndingCredit > 0)
            {
                row.EndingDebit.Should().Be(0);
            }
        }
    }

    [Fact]
    public async Task GetAccountLedgerQuery_ShouldCalculateRunningBalancesAccurately()
    {
        // Arrange
        var (context, uow) = await TestDatabaseFactory.CreateSeededContextAsync();
        var handler = new GetAccountLedgerQueryHandler(uow);

        var accCash = await context.Accounts.FirstAsync(a => a.Code == "1101");

        // Act
        var result = await handler.Handle(new GetAccountLedgerQuery(AccountId: accCash.Id, FromDate: null, ToDate: null, CostCenterId: null), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.AccountId.Should().Be(accCash.Id);
        result.Data.AccountCode.Should().Be("1101");

        // Running balance verification
        decimal running = result.Data.OpeningBalance;
        foreach (var tx in result.Data.Transactions)
        {
            if (result.Data.Nature == AccountNature.Debit)
            {
                running += (tx.Debit - tx.Credit);
            }
            else
            {
                running += (tx.Credit - tx.Debit);
            }
            tx.RunningBalance.Should().Be(running);
        }
        result.Data.EndingBalance.Should().Be(running);
    }
}
