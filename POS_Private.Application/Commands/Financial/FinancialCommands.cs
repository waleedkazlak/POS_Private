using POS_Private.Application.DTOs;
using POS_Private.Application.Services;
using POS_Private.Domain.Entities;
using POS_Private.Domain.Enums;
using POS_Private.Domain.Interfaces;
using MediatR;

namespace POS_Private.Application.Commands.Financial;

// ==========================================
// Account Commands
// ==========================================

public record CreateAccountCommand(CreateAccountDto Dto) : IRequest<APIBaseResponse<int>>;

public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, APIBaseResponse<int>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateAccountCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<int>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var existing = await _unitOfWork.Accounts.GetByCodeAsync(request.Dto.Code.Trim());
        if (existing != null)
            return APIBaseResponse<int>.Failure("An account with this code already exists.");

        int level = 1;
        if (request.Dto.ParentAccountId.HasValue)
        {
            var parent = await _unitOfWork.Accounts.GetByIdAsync(request.Dto.ParentAccountId.Value);
            if (parent != null)
            {
                level = parent.Level + 1;
            }
        }

        var account = new Account
        {
            Code = request.Dto.Code.Trim(),
            NameEn = request.Dto.NameEn.Trim(),
            NameAr = request.Dto.NameAr.Trim(),
            Type = request.Dto.Type,
            Nature = request.Dto.Nature,
            Level = level,
            IsPostable = request.Dto.IsPostable,
            IsActive = true,
            Description = request.Dto.Description,
            ETATaxType = request.Dto.ETATaxType,
            ParentAccountId = request.Dto.ParentAccountId,
            CurrentBalance = 0m
        };

        var id = await _unitOfWork.Accounts.AddAsync(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return APIBaseResponse<int>.Success(id, "Account created successfully.");
    }
}

public record UpdateAccountCommand(int Id, UpdateAccountDto Dto) : IRequest<APIBaseResponse<bool>>;

public class UpdateAccountCommandHandler : IRequestHandler<UpdateAccountCommand, APIBaseResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAccountCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<bool>> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _unitOfWork.Accounts.GetByIdAsync(request.Id);
        if (account == null)
            return APIBaseResponse<bool>.Failure("Account not found.");

        account.NameEn = request.Dto.NameEn.Trim();
        account.NameAr = request.Dto.NameAr.Trim();
        account.Nature = request.Dto.Nature;
        account.IsPostable = request.Dto.IsPostable;
        account.IsActive = request.Dto.IsActive;
        account.Description = request.Dto.Description;
        account.ETATaxType = request.Dto.ETATaxType;
        account.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Accounts.UpdateAsync(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return APIBaseResponse<bool>.Success(true, "Account updated successfully.");
    }
}

public record DeleteAccountCommand(int Id) : IRequest<APIBaseResponse<bool>>;

public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand, APIBaseResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAccountCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<bool>> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _unitOfWork.Accounts.GetByIdAsync(request.Id);
        if (account == null)
            return APIBaseResponse<bool>.Failure("Account not found.");

        if (account.ChildrenAccounts.Any())
            return APIBaseResponse<bool>.Failure("Cannot delete account with existing child accounts.");

        if (account.JournalLines.Any() || account.CurrentBalance != 0m)
            return APIBaseResponse<bool>.Failure("Cannot delete account with existing transactions or non-zero balance.");

        await _unitOfWork.Accounts.DeleteAsync(request.Id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return APIBaseResponse<bool>.Success(true, "Account deleted successfully.");
    }
}

// ==========================================
// Cost Center Commands
// ==========================================

public record CreateCostCenterCommand(CreateCostCenterDto Dto) : IRequest<APIBaseResponse<int>>;

public class CreateCostCenterCommandHandler : IRequestHandler<CreateCostCenterCommand, APIBaseResponse<int>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateCostCenterCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<int>> Handle(CreateCostCenterCommand request, CancellationToken cancellationToken)
    {
        var existing = await _unitOfWork.CostCenters.GetByCodeAsync(request.Dto.Code.Trim());
        if (existing != null)
            return APIBaseResponse<int>.Failure("Cost center code already exists.");

        var cc = new CostCenter
        {
            Code = request.Dto.Code.Trim(),
            NameEn = request.Dto.NameEn.Trim(),
            NameAr = request.Dto.NameAr.Trim(),
            Type = request.Dto.Type,
            ParentCostCenterId = request.Dto.ParentCostCenterId,
            BranchId = request.Dto.BranchId,
            Description = request.Dto.Description,
            IsActive = true
        };

        var id = await _unitOfWork.CostCenters.AddAsync(cc);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return APIBaseResponse<int>.Success(id, "Cost center created successfully.");
    }
}

