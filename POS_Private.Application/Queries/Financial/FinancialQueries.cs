using POS_Private.Application.DTOs;
using POS_Private.Domain.Entities;
using POS_Private.Domain.Enums;
using POS_Private.Domain.Interfaces;
using MediatR;

namespace POS_Private.Application.Queries.Financial;

// ==========================================
// Account Queries
// ==========================================

public record GetAccountsQuery(bool? IsActive, AccountType? Type) : IRequest<APIBaseResponse<IEnumerable<AccountDto>>>;

public class GetAccountsQueryHandler : IRequestHandler<GetAccountsQuery, APIBaseResponse<IEnumerable<AccountDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAccountsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<IEnumerable<AccountDto>>> Handle(GetAccountsQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _unitOfWork.Accounts.GetAllAsync(request.IsActive, request.Type);
        var dtos = accounts.Select(a => MapAccountToDto(a)).ToList();
        return APIBaseResponse<IEnumerable<AccountDto>>.Success(dtos);
    }

    private static AccountDto MapAccountToDto(Account a) => new()
    {
        Id = a.Id,
        Code = a.Code,
        NameEn = a.NameEn,
        NameAr = a.NameAr,
        Type = a.Type,
        Nature = a.Nature,
        Level = a.Level,
        IsPostable = a.IsPostable,
        IsActive = a.IsActive,
        Description = a.Description,
        ETATaxType = a.ETATaxType,
        CurrentBalance = a.CurrentBalance,
        ParentAccountId = a.ParentAccountId,
        ParentAccountNameEn = a.ParentAccount?.NameEn,
        ParentAccountNameAr = a.ParentAccount?.NameAr,
        ParentAccountCode = a.ParentAccount?.Code
    };
}

public record GetAccountHierarchyQuery : IRequest<APIBaseResponse<IEnumerable<AccountDto>>>;

public class GetAccountHierarchyQueryHandler : IRequestHandler<GetAccountHierarchyQuery, APIBaseResponse<IEnumerable<AccountDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAccountHierarchyQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<IEnumerable<AccountDto>>> Handle(GetAccountHierarchyQuery request, CancellationToken cancellationToken)
    {
        var allAccounts = (await _unitOfWork.Accounts.GetAllAsync()).ToList();

        // Build tree
        var lookup = allAccounts.ToDictionary(a => a.Id, a => new AccountDto
        {
            Id = a.Id,
            Code = a.Code,
            NameEn = a.NameEn,
            NameAr = a.NameAr,
            Type = a.Type,
            Nature = a.Nature,
            Level = a.Level,
            IsPostable = a.IsPostable,
            IsActive = a.IsActive,
            Description = a.Description,
            ETATaxType = a.ETATaxType,
            CurrentBalance = a.CurrentBalance,
            ParentAccountId = a.ParentAccountId,
            ParentAccountCode = a.ParentAccount?.Code,
            ParentAccountNameEn = a.ParentAccount?.NameEn,
            ParentAccountNameAr = a.ParentAccount?.NameAr,
            Children = new List<AccountDto>()
        });

        var roots = new List<AccountDto>();

        foreach (var acc in allAccounts)
        {
            var dto = lookup[acc.Id];
            if (acc.ParentAccountId.HasValue && lookup.ContainsKey(acc.ParentAccountId.Value))
            {
                lookup[acc.ParentAccountId.Value].Children.Add(dto);
            }
            else
            {
                roots.Add(dto);
            }
        }

        return APIBaseResponse<IEnumerable<AccountDto>>.Success(roots);
    }
}

public record GetAccountByIdQuery(int Id) : IRequest<APIBaseResponse<AccountDto>>;

public class GetAccountByIdQueryHandler : IRequestHandler<GetAccountByIdQuery, APIBaseResponse<AccountDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAccountByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<AccountDto>> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var a = await _unitOfWork.Accounts.GetByIdAsync(request.Id);
        if (a == null)
            return APIBaseResponse<AccountDto>.Failure("Account not found.");

        var dto = new AccountDto
        {
            Id = a.Id,
            Code = a.Code,
            NameEn = a.NameEn,
            NameAr = a.NameAr,
            Type = a.Type,
            Nature = a.Nature,
            Level = a.Level,
            IsPostable = a.IsPostable,
            IsActive = a.IsActive,
            Description = a.Description,
            ETATaxType = a.ETATaxType,
            CurrentBalance = a.CurrentBalance,
            ParentAccountId = a.ParentAccountId,
            ParentAccountNameEn = a.ParentAccount?.NameEn,
            ParentAccountNameAr = a.ParentAccount?.NameAr,
            ParentAccountCode = a.ParentAccount?.Code
        };

        return APIBaseResponse<AccountDto>.Success(dto);
    }
}

