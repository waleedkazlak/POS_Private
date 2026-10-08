using POS_Private.Domain.Enums;

namespace POS_Private.Domain.Entities;

public class Payment : BaseEntity
{
    public int InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }

    public PaymentMethod PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public string? ReferenceNumber { get; set; } // e.g. InstaPay Txn ID, Visa Auth Code, Vodafone Cash ref
    public string? Notes { get; set; }
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
}