public record UpdateCostCenterCommand(int Id, UpdateCostCenterDto Dto) : IRequest<APIBaseResponse<bool>>;

public class UpdateCostCenterCommandHandler : IRequestHandler<UpdateCostCenterCommand, APIBaseResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCostCenterCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<bool>> Handle(UpdateCostCenterCommand request, CancellationToken cancellationToken)
    {
        var cc = await _unitOfWork.CostCenters.GetByIdAsync(request.Id);
        if (cc == null)
            return APIBaseResponse<bool>.Failure("Cost center not found.");

        cc.NameEn = request.Dto.NameEn.Trim();
        cc.NameAr = request.Dto.NameAr.Trim();
        cc.Type = request.Dto.Type;
        cc.ParentCostCenterId = request.Dto.ParentCostCenterId;
        cc.BranchId = request.Dto.BranchId;
        cc.IsActive = request.Dto.IsActive;
        cc.Description = request.Dto.Description;
        cc.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.CostCenters.UpdateAsync(cc);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return APIBaseResponse<bool>.Success(true, "Cost center updated successfully.");
    }
}

public record DeleteCostCenterCommand(int Id) : IRequest<APIBaseResponse<bool>>;

public class DeleteCostCenterCommandHandler : IRequestHandler<DeleteCostCenterCommand, APIBaseResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCostCenterCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<bool>> Handle(DeleteCostCenterCommand request, CancellationToken cancellationToken)
    {
        var cc = await _unitOfWork.CostCenters.GetByIdAsync(request.Id);
        if (cc == null)
            return APIBaseResponse<bool>.Failure("Cost center not found.");

        if (cc.JournalLines.Any())
            return APIBaseResponse<bool>.Failure("Cannot delete cost center assigned to existing journal entry lines.");

        await _unitOfWork.CostCenters.DeleteAsync(request.Id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return APIBaseResponse<bool>.Success(true, "Cost center deleted successfully.");
    }
}

// ==========================================
// Journal Entry Commands
// ==========================================

public record CreateJournalEntryCommand(CreateJournalEntryDto Dto) : IRequest<APIBaseResponse<JournalEntryDto>>;