// ==========================================
// Cost Center Queries
// ==========================================

public record GetCostCentersQuery(bool? IsActive, int? BranchId) : IRequest<APIBaseResponse<IEnumerable<CostCenterDto>>>;

public class GetCostCentersQueryHandler : IRequestHandler<GetCostCentersQuery, APIBaseResponse<IEnumerable<CostCenterDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCostCentersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<IEnumerable<CostCenterDto>>> Handle(GetCostCentersQuery request, CancellationToken cancellationToken)
    {
        var centers = await _unitOfWork.CostCenters.GetAllAsync(request.IsActive, request.BranchId);
        var dtos = centers.Select(c => new CostCenterDto
        {
            Id = c.Id,
            Code = c.Code,
            NameEn = c.NameEn,
            NameAr = c.NameAr,
            Type = c.Type,
            IsActive = c.IsActive,
            Description = c.Description,
            ParentCostCenterId = c.ParentCostCenterId,
            ParentCostCenterNameEn = c.ParentCostCenter?.NameEn,
            ParentCostCenterNameAr = c.ParentCostCenter?.NameAr,
            BranchId = c.BranchId,
            BranchName = c.Branch?.Name
        });

        return APIBaseResponse<IEnumerable<CostCenterDto>>.Success(dtos);
    }
}

public record GetCostCenterByIdQuery(int Id) : IRequest<APIBaseResponse<CostCenterDto>>;

public class GetCostCenterByIdQueryHandler : IRequestHandler<GetCostCenterByIdQuery, APIBaseResponse<CostCenterDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCostCenterByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<CostCenterDto>> Handle(GetCostCenterByIdQuery request, CancellationToken cancellationToken)
    {
        var c = await _unitOfWork.CostCenters.GetByIdAsync(request.Id);
        if (c == null)
            return APIBaseResponse<CostCenterDto>.Failure("Cost center not found.");

        var dto = new CostCenterDto
        {
            Id = c.Id,
            Code = c.Code,
            NameEn = c.NameEn,
            NameAr = c.NameAr,
            Type = c.Type,
            IsActive = c.IsActive,
            Description = c.Description,
            ParentCostCenterId = c.ParentCostCenterId,
            ParentCostCenterNameEn = c.ParentCostCenter?.NameEn,
            ParentCostCenterNameAr = c.ParentCostCenter?.NameAr,
            BranchId = c.BranchId,
            BranchName = c.Branch?.Name
        };

        return APIBaseResponse<CostCenterDto>.Success(dto);
    }
}

// ==========================================
// Journal Entry Queries
// ==========================================

