namespace POS_Private.Domain.Entities;

public class Category : BaseEntity
{
    public string NameEn { get; set; } = null!;
    public string? NameAr { get; set; }
    public string Slug { get; set; } = null!;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? PictureUrl { get; set; }
    public int? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;

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

    public ICollection<Category> SubCategories { get; set; } = new List<Category>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
