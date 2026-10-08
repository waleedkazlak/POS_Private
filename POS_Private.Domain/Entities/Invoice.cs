using POS_Private.Domain.Enums;

namespace POS_Private.Domain.Entities;

public class Invoice : BaseEntity
{
    public string InvoiceNumber { get; set; } = null!;
    public InvoiceType InvoiceType { get; set; } = InvoiceType.Sale;
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Completed;
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    public int? InventoryId { get; set; }
    public Inventory? Inventory { get; set; }

    public int? RegisterId { get; set; }
    public PosRegister? Register { get; set; }

    public int? ShiftId { get; set; }
    public CashierShift? Shift { get; set; }

    public int CashierUserId { get; set; }
    public User? CashierUser { get; set; }

    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int? CouponId { get; set; }
    public Coupon? Coupon { get; set; }

    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public DiscountType? DiscountType { get; set; }
    
    public decimal TaxPercentage { get; set; } = 14m; // Standard Egyptian VAT is 14%
    public decimal TaxAmount { get; set; }
    
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal ChangeAmount { get; set; }

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Paid;
    public string? Notes { get; set; }

    // Egyptian E-Receipt / Tax Authority fields ready
    public string? EtaUuid { get; set; }
    public string? EtaSubmissionId { get; set; }

    public ICollection<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<InvoiceReturn> Returns { get; set; } = new List<InvoiceReturn>();
}