public record GetJournalEntriesQuery(
    string? SearchTerm,
    JournalEntryStatus? Status,
    JournalSourceType? SourceType,
    DateTime? FromDate,
    DateTime? ToDate,
    int? AccountId,
    int? CostCenterId,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<APIBaseResponse<PaginatedResultDto<JournalEntryDto>>>;

public class GetJournalEntriesQueryHandler : IRequestHandler<GetJournalEntriesQuery, APIBaseResponse<PaginatedResultDto<JournalEntryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetJournalEntriesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<PaginatedResultDto<JournalEntryDto>>> Handle(GetJournalEntriesQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _unitOfWork.JournalEntries.SearchAsync(
            request.SearchTerm,
            request.Status,
            request.SourceType,
            request.FromDate,
            request.ToDate,
            request.AccountId,
            request.CostCenterId,
            request.PageNumber,
            request.PageSize);

        var dtos = items.Select(j => MapToDto(j)).ToList();

        var result = new PaginatedResultDto<JournalEntryDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return APIBaseResponse<PaginatedResultDto<JournalEntryDto>>.Success(result);
    }

    private static JournalEntryDto MapToDto(JournalEntry j) => new()
    {
        Id = j.Id,
        EntryNumber = j.EntryNumber,
        EntryDate = j.EntryDate,
        PostingDate = j.PostingDate,
        Status = j.Status,
        SourceType = j.SourceType,
        ReferenceNumber = j.ReferenceNumber,
        NarrationEn = j.NarrationEn,
        NarrationAr = j.NarrationAr,
        TotalDebit = j.TotalDebit,
        TotalCredit = j.TotalCredit,
        CreatedByUserId = j.CreatedByUserId,
        CreatedByName = j.CreatedByUser?.FullName ?? j.CreatedByUser?.Username,
        ApprovedByUserId = j.ApprovedByUserId,
        ApprovedByName = j.ApprovedByUser?.FullName ?? j.ApprovedByUser?.Username,
        ApprovedAt = j.ApprovedAt,
        VoidReason = j.VoidReason,
        VoidedAt = j.VoidedAt,
        VoidedByName = j.VoidedByUser?.FullName ?? j.VoidedByUser?.Username,
        ETASubmissionStatus = j.ETASubmissionStatus,
        ETAUUID = j.ETAUUID,
        ETALongId = j.ETALongId,
        ETASubmissionDate = j.ETASubmissionDate,
        ETARejectionReason = j.ETARejectionReason,
        BranchTaxCode = j.BranchTaxCode,
        Lines = j.Lines.Select(l => new JournalEntryLineDto
        {
            Id = l.Id,
            JournalEntryId = l.JournalEntryId,
            AccountId = l.AccountId,
            AccountCode = l.Account?.Code ?? "",
            AccountNameEn = l.Account?.NameEn ?? "",
            AccountNameAr = l.Account?.NameAr ?? "",
            CostCenterId = l.CostCenterId,
            CostCenterCode = l.CostCenter?.Code,
            CostCenterNameEn = l.CostCenter?.NameEn,
            CostCenterNameAr = l.CostCenter?.NameAr,
            Debit = l.Debit,
            Credit = l.Credit,
            Description = l.Description,
            ETATaxType = l.ETATaxType,
            TaxRate = l.TaxRate,
            TaxAmount = l.TaxAmount
        }).ToList()
    };
}

public record GetJournalEntryByIdQuery(int Id) : IRequest<APIBaseResponse<JournalEntryDto>>;

public class GetJournalEntryByIdQueryHandler : IRequestHandler<GetJournalEntryByIdQuery, APIBaseResponse<JournalEntryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetJournalEntryByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<JournalEntryDto>> Handle(GetJournalEntryByIdQuery request, CancellationToken cancellationToken)
    {
        var j = await _unitOfWork.JournalEntries.GetByIdAsync(request.Id);
        if (j == null)
            return APIBaseResponse<JournalEntryDto>.Failure("Journal entry not found.");

        var dto = new JournalEntryDto
        {
            Id = j.Id,
            EntryNumber = j.EntryNumber,
            EntryDate = j.EntryDate,
            PostingDate = j.PostingDate,
            Status = j.Status,
            SourceType = j.SourceType,
            ReferenceNumber = j.ReferenceNumber,
            NarrationEn = j.NarrationEn,
            NarrationAr = j.NarrationAr,
            TotalDebit = j.TotalDebit,
            TotalCredit = j.TotalCredit,
            CreatedByUserId = j.CreatedByUserId,
            CreatedByName = j.CreatedByUser?.FullName ?? j.CreatedByUser?.Username,
            ApprovedByUserId = j.ApprovedByUserId,
            ApprovedByName = j.ApprovedByUser?.FullName ?? j.ApprovedByUser?.Username,
            ApprovedAt = j.ApprovedAt,
            VoidReason = j.VoidReason,
            VoidedAt = j.VoidedAt,
            VoidedByName = j.VoidedByUser?.FullName ?? j.VoidedByUser?.Username,
            ETASubmissionStatus = j.ETASubmissionStatus,
            ETAUUID = j.ETAUUID,
            ETALongId = j.ETALongId,
            ETASubmissionDate = j.ETASubmissionDate,
            ETARejectionReason = j.ETARejectionReason,
            BranchTaxCode = j.BranchTaxCode,
            Lines = j.Lines.Select(l => new JournalEntryLineDto
            {
                Id = l.Id,
                JournalEntryId = l.JournalEntryId,
                AccountId = l.AccountId,
                AccountCode = l.Account?.Code ?? "",
                AccountNameEn = l.Account?.NameEn ?? "",
                AccountNameAr = l.Account?.NameAr ?? "",
                CostCenterId = l.CostCenterId,
                CostCenterCode = l.CostCenter?.Code,
                CostCenterNameEn = l.CostCenter?.NameEn,
                CostCenterNameAr = l.CostCenter?.NameAr,
                Debit = l.Debit,
                Credit = l.Credit,
                Description = l.Description,
                ETATaxType = l.ETATaxType,
                TaxRate = l.TaxRate,
                TaxAmount = l.TaxAmount
            }).ToList()
        };

        return APIBaseResponse<JournalEntryDto>.Success(dto);
    }
}

