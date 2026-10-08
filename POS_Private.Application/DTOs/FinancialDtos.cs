using POS_Private.Domain.Enums;

namespace POS_Private.Application.DTOs;

// ==========================================
// Accounts DTOs
// ==========================================

public class AccountDto
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public AccountType Type { get; set; }
    public string TypeName => Type.ToString();
    public AccountNature Nature { get; set; }
    public string NatureName => Nature.ToString();
    public int Level { get; set; }
    public bool IsPostable { get; set; }
    public bool IsActive { get; set; }
    public string? Description { get; set; }
    public ETATaxType ETATaxType { get; set; }
    public string ETATaxTypeName => ETATaxType.ToString();
    public decimal CurrentBalance { get; set; }
    public int? ParentAccountId { get; set; }
    public string? ParentAccountNameEn { get; set; }
    public string? ParentAccountNameAr { get; set; }
    public string? ParentAccountCode { get; set; }
    public List<AccountDto> Children { get; set; } = new();
}

public class CreateAccountDto
{
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public AccountType Type { get; set; }
    public AccountNature Nature { get; set; }
    public int? ParentAccountId { get; set; }
    public bool IsPostable { get; set; } = true;
    public string? Description { get; set; }
    public ETATaxType ETATaxType { get; set; } = ETATaxType.None;
}

public class UpdateAccountDto
{
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public AccountNature Nature { get; set; }
    public bool IsPostable { get; set; }
    public bool IsActive { get; set; }
    public string? Description { get; set; }
    public ETATaxType ETATaxType { get; set; }
}

// ==========================================
// Cost Centers DTOs
// ==========================================

public class CostCenterDto
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public CostCenterType Type { get; set; }
    public string TypeName => Type.ToString();
    public bool IsActive { get; set; }
    public string? Description { get; set; }
    public int? ParentCostCenterId { get; set; }
    public string? ParentCostCenterNameEn { get; set; }
    public string? ParentCostCenterNameAr { get; set; }
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
}

public class CreateCostCenterDto
{
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public CostCenterType Type { get; set; } = CostCenterType.CostCenter;
    public int? ParentCostCenterId { get; set; }
    public int? BranchId { get; set; }
    public string? Description { get; set; }
}

public class UpdateCostCenterDto
{
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public CostCenterType Type { get; set; }
    public int? ParentCostCenterId { get; set; }
    public int? BranchId { get; set; }
    public bool IsActive { get; set; }
    public string? Description { get; set; }
}

// ==========================================
// Journal Entry DTOs
// ==========================================

public class JournalEntryDto
{
    public int Id { get; set; }
    public string EntryNumber { get; set; } = null!;
    public DateTime EntryDate { get; set; }
    public DateTime? PostingDate { get; set; }
    public JournalEntryStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public JournalSourceType SourceType { get; set; }
    public string SourceTypeName => SourceType.ToString();
    public string? ReferenceNumber { get; set; }
    public string NarrationEn { get; set; } = null!;
    public string NarrationAr { get; set; } = null!;
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public bool IsBalanced => TotalDebit == TotalCredit;

    public int? CreatedByUserId { get; set; }
    public string? CreatedByName { get; set; }

    public int? ApprovedByUserId { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public string? VoidReason { get; set; }
    public DateTime? VoidedAt { get; set; }
    public string? VoidedByName { get; set; }

    // ETA Metadata
    public ETASubmissionStatus ETASubmissionStatus { get; set; }
    public string ETASubmissionStatusName => ETASubmissionStatus.ToString();
    public string? ETAUUID { get; set; }
    public string? ETALongId { get; set; }
    public DateTime? ETASubmissionDate { get; set; }
    public string? ETARejectionReason { get; set; }
    public string? BranchTaxCode { get; set; }

    public List<JournalEntryLineDto> Lines { get; set; } = new();
}

public class JournalEntryLineDto
{
    public int Id { get; set; }
    public int JournalEntryId { get; set; }
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = null!;
    public string AccountNameEn { get; set; } = null!;
    public string AccountNameAr { get; set; } = null!;
    public int? CostCenterId { get; set; }
    public string? CostCenterCode { get; set; }
    public string? CostCenterNameEn { get; set; }
    public string? CostCenterNameAr { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string? Description { get; set; }
    public ETATaxType ETATaxType { get; set; }
    public decimal? TaxRate { get; set; }
    public decimal? TaxAmount { get; set; }
}

public class CreateJournalEntryLineDto
{
    public int AccountId { get; set; }
    public int? CostCenterId { get; set; }
    public decimal Debit { get; set; } = 0m;
    public decimal Credit { get; set; } = 0m;
    public string? Description { get; set; }
    public ETATaxType ETATaxType { get; set; } = ETATaxType.None;
    public decimal? TaxRate { get; set; }
    public decimal? TaxAmount { get; set; }
}

public class CreateJournalEntryDto
{
    public DateTime EntryDate { get; set; } = DateTime.UtcNow;
    public JournalSourceType SourceType { get; set; } = JournalSourceType.Manual;
    public string? ReferenceNumber { get; set; }
    public string NarrationEn { get; set; } = null!;
    public string NarrationAr { get; set; } = null!;
    public string? BranchTaxCode { get; set; }
    public bool AutoPost { get; set; } = false; // Post immediately if true and balanced
    public List<CreateJournalEntryLineDto> Lines { get; set; } = new();
}

public class VoidJournalEntryDto
{
    public string Reason { get; set; } = null!;
}

// ==========================================
// Financial Reports & Audit DTOs
// ==========================================

public class TrialBalanceRowDto
{
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = null!;
    public string AccountNameEn { get; set; } = null!;
    public string AccountNameAr { get; set; } = null!;
    public AccountType Type { get; set; }
    public AccountNature Nature { get; set; }
    public int Level { get; set; }
    public bool IsPostable { get; set; }

