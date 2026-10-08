namespace POS_Private.Domain.Entities;

public class ProductImage : BaseEntity
{
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public string ImageUrl { get; set; } = null!;
    public string? AltText { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsPrimary { get; set; } = false;
    public int? Width { get; set; }
    public int? Height { get; set; }
}