// ==========================================
// Financial Reports Queries
// ==========================================

public record GetTrialBalanceQuery(DateTime? FromDate, DateTime? ToDate, int? CostCenterId) : IRequest<APIBaseResponse<TrialBalanceReportDto>>;

public class GetTrialBalanceQueryHandler : IRequestHandler<GetTrialBalanceQuery, APIBaseResponse<TrialBalanceReportDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTrialBalanceQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<TrialBalanceReportDto>> Handle(GetTrialBalanceQuery request, CancellationToken cancellationToken)
    {
        var fromDate = request.FromDate ?? new DateTime(DateTime.UtcNow.Year, 1, 1);
        var toDate = request.ToDate ?? DateTime.UtcNow;

        var accounts = (await _unitOfWork.Accounts.GetAllAsync(isActive: true)).OrderBy(a => a.Code).ToList();

        // Fetch all lines from POSTED journal entries up to toDate
        var lines = (await _unitOfWork.JournalEntries.GetLinesWithEntryAsync(
            status: JournalEntryStatus.Posted,
            toDate: toDate,
            costCenterId: request.CostCenterId)).ToList();

        var report = new TrialBalanceReportDto
        {
            FromDate = fromDate,
            ToDate = toDate,
            CostCenterId = request.CostCenterId
        };

        if (request.CostCenterId.HasValue)
        {
            var cc = await _unitOfWork.CostCenters.GetByIdAsync(request.CostCenterId.Value);
            report.CostCenterName = cc?.NameEn;
        }

        foreach (var acc in accounts)
        {
            var accLines = lines.Where(l => l.AccountId == acc.Id).ToList();

            // Opening movement: before FromDate
            var openingDebits = accLines.Where(l => l.JournalEntry.EntryDate < fromDate).Sum(l => l.Debit);
            var openingCredits = accLines.Where(l => l.JournalEntry.EntryDate < fromDate).Sum(l => l.Credit);

            decimal openDebit = 0m;
            decimal openCredit = 0m;
            if (acc.Nature == AccountNature.Debit)
            {
                var net = openingDebits - openingCredits;
                if (net >= 0) openDebit = net;
                else openCredit = -net;
            }
            else
            {
                var net = openingCredits - openingDebits;
                if (net >= 0) openCredit = net;
                else openDebit = -net;
            }

            // Period movement: between FromDate and ToDate
            var periodDebit = accLines.Where(l => l.JournalEntry.EntryDate >= fromDate && l.JournalEntry.EntryDate <= toDate).Sum(l => l.Debit);
            var periodCredit = accLines.Where(l => l.JournalEntry.EntryDate >= fromDate && l.JournalEntry.EntryDate <= toDate).Sum(l => l.Credit);

            // Ending balance
            var allDebits = accLines.Where(l => l.JournalEntry.EntryDate <= toDate).Sum(l => l.Debit);
            var allCredits = accLines.Where(l => l.JournalEntry.EntryDate <= toDate).Sum(l => l.Credit);

            decimal endDebit = 0m;
            decimal endCredit = 0m;
            if (acc.Nature == AccountNature.Debit)
            {
                var net = allDebits - allCredits;
                if (net >= 0) endDebit = net;
                else endCredit = -net;
            }
            else
            {
                var net = allCredits - allDebits;
                if (net >= 0) endCredit = net;
                else endDebit = -net;
            }

            // Include if there's any balance or movement, or if leaf postable
            if (openDebit > 0 || openCredit > 0 || periodDebit > 0 || periodCredit > 0 || endDebit > 0 || endCredit > 0 || acc.IsPostable)
            {
                report.Rows.Add(new TrialBalanceRowDto
                {
                    AccountId = acc.Id,
                    AccountCode = acc.Code,
                    AccountNameEn = acc.NameEn,
                    AccountNameAr = acc.NameAr,
                    Type = acc.Type,
                    Nature = acc.Nature,
                    Level = acc.Level,
                    IsPostable = acc.IsPostable,
                    OpeningDebit = openDebit,
                    OpeningCredit = openCredit,
                    PeriodDebit = periodDebit,
                    PeriodCredit = periodCredit,
                    EndingDebit = endDebit,
                    EndingCredit = endCredit
                });
            }
        }

        // Totals for postable accounts only to prevent double counting parent nodes
        var postableRows = report.Rows.Where(r => r.IsPostable).ToList();
        report.TotalOpeningDebit = postableRows.Sum(r => r.OpeningDebit);
        report.TotalOpeningCredit = postableRows.Sum(r => r.OpeningCredit);
        report.TotalPeriodDebit = postableRows.Sum(r => r.PeriodDebit);
        report.TotalPeriodCredit = postableRows.Sum(r => r.PeriodCredit);
        report.TotalEndingDebit = postableRows.Sum(r => r.EndingDebit);
        report.TotalEndingCredit = postableRows.Sum(r => r.EndingCredit);

        return APIBaseResponse<TrialBalanceReportDto>.Success(report);
    }
}

