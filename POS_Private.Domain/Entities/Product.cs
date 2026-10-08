namespace POS_Private.Domain.Entities;

public class Product : BaseEntity
{
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public string NameEn { get; set; } = null!;
    public string? NameAr { get; set; }
    public string Slug { get; set; } = null!;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    
    public string? SKU { get; set; }
    public string? Barcode { get; set; }
    
    public decimal BasePrice { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? CompareAtPrice { get; set; }

    public string? PictureUrl { get; set; }
    public string? Tags { get; set; }
    
    public bool TrackStock { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; } = false;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string Name
    {
        get => NameEn;
        set => NameEn = value;
    }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? Description
    {
        get => DescriptionEn;
        set => DescriptionEn = value;
    }

    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<BranchStock> BranchStocks { get; set; } = new List<BranchStock>();
    public ICollection<InvoiceLine> InvoiceLines { get; set; } = new List<InvoiceLine>();
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
