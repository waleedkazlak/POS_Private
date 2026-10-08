using POS_Private.Domain.Enums;

namespace POS_Private.Domain.Entities;

public class Account : BaseEntity
{
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public AccountType Type { get; set; }
    public AccountNature Nature { get; set; }
    public int Level { get; set; } = 1;
    public bool IsPostable { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
    public ETATaxType ETATaxType { get; set; } = ETATaxType.None;
    public decimal CurrentBalance { get; set; } = 0m;

    public int? ParentAccountId { get; set; }
    public Account? ParentAccount { get; set; }
    public ICollection<Account> ChildrenAccounts { get; set; } = new List<Account>();

    public ICollection<JournalEntryLine> JournalLines { get; set; } = new List<JournalEntryLine>();
}

public class CostCenter : BaseEntity
{
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public CostCenterType Type { get; set; } = CostCenterType.CostCenter;
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }

    public int? ParentCostCenterId { get; set; }
    public CostCenter? ParentCostCenter { get; set; }
    public ICollection<CostCenter> SubCostCenters { get; set; } = new List<CostCenter>();

    public int? BranchId { get; set; }
    public Branch? Branch { get; set; }

    public ICollection<JournalEntryLine> JournalLines { get; set; } = new List<JournalEntryLine>();
}

public class JournalEntry : BaseEntity
{
    public string EntryNumber { get; set; } = null!; // e.g., JE-2026-00001
    public DateTime EntryDate { get; set; } = DateTime.UtcNow;
    public DateTime? PostingDate { get; set; }
    public JournalEntryStatus Status { get; set; } = JournalEntryStatus.Draft;
    public JournalSourceType SourceType { get; set; } = JournalSourceType.Manual;
    public string? ReferenceNumber { get; set; } // Reference to invoice, bill, receipt, etc.

    public string NarrationEn { get; set; } = null!;
    public string NarrationAr { get; set; } = null!;

    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }

    public int? CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }

    public int? ApprovedByUserId { get; set; }
    public User? ApprovedByUser { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public string? VoidReason { get; set; }
    public DateTime? VoidedAt { get; set; }
    public int? VoidedByUserId { get; set; }
    public User? VoidedByUser { get; set; }

    // ETA / Governmental e-Finance & Tax Authority Integration
    public ETASubmissionStatus ETASubmissionStatus { get; set; } = ETASubmissionStatus.NotApplicable;
    public string? ETAUUID { get; set; }
    public string? ETALongId { get; set; }
    public DateTime? ETASubmissionDate { get; set; }
    public string? ETARejectionReason { get; set; }
    public string? BranchTaxCode { get; set; }

    public ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();
}

public class JournalEntryLine : BaseEntity
{
    public int JournalEntryId { get; set; }
    public JournalEntry JournalEntry { get; set; } = null!;

    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;

    public int? CostCenterId { get; set; }
    public CostCenter? CostCenter { get; set; }

    public decimal Debit { get; set; } = 0m;
    public decimal Credit { get; set; } = 0m;

    public string? Description { get; set; }

    // ETA Tax Code on specific line (e.g., T1 VAT, T4 WHT)
    public ETATaxType ETATaxType { get; set; } = ETATaxType.None;
    public decimal? TaxRate { get; set; }
    public decimal? TaxAmount { get; set; }
}

public class FiscalPeriod : BaseEntity
{
    public int Year { get; set; }
    public int PeriodNumber { get; set; } // 1..12
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsClosed { get; set; } = false;
    public DateTime? ClosedAt { get; set; }
}

public class ETACompanySetting : BaseEntity
{
    public string TaxRegistrationNumber { get; set; } = "100-200-300"; // رقم التسجيل الضريبي
    public string ActivityCode { get; set; } = "4771"; // كود النشاط حسب مصلحة الضرائب المصرية
    public string CompanyName { get; set; } = "POS Private Co.";
    public string BranchTaxCode { get; set; } = "0"; // كود الفرع الضريبي الرئيسي
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? TokenPin { get; set; }
    public bool IsProduction { get; set; } = false;
    public bool AutoSubmitInvoices { get; set; } = false;
    public decimal DefaultVatRate { get; set; } = 14.0m;
    public decimal DefaultWhtRate { get; set; } = 1.0m;
}