public record GetAccountLedgerQuery(int AccountId, DateTime? FromDate, DateTime? ToDate, int? CostCenterId) : IRequest<APIBaseResponse<AccountLedgerReportDto>>;

public class GetAccountLedgerQueryHandler : IRequestHandler<GetAccountLedgerQuery, APIBaseResponse<AccountLedgerReportDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAccountLedgerQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<AccountLedgerReportDto>> Handle(GetAccountLedgerQuery request, CancellationToken cancellationToken)
    {
        var acc = await _unitOfWork.Accounts.GetByIdAsync(request.AccountId);
        if (acc == null)
            return APIBaseResponse<AccountLedgerReportDto>.Failure("Account not found.");

        var fromDate = request.FromDate ?? new DateTime(DateTime.UtcNow.Year, 1, 1);
        var toDate = request.ToDate ?? DateTime.UtcNow;

        var allLines = (await _unitOfWork.JournalEntries.GetLinesWithEntryAsync(
            status: JournalEntryStatus.Posted,
            accountId: request.AccountId,
            costCenterId: request.CostCenterId)).ToList();

        // Opening balance calculation
        var priorLines = allLines.Where(l => l.JournalEntry.EntryDate < fromDate).ToList();
        decimal openingBalance = acc.Nature == AccountNature.Debit
            ? priorLines.Sum(l => l.Debit) - priorLines.Sum(l => l.Credit)
            : priorLines.Sum(l => l.Credit) - priorLines.Sum(l => l.Debit);

        // Period lines
        var periodLines = allLines
            .Where(l => l.JournalEntry.EntryDate >= fromDate && l.JournalEntry.EntryDate <= toDate)
            .OrderBy(l => l.JournalEntry.EntryDate)
            .ThenBy(l => l.JournalEntry.Id)
            .ToList();

        decimal running = openingBalance;
        var rows = new List<AccountLedgerRowDto>();

        foreach (var l in periodLines)
        {
            if (acc.Nature == AccountNature.Debit)
            {
                running += (l.Debit - l.Credit);
            }
            else
            {
                running += (l.Credit - l.Debit);
            }

            rows.Add(new AccountLedgerRowDto
            {
                JournalEntryId = l.JournalEntryId,
                EntryNumber = l.JournalEntry.EntryNumber,
                EntryDate = l.JournalEntry.EntryDate,
                ReferenceNumber = l.JournalEntry.ReferenceNumber,
                NarrationEn = l.Description ?? l.JournalEntry.NarrationEn,
                NarrationAr = l.Description ?? l.JournalEntry.NarrationAr,
                CostCenterCode = l.CostCenter?.Code,
                CostCenterName = l.CostCenter?.NameEn,
                Debit = l.Debit,
                Credit = l.Credit,
                RunningBalance = running
            });
        }

        var report = new AccountLedgerReportDto
        {
            AccountId = acc.Id,
            AccountCode = acc.Code,
            AccountNameEn = acc.NameEn,
            AccountNameAr = acc.NameAr,
            Type = acc.Type,
            Nature = acc.Nature,
            FromDate = fromDate,
            ToDate = toDate,
            OpeningBalance = openingBalance,
            TotalDebits = periodLines.Sum(l => l.Debit),
            TotalCredits = periodLines.Sum(l => l.Credit),
            EndingBalance = running,
            Transactions = rows
        };

        return APIBaseResponse<AccountLedgerReportDto>.Success(report);
    }
}

