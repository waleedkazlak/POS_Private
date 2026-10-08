namespace POS_Private.Domain.Entities;

public class ProductVariant : BaseEntity
{
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int? ColorId { get; set; }
    public Color? Color { get; set; }

    public int? SizeId { get; set; }
    public Size? Size { get; set; }

    public string? SKU { get; set; }
    public string? Barcode { get; set; }

    /// <summary>
    /// Optional price override. If null, the product's BasePrice is used without duplication.
    /// </summary>
    public decimal? PriceOverride { get; set; }

    /// <summary>
    /// Optional cost price override. If null, the product's CostPrice is used.
    /// </summary>
    public decimal? CostPriceOverride { get; set; }

    public string? ImageUrl { get; set; }
    public int StockQuantity { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    public decimal EffectivePrice => PriceOverride ?? Product?.BasePrice ?? 0m;
    public decimal? EffectiveCostPrice => CostPriceOverride ?? Product?.CostPrice;

    public ICollection<BranchStock> BranchStocks { get; set; } = new List<BranchStock>();
    public ICollection<InvoiceLine> InvoiceLines { get; set; } = new List<InvoiceLine>();
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