    public decimal OpeningDebit { get; set; }
    public decimal OpeningCredit { get; set; }

    public decimal PeriodDebit { get; set; }
    public decimal PeriodCredit { get; set; }

    public decimal EndingDebit { get; set; }
    public decimal EndingCredit { get; set; }
}

public class TrialBalanceReportDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int? CostCenterId { get; set; }
    public string? CostCenterName { get; set; }

    public decimal TotalOpeningDebit { get; set; }
    public decimal TotalOpeningCredit { get; set; }

    public decimal TotalPeriodDebit { get; set; }
    public decimal TotalPeriodCredit { get; set; }

    public decimal TotalEndingDebit { get; set; }
    public decimal TotalEndingCredit { get; set; }

    public bool IsBalanced => Math.Abs(TotalPeriodDebit - TotalPeriodCredit) < 0.01m &&
                              Math.Abs(TotalEndingDebit - TotalEndingCredit) < 0.01m;

    public List<TrialBalanceRowDto> Rows { get; set; } = new();
}

public class AccountLedgerRowDto
{
    public int JournalEntryId { get; set; }
    public string EntryNumber { get; set; } = null!;
    public DateTime EntryDate { get; set; }
    public string? ReferenceNumber { get; set; }
    public string NarrationEn { get; set; } = null!;
    public string NarrationAr { get; set; } = null!;
    public string? CostCenterCode { get; set; }
    public string? CostCenterName { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal RunningBalance { get; set; }
}

public class AccountLedgerReportDto
{
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = null!;
    public string AccountNameEn { get; set; } = null!;
    public string AccountNameAr { get; set; } = null!;
    public AccountType Type { get; set; }
    public AccountNature Nature { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal TotalDebits { get; set; }
    public decimal TotalCredits { get; set; }
    public decimal EndingBalance { get; set; }
    public List<AccountLedgerRowDto> Transactions { get; set; } = new();
}

public class CostCenterProfitabilityDto
{
    public int CostCenterId { get; set; }
    public string CostCenterCode { get; set; } = null!;
    public string CostCenterNameEn { get; set; } = null!;
    public string CostCenterNameAr { get; set; } = null!;
    public CostCenterType Type { get; set; }
    public decimal TotalRevenues { get; set; }
    public decimal TotalCosts { get; set; }
    public decimal NetIncome => TotalRevenues - TotalCosts;
    public decimal ProfitMarginPercentage => TotalRevenues > 0 ? (NetIncome / TotalRevenues) * 100m : 0m;
}

public class CostCenterIncomeStatementDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal TotalCompanyRevenue { get; set; }
    public decimal TotalCompanyCost { get; set; }
    public decimal NetCompanyProfit => TotalCompanyRevenue - TotalCompanyCost;
    public List<CostCenterProfitabilityDto> CostCenters { get; set; } = new();
}

// ==========================================
// ETA / e-Finance Tax Compliance DTOs
// ==========================================

public class ETATaxLineSummaryDto
{
    public ETATaxType TaxType { get; set; }
    public string TaxTypeCode => TaxType.ToString();
    public string TaxNameEn { get; set; } = null!;
    public string TaxNameAr { get; set; } = null!;
    public decimal TaxableAmount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal CalculatedTax { get; set; }
    public decimal TotalSalesTaxCollected { get; set; }
    public decimal TotalPurchaseTaxDeducted { get; set; }
    public decimal NetTaxDue => TotalSalesTaxCollected - TotalPurchaseTaxDeducted;
}

public class ETATaxSummaryReportDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string TaxRegistrationNumber { get; set; } = null!;
    public string ActivityCode { get; set; } = null!;
    public string CompanyName { get; set; } = null!;
    public decimal GrossSales { get; set; }
    public decimal TotalVatCollected { get; set; } // 14% Output VAT
    public decimal TotalVatPaidOnPurchases { get; set; } // 14% Input VAT
    public decimal NetVatPayable => TotalVatCollected - TotalVatPaidOnPurchases;
    public decimal TotalWithholdingTaxDeducted { get; set; } // 1% WHT
    public List<ETATaxLineSummaryDto> TaxBreakdowns { get; set; } = new();
}

public class ETACompanySettingDto
{
    public int Id { get; set; }
    public string TaxRegistrationNumber { get; set; } = null!;
    public string ActivityCode { get; set; } = null!;
    public string CompanyName { get; set; } = null!;
    public string BranchTaxCode { get; set; } = null!;
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? TokenPin { get; set; }
    public bool IsProduction { get; set; }
    public bool AutoSubmitInvoices { get; set; }
    public decimal DefaultVatRate { get; set; }
    public decimal DefaultWhtRate { get; set; }
}

public class UpdateETACompanySettingDto
{
    public string TaxRegistrationNumber { get; set; } = null!;
    public string ActivityCode { get; set; } = null!;
    public string CompanyName { get; set; } = null!;
    public string BranchTaxCode { get; set; } = null!;
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? TokenPin { get; set; }
    public bool IsProduction { get; set; }
    public bool AutoSubmitInvoices { get; set; }
    public decimal DefaultVatRate { get; set; }
    public decimal DefaultWhtRate { get; set; }
}
