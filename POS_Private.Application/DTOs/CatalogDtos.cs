namespace POS_Private.Application.DTOs;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string? NameAr { get; set; }
    public string Slug { get; set; } = null!;
    public string? Description { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? PictureUrl { get; set; }
    public int? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public int ProductsCount { get; set; }
}

public class CreateCategoryDto
{
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string? Slug { get; set; }
    public string DescriptionEn { get; set; } = null!;
    public string DescriptionAr { get; set; } = null!;
    public string? PictureUrl { get; set; }
    public int? ParentCategoryId { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}

public class UpdateCategoryDto : CreateCategoryDto
{
    public int Id { get; set; }
}

public class ColorDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string? HexCode { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

public class CreateColorDto
{
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string? HexCode { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}

public class SizeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string Code { get; set; } = null!;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

public class CreateSizeDto
{
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string Code { get; set; } = null!;
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}

public class ProductDto
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string? NameAr { get; set; }
    public string Slug { get; set; } = null!;
    public string? Description { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? SKU { get; set; }
    public string? Barcode { get; set; }
    public decimal BasePrice { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public string? PictureUrl { get; set; }
    public string? Tags { get; set; }
    public bool TrackStock { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public int TotalStock { get; set; }
    public List<InventoryStockBreakdownDto> StockByInventory { get; set; } = new();
    public List<ProductVariantDto> Variants { get; set; } = new();
}

public class ProductVariantDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int? ColorId { get; set; }
    public string? ColorName { get; set; }
    public string? ColorHex { get; set; }
    public int? SizeId { get; set; }
    public string? SizeName { get; set; }
    public string? SizeCode { get; set; }
    public string? SKU { get; set; }
    public string? Barcode { get; set; }
    public decimal Price { get; set; }
    public decimal? PriceOverride { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? CostPriceOverride { get; set; }
    public string? ImageUrl { get; set; }
    public int StockQuantity { get; set; }
    public List<InventoryStockBreakdownDto> StockByInventory { get; set; } = new();
    public bool IsActive { get; set; }
}

public class ProductImageDto
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = null!;
    public string? AltText { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }
}

public class CreateProductDto
{
    public int CategoryId { get; set; }
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string? Slug { get; set; }
    public string DescriptionEn { get; set; } = null!;
    public string DescriptionAr { get; set; } = null!;
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
    public List<CreateProductVariantDto>? Variants { get; set; }
}

public class CreateProductVariantDto
{
    public int? Id { get; set; }
    public int? ColorId { get; set; }
    public int? SizeId { get; set; }
    public string? SKU { get; set; }
    public string? Barcode { get; set; }
    public decimal? PriceOverride { get; set; }
    public decimal? CostPriceOverride { get; set; }
    public string? ImageUrl { get; set; }
    public int InitialStock { get; set; } = 0;
}

public class UpdateProductDto : CreateProductDto
{
    public int Id { get; set; }
}

public class ProductSearchFilterDto
{
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class PosProductDto
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Barcode { get; set; }
    public string? SKU { get; set; }
    public decimal Price { get; set; }
    public string? PictureUrl { get; set; }
    public int AvailableStock { get; set; }
    public List<ProductVariantDto> Variants { get; set; } = new();
}
