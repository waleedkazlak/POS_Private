using POS_Private.Domain.Enums;

namespace POS_Private.Domain.Entities;

public class InvoiceReturn : BaseEntity
{
    public int OriginalInvoiceId { get; set; }
    public Invoice? OriginalInvoice { get; set; }

    public string ReturnNumber { get; set; } = null!;
    public string? ReturnReason { get; set; }
    public PaymentMethod RefundMethod { get; set; } = PaymentMethod.Cash;
    public decimal RefundAmount { get; set; }
    
    public int ProcessedByUserId { get; set; }
    public User? ProcessedByUser { get; set; }

    public ICollection<InvoiceReturnLine> Lines { get; set; } = new List<InvoiceReturnLine>();
}

public class InvoiceReturnLine : BaseEntity
{
    public int InvoiceReturnId { get; set; }
    public InvoiceReturn? InvoiceReturn { get; set; }

    public int InvoiceLineId { get; set; }
    public InvoiceLine? InvoiceLine { get; set; }

    public int ProductId { get; set; }
    public int? VariantId { get; set; }
    public int Quantity { get; set; }
    public decimal RefundAmount { get; set; }
    public bool Restock { get; set; } = true;
}