public class CreateJournalEntryCommandHandler : IRequestHandler<CreateJournalEntryCommand, APIBaseResponse<JournalEntryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public CreateJournalEntryCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<APIBaseResponse<JournalEntryDto>> Handle(CreateJournalEntryCommand request, CancellationToken cancellationToken)
    {
        if (request.Dto.Lines == null || request.Dto.Lines.Count < 2)
            return APIBaseResponse<JournalEntryDto>.Failure("A journal entry requires at least 2 lines (debit and credit).");

        decimal totalDebit = request.Dto.Lines.Sum(l => l.Debit);
        decimal totalCredit = request.Dto.Lines.Sum(l => l.Credit);

        if (Math.Abs(totalDebit - totalCredit) > 0.001m)
            return APIBaseResponse<JournalEntryDto>.Failure($"Journal entry is out of balance. Total Debit ({totalDebit:F2}) must equal Total Credit ({totalCredit:F2}).");

        if (totalDebit <= 0)
            return APIBaseResponse<JournalEntryDto>.Failure("Total amount must be greater than zero.");

        // Validate accounts are postable
        foreach (var line in request.Dto.Lines)
        {
            var account = await _unitOfWork.Accounts.GetByIdAsync(line.AccountId);
            if (account == null)
                return APIBaseResponse<JournalEntryDto>.Failure($"Account ID {line.AccountId} does not exist.");
            if (!account.IsPostable)
                return APIBaseResponse<JournalEntryDto>.Failure($"Account '{account.Code} - {account.NameEn}' is a control/header account and cannot accept transactions directly.");
        }

        var entryNumber = await _unitOfWork.JournalEntries.GenerateNextEntryNumberAsync();
        var currentUserId = _currentUser.UserId;

        var entry = new JournalEntry
        {
            EntryNumber = entryNumber,
            EntryDate = request.Dto.EntryDate,
            Status = JournalEntryStatus.Draft,
            SourceType = request.Dto.SourceType,
            ReferenceNumber = request.Dto.ReferenceNumber,
            NarrationEn = request.Dto.NarrationEn.Trim(),
            NarrationAr = request.Dto.NarrationAr.Trim(),
            TotalDebit = totalDebit,
            TotalCredit = totalCredit,
            CreatedByUserId = currentUserId,
            BranchTaxCode = request.Dto.BranchTaxCode ?? "0",
            ETASubmissionStatus = ETASubmissionStatus.NotApplicable
        };

        foreach (var line in request.Dto.Lines)
        {
            entry.Lines.Add(new JournalEntryLine
            {
                AccountId = line.AccountId,
                CostCenterId = line.CostCenterId,
                Debit = line.Debit,
                Credit = line.Credit,
                Description = line.Description,
                ETATaxType = line.ETATaxType,
                TaxRate = line.TaxRate,
                TaxAmount = line.TaxAmount
            });
        }

        await _unitOfWork.JournalEntries.AddAsync(entry);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // If auto-post requested
        if (request.Dto.AutoPost)
        {
            await PostEntryInternalAsync(entry, currentUserId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var result = await _unitOfWork.JournalEntries.GetByIdAsync(entry.Id);
        return APIBaseResponse<JournalEntryDto>.Success(MapToDto(result!), "Journal entry created successfully.");
    }

    private async Task PostEntryInternalAsync(JournalEntry entry, int? userId)
    {
        entry.Status = JournalEntryStatus.Posted;
        entry.PostingDate = DateTime.UtcNow;
        entry.ApprovedByUserId = userId;
        entry.ApprovedAt = DateTime.UtcNow;

        foreach (var line in entry.Lines)
        {
            var account = await _unitOfWork.Accounts.GetByIdAsync(line.AccountId);
            if (account != null)
            {
                decimal delta = 0m;
                if (account.Nature == AccountNature.Debit)
                {
                    delta = line.Debit - line.Credit;
                }
                else
                {
                    delta = line.Credit - line.Debit;
                }
                account.CurrentBalance += delta;
                account.UpdatedAt = DateTime.UtcNow;
            }
        }
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

public record PostJournalEntryCommand(int Id) : IRequest<APIBaseResponse<JournalEntryDto>>;

public class PostJournalEntryCommandHandler : IRequestHandler<PostJournalEntryCommand, APIBaseResponse<JournalEntryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public PostJournalEntryCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<APIBaseResponse<JournalEntryDto>> Handle(PostJournalEntryCommand request, CancellationToken cancellationToken)
    {
        var entry = await _unitOfWork.JournalEntries.GetByIdAsync(request.Id);
        if (entry == null)
            return APIBaseResponse<JournalEntryDto>.Failure("Journal entry not found.");

        if (entry.Status != JournalEntryStatus.Draft)
            return APIBaseResponse<JournalEntryDto>.Failure($"Only draft entries can be posted. Current status is {entry.Status}.");

        if (Math.Abs(entry.TotalDebit - entry.TotalCredit) > 0.001m)
            return APIBaseResponse<JournalEntryDto>.Failure($"Cannot post unbalanced entry. Debit: {entry.TotalDebit}, Credit: {entry.TotalCredit}.");

        entry.Status = JournalEntryStatus.Posted;
        entry.PostingDate = DateTime.UtcNow;
        entry.ApprovedByUserId = _currentUser.UserId;
        entry.ApprovedAt = DateTime.UtcNow;

        // Apply balances to accounts
        foreach (var line in entry.Lines)
        {
            var account = await _unitOfWork.Accounts.GetByIdAsync(line.AccountId);
            if (account != null)
            {
                decimal delta = account.Nature == AccountNature.Debit
                    ? line.Debit - line.Credit
                    : line.Credit - line.Debit;

                account.CurrentBalance += delta;
                account.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _unitOfWork.JournalEntries.GetByIdAsync(entry.Id);
        return APIBaseResponse<JournalEntryDto>.Success(FinancialMapper.ToJournalEntryDto(updated!), "Journal entry approved and posted successfully.");
    }
}

public record VoidJournalEntryCommand(int Id, VoidJournalEntryDto Dto) : IRequest<APIBaseResponse<bool>>;

public class VoidJournalEntryCommandHandler : IRequestHandler<VoidJournalEntryCommand, APIBaseResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public VoidJournalEntryCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<APIBaseResponse<bool>> Handle(VoidJournalEntryCommand request, CancellationToken cancellationToken)
    {
        var entry = await _unitOfWork.JournalEntries.GetByIdAsync(request.Id);
        if (entry == null)
            return APIBaseResponse<bool>.Failure("Journal entry not found.");

        if (entry.Status == JournalEntryStatus.Voided)
            return APIBaseResponse<bool>.Failure("Entry is already voided.");

        // If previously posted, reverse account balances
        if (entry.Status == JournalEntryStatus.Posted)
        {
            foreach (var line in entry.Lines)
            {
                var account = await _unitOfWork.Accounts.GetByIdAsync(line.AccountId);
                if (account != null)
                {
                    decimal delta = account.Nature == AccountNature.Debit
                        ? line.Debit - line.Credit
                        : line.Credit - line.Debit;

                    // Reversal subtracts previously applied delta
                    account.CurrentBalance -= delta;
                    account.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        entry.Status = JournalEntryStatus.Voided;
        entry.VoidReason = request.Dto.Reason.Trim();
        entry.VoidedAt = DateTime.UtcNow;
        entry.VoidedByUserId = _currentUser.UserId;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return APIBaseResponse<bool>.Success(true, "Journal entry has been voided and ledger balances reversed.");
    }
}

// ==========================================
// ETA Integration Commands
// ==========================================

public record UpdateETACompanySettingCommand(UpdateETACompanySettingDto Dto) : IRequest<APIBaseResponse<ETACompanySettingDto>>;

public class UpdateETACompanySettingCommandHandler : IRequestHandler<UpdateETACompanySettingCommand, APIBaseResponse<ETACompanySettingDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateETACompanySettingCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<ETACompanySettingDto>> Handle(UpdateETACompanySettingCommand request, CancellationToken cancellationToken)
    {
        var setting = await _unitOfWork.ETASettings.GetSettingAsync();

        setting.TaxRegistrationNumber = request.Dto.TaxRegistrationNumber.Trim();
        setting.ActivityCode = request.Dto.ActivityCode.Trim();
        setting.CompanyName = request.Dto.CompanyName.Trim();
        setting.BranchTaxCode = request.Dto.BranchTaxCode.Trim();
        setting.ClientId = request.Dto.ClientId;
        setting.ClientSecret = request.Dto.ClientSecret;
        setting.TokenPin = request.Dto.TokenPin;
        setting.IsProduction = request.Dto.IsProduction;
        setting.AutoSubmitInvoices = request.Dto.AutoSubmitInvoices;
        setting.DefaultVatRate = request.Dto.DefaultVatRate;
        setting.DefaultWhtRate = request.Dto.DefaultWhtRate;
        setting.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.ETASettings.UpdateSettingAsync(setting);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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

        return APIBaseResponse<ETACompanySettingDto>.Success(dto, "ETA company tax settings updated successfully.");
    }
}

public record SubmitJournalEntryToETACommand(int Id) : IRequest<APIBaseResponse<JournalEntryDto>>;

public class SubmitJournalEntryToETACommandHandler : IRequestHandler<SubmitJournalEntryToETACommand, APIBaseResponse<JournalEntryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public SubmitJournalEntryToETACommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<JournalEntryDto>> Handle(SubmitJournalEntryToETACommand request, CancellationToken cancellationToken)
    {
        var entry = await _unitOfWork.JournalEntries.GetByIdAsync(request.Id);
        if (entry == null)
            return APIBaseResponse<JournalEntryDto>.Failure("Journal entry not found.");

        if (entry.Status != JournalEntryStatus.Posted)
            return APIBaseResponse<JournalEntryDto>.Failure("Only posted entries can be submitted to the Egyptian Tax Authority (ETA).");

        // Simulate e-Finance / ETA API e-Invoice/e-Receipt submission response
        var etaSettings = await _unitOfWork.ETASettings.GetSettingAsync();
        entry.ETASubmissionStatus = ETASubmissionStatus.Valid;
        entry.ETAUUID = Guid.NewGuid().ToString("D").ToUpperInvariant();
        entry.ETALongId = $"ETA-{etaSettings.ActivityCode}-{DateTime.UtcNow:yyyyMMdd}-{entry.Id:D6}";
        entry.ETASubmissionDate = DateTime.UtcNow;
        entry.BranchTaxCode = etaSettings.BranchTaxCode;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _unitOfWork.JournalEntries.GetByIdAsync(entry.Id);
        return APIBaseResponse<JournalEntryDto>.Success(FinancialMapper.ToJournalEntryDto(updated!), "Document submitted to Egyptian Tax Authority (ETA) successfully.");
    }
}

internal static class FinancialMapper
{
    public static JournalEntryDto ToJournalEntryDto(JournalEntry j) => new()
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
