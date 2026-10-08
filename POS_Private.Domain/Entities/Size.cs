namespace POS_Private.Domain.Entities;

public class Size : BaseEntity
{
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string Code { get; set; } = null!; // e.g. "S", "M", "L", "XL", "54", "56", "Free Size"
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
}
