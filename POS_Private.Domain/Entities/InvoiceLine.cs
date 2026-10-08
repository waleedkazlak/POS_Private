namespace POS_Private.Domain.Entities;

public class InvoiceLine : BaseEntity
{
    public int InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int? VariantId { get; set; }
    public ProductVariant? Variant { get; set; }

    public int? InventoryId { get; set; }
    public Inventory? Inventory { get; set; }

    public string ProductName { get; set; } = null!;
    public string? VariantName { get; set; }
    public string? SKU { get; set; }

    public decimal UnitPrice { get; set; }
    public decimal? CostPrice { get; set; }
    public int Quantity { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public decimal TaxAmount { get; set; } = 0;
    public decimal TotalLineAmount { get; set; }
    public string? Notes { get; set; }

    public ICollection<InvoiceReturnLine> ReturnLines { get; set; } = new List<InvoiceReturnLine>();
}