public record GetCostCenterIncomeStatementQuery(DateTime? FromDate, DateTime? ToDate) : IRequest<APIBaseResponse<CostCenterIncomeStatementDto>>;

public class GetCostCenterIncomeStatementQueryHandler : IRequestHandler<GetCostCenterIncomeStatementQuery, APIBaseResponse<CostCenterIncomeStatementDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCostCenterIncomeStatementQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<CostCenterIncomeStatementDto>> Handle(GetCostCenterIncomeStatementQuery request, CancellationToken cancellationToken)
    {
        var fromDate = request.FromDate ?? new DateTime(DateTime.UtcNow.Year, 1, 1);
        var toDate = request.ToDate ?? DateTime.UtcNow;

        var costCenters = (await _unitOfWork.CostCenters.GetAllAsync(isActive: true)).OrderBy(c => c.Code).ToList();

        var lines = (await _unitOfWork.JournalEntries.GetLinesWithEntryAsync(
            status: JournalEntryStatus.Posted,
            fromDate: fromDate,
            toDate: toDate)).ToList();

        var report = new CostCenterIncomeStatementDto
        {
            FromDate = fromDate,
            ToDate = toDate
        };

        foreach (var cc in costCenters)
        {
            var ccLines = lines.Where(l => l.CostCenterId == cc.Id).ToList();

            // Revenue accounts: net credit (Credit - Debit)
            var revenues = ccLines.Where(l => l.Account.Type == AccountType.Revenue).Sum(l => l.Credit - l.Debit);
            // Expense accounts: net debit (Debit - Credit)
            var costs = ccLines.Where(l => l.Account.Type == AccountType.Expense).Sum(l => l.Debit - l.Credit);

            report.CostCenters.Add(new CostCenterProfitabilityDto
            {
                CostCenterId = cc.Id,
                CostCenterCode = cc.Code,
                CostCenterNameEn = cc.NameEn,
                CostCenterNameAr = cc.NameAr,
                Type = cc.Type,
                TotalRevenues = Math.Max(0, revenues),
                TotalCosts = Math.Max(0, costs)
            });
        }

        report.TotalCompanyRevenue = report.CostCenters.Sum(c => c.TotalRevenues);
        report.TotalCompanyCost = report.CostCenters.Sum(c => c.TotalCosts);

        return APIBaseResponse<CostCenterIncomeStatementDto>.Success(report);
    }
}

public record GetETATaxSummaryQuery(DateTime? FromDate, DateTime? ToDate) : IRequest<APIBaseResponse<ETATaxSummaryReportDto>>;

