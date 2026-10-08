using POS_Private.Domain.Enums;

namespace POS_Private.Domain.Entities;

public class Vendor : BaseEntity
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? ContactPerson { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? TaxNumber { get; set; }
    public string? CommercialRegister { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    
    public decimal OpeningBalance { get; set; } = 0m;
    public decimal CurrentBalance { get; set; } = 0m;
    public decimal? CreditLimit { get; set; }
    public int? PaymentTermsDays { get; set; }
    
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<VendorBill> Bills { get; set; } = new List<VendorBill>();
    public ICollection<VendorPayment> Payments { get; set; } = new List<VendorPayment>();
    public ICollection<VendorTransaction> Transactions { get; set; } = new List<VendorTransaction>();
}

public class VendorTransaction : BaseEntity
{
    public int VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    public VendorTransactionType TransactionType { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public string ReferenceNumber { get; set; } = null!; // Bill #, Payment #, etc.
    
    public decimal DebitAmount { get; set; } = 0m;   // Reduces vendor payable (Payments, Returns)
    public decimal CreditAmount { get; set; } = 0m;  // Increases vendor payable (Bills, Opening)
    public decimal BalanceAfter { get; set; } = 0m;  // Running balance
    
    public string? Notes { get; set; }
    public int? BranchId { get; set; }
    public Branch? Branch { get; set; }
    
    public int? CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }
}

public class VendorBill : BaseEntity
{
    public string BillNumber { get; set; } = null!;
    public int VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    public int? BranchId { get; set; }
    public Branch? Branch { get; set; }

    public int? InventoryId { get; set; }
    public Inventory? Inventory { get; set; }

    public DateTime BillDate { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    
    public decimal SubTotal { get; set; } = 0m;
    public decimal TaxAmount { get; set; } = 0m;
    public decimal DiscountAmount { get; set; } = 0m;
    public decimal TotalAmount { get; set; } = 0m;
    public decimal PaidAmount { get; set; } = 0m;
    public decimal RemainingAmount { get; set; } = 0m;

    public VendorBillStatus Status { get; set; } = VendorBillStatus.Approved;
    public string? Notes { get; set; }
    public string? VendorInvoiceNumber { get; set; } // The vendor's physical paper invoice number

    public int? CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }

    public ICollection<VendorBillLine> Lines { get; set; } = new List<VendorBillLine>();
    public ICollection<VendorPayment> Payments { get; set; } = new List<VendorPayment>();
}

public class VendorBillLine : BaseEntity
{
    public int VendorBillId { get; set; }
    public VendorBill? VendorBill { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int? VariantId { get; set; }
    public ProductVariant? Variant { get; set; }

    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; } = 0m;
    public decimal TaxAmount { get; set; } = 0m;
    public decimal LineTotal { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public string? Notes { get; set; }
}

public class VendorPayment : BaseEntity
{
    public string PaymentNumber { get; set; } = null!;
    public int VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    public int? BranchId { get; set; }
    public Branch? Branch { get; set; }

    public int? VendorBillId { get; set; }
    public VendorBill? VendorBill { get; set; }

    public decimal Amount { get; set; }
    public ExpensePaymentMethod PaymentMethod { get; set; } = ExpensePaymentMethod.BankTransfer;
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    
    public string? ReferenceNo { get; set; } // Cheque number, Bank transfer reference
    public string? Notes { get; set; }

    public int? CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }
}