public class GetETATaxSummaryQueryHandler : IRequestHandler<GetETATaxSummaryQuery, APIBaseResponse<ETATaxSummaryReportDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetETATaxSummaryQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<ETATaxSummaryReportDto>> Handle(GetETATaxSummaryQuery request, CancellationToken cancellationToken)
    {
        var fromDate = request.FromDate ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var toDate = request.ToDate ?? DateTime.UtcNow;

        var etaSetting = await _unitOfWork.ETASettings.GetSettingAsync();

        var postedLines = (await _unitOfWork.JournalEntries.GetLinesWithEntryAsync(
            status: JournalEntryStatus.Posted,
            fromDate: fromDate,
            toDate: toDate)).ToList();

        // Gross sales from revenue accounts
        var grossSales = postedLines.Where(l => l.Account.Type == AccountType.Revenue).Sum(l => l.Credit - l.Debit);

        // Output VAT (T1 Credit from tax payable account or line tax)
        var outputVat = postedLines.Where(l => l.ETATaxType == ETATaxType.T1_VAT && l.Credit > 0).Sum(l => l.Credit);
        if (outputVat == 0m)
        {
            // Fallback to 2102 account credit lines
            outputVat = postedLines.Where(l => l.Account.Code == "2102").Sum(l => l.Credit);
        }

        // Input VAT (T1 Debit from input vat receivable account)
        var inputVat = postedLines.Where(l => l.ETATaxType == ETATaxType.T1_VAT && l.Debit > 0).Sum(l => l.Debit);
        if (inputVat == 0m)
        {
            inputVat = postedLines.Where(l => l.Account.Code == "1106").Sum(l => l.Debit);
        }

        // WHT 1%
        var wht = postedLines.Where(l => l.ETATaxType == ETATaxType.T4_WithholdingTax).Sum(l => l.Credit > 0 ? l.Credit : l.Debit);
        if (wht == 0m)
        {
            wht = postedLines.Where(l => l.Account.Code == "2103" || l.Account.Code == "1107").Sum(l => l.Credit > 0 ? l.Credit : l.Debit);
        }

        var report = new ETATaxSummaryReportDto
        {
            FromDate = fromDate,
            ToDate = toDate,
            TaxRegistrationNumber = etaSetting.TaxRegistrationNumber,
            ActivityCode = etaSetting.ActivityCode,
            CompanyName = etaSetting.CompanyName,
            GrossSales = Math.Max(0, grossSales),
            TotalVatCollected = outputVat,
            TotalVatPaidOnPurchases = inputVat,
            TotalWithholdingTaxDeducted = wht,
            TaxBreakdowns = new List<ETATaxLineSummaryDto>
            {
                new()
                {
                    TaxType = ETATaxType.T1_VAT,
                    TaxNameEn = "Value Added Tax (VAT 14%)",
                    TaxNameAr = "ضريبة القيمة المضافة (14%)",
                    TaxRate = 14.0m,
                    TaxableAmount = Math.Max(0, grossSales),
                    TotalSalesTaxCollected = outputVat,
                    TotalPurchaseTaxDeducted = inputVat,
                    CalculatedTax = outputVat - inputVat
                },
                new()
                {
                    TaxType = ETATaxType.T4_WithholdingTax,
                    TaxNameEn = "Withholding Tax (WHT 1%)",
                    TaxNameAr = "ضريبة الخصم والإضافة (1%)",
                    TaxRate = 1.0m,
                    TaxableAmount = Math.Max(0, grossSales),
                    TotalSalesTaxCollected = wht,
                    TotalPurchaseTaxDeducted = 0m,
                    CalculatedTax = wht
                }
            }
        };

        return APIBaseResponse<ETATaxSummaryReportDto>.Success(report);
    }
}

public record GetETACompanySettingQuery : IRequest<APIBaseResponse<ETACompanySettingDto>>;

public class GetETACompanySettingQueryHandler : IRequestHandler<GetETACompanySettingQuery, APIBaseResponse<ETACompanySettingDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetETACompanySettingQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<ETACompanySettingDto>> Handle(GetETACompanySettingQuery request, CancellationToken cancellationToken)
    {
        var setting = await _unitOfWork.ETASettings.GetSettingAsync();
        var dto = new ETACompanySettingDto
        {
            Id = setting.Id,
            TaxRegistrationNumber = setting.TaxRegistrationNumber,
            ActivityCode = setting.ActivityCode,
            CompanyName = setting.CompanyName,
            BranchTaxCode = setting.BranchTaxCode,
            ClientId = setting.ClientId,
            ClientSecret = setting.ClientSecret,
            TokenPin = setting.TokenPin,
            IsProduction = setting.IsProduction,
            AutoSubmitInvoices = setting.AutoSubmitInvoices,
            DefaultVatRate = setting.DefaultVatRate,
            DefaultWhtRate = setting.DefaultWhtRate
        };

        return APIBaseResponse<ETACompanySettingDto>.Success(dto);
    }
}
