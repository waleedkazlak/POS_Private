using POS_Private.Application.Common;
using POS_Private.Application.DTOs;
using POS_Private.Application.Exceptions;
using POS_Private.Domain.Entities;
using POS_Private.Domain.Interfaces;
using MediatR;

namespace POS_Private.Application.Queries.Product;

public record GetProductsQuery(ProductSearchFilterDto Filter) : IRequest<APIBaseResponse<PaginatedResultDto<ProductDto>>>;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, APIBaseResponse<PaginatedResultDto<ProductDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProductsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<PaginatedResultDto<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _unitOfWork.Products.SearchAsync(
            request.Filter.SearchTerm,
            request.Filter.CategoryId,
            request.Filter.IsActive,
            request.Filter.PageNumber,
            request.Filter.PageSize);

        var dtos = items.Select(p => new ProductDto
        {
            Id = p.Id,
            CategoryId = p.CategoryId,
            CategoryName = LocalizationHelper.Localize(p.Category?.NameEn, p.Category?.NameAr),
            Name = LocalizationHelper.Localize(p.NameEn, p.NameAr),
            NameEn = p.NameEn,
            NameAr = p.NameAr,
            Slug = p.Slug,
            Description = LocalizationHelper.LocalizeNullable(p.DescriptionEn, p.DescriptionAr),
            DescriptionEn = p.DescriptionEn,
            DescriptionAr = p.DescriptionAr,
            SKU = p.SKU,
            Barcode = p.Barcode,
            BasePrice = p.BasePrice,
            CostPrice = p.CostPrice,
            CompareAtPrice = p.CompareAtPrice,
            PictureUrl = p.PictureUrl,
            Tags = p.Tags,
            TrackStock = p.TrackStock,
            IsActive = p.IsActive,
            IsFeatured = p.IsFeatured,
            TotalStock = p.BranchStocks.Sum(bs => bs.QuantityOnHand),
            StockByInventory = p.BranchStocks.Select(bs => new InventoryStockBreakdownDto
            {
                InventoryId = bs.InventoryId,
                InventoryCode = bs.Inventory?.Code ?? string.Empty,
                InventoryName = bs.Inventory != null ? LocalizationHelper.Localize(bs.Inventory.NameEn, bs.Inventory.NameAr) : "N/A",
                BranchId = bs.BranchId,
                BranchName = bs.Branch != null ? LocalizationHelper.Localize(bs.Branch.NameEn, bs.Branch.NameAr) : null,
                QuantityOnHand = bs.QuantityOnHand,
                MinAlertLevel = bs.MinAlertLevel
            }).ToList(),
            Variants = p.Variants.Select(v => MapVariantDto(v, p)).ToList()
        }).ToList();

        var result = new PaginatedResultDto<ProductDto>(dtos, totalCount, request.Filter.PageNumber, request.Filter.PageSize);
        return APIBaseResponse<PaginatedResultDto<ProductDto>>.Success(result);
    }

    internal static ProductVariantDto MapVariantDto(ProductVariant v, Domain.Entities.Product p)
    {
        var colorName = v.Color != null ? LocalizationHelper.Localize(v.Color.NameEn, v.Color.NameAr) : null;
        var sizeName = v.Size != null ? LocalizationHelper.Localize(v.Size.NameEn, v.Size.NameAr) : null;
        var effectivePrice = v.PriceOverride ?? p.BasePrice;
        var effectiveCost = v.CostPriceOverride ?? p.CostPrice;

        return new ProductVariantDto
        {
            Id = v.Id,
            ProductId = v.ProductId,
            ColorId = v.ColorId,
            ColorName = colorName,
            ColorHex = v.Color?.HexCode,
            SizeId = v.SizeId,
            SizeName = sizeName,
            SizeCode = v.Size?.Code,
            SKU = v.SKU,
            Barcode = v.Barcode,
            Price = effectivePrice,
            PriceOverride = v.PriceOverride,
            CostPrice = effectiveCost,
            CostPriceOverride = v.CostPriceOverride,
            ImageUrl = v.ImageUrl ?? p.PictureUrl,
            StockQuantity = v.StockQuantity,
            StockByInventory = p.BranchStocks.Where(bs => bs.VariantId == v.Id).Select(bs => new InventoryStockBreakdownDto
            {
                InventoryId = bs.InventoryId,
                InventoryCode = bs.Inventory?.Code ?? string.Empty,
                InventoryName = bs.Inventory != null ? LocalizationHelper.Localize(bs.Inventory.NameEn, bs.Inventory.NameAr) : "N/A",
                BranchId = bs.BranchId,
                BranchName = bs.Branch != null ? LocalizationHelper.Localize(bs.Branch.NameEn, bs.Branch.NameAr) : null,
                QuantityOnHand = bs.QuantityOnHand,
                MinAlertLevel = bs.MinAlertLevel
            }).ToList(),
            IsActive = v.IsActive
        };
    }
}

public record GetProductByIdQuery(int Id) : IRequest<APIBaseResponse<ProductDto>>;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, APIBaseResponse<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProductByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdWithDetailsAsync(request.Id);
        if (product == null)
            throw new NotFoundException(nameof(Domain.Entities.Product), request.Id);

        var dto = new ProductDto
        {
            Id = product.Id,
            CategoryId = product.CategoryId,
            CategoryName = LocalizationHelper.Localize(product.Category?.NameEn, product.Category?.NameAr),
            Name = LocalizationHelper.Localize(product.NameEn, product.NameAr),
            NameEn = product.NameEn,
            NameAr = product.NameAr,
            Slug = product.Slug,
            Description = LocalizationHelper.LocalizeNullable(product.DescriptionEn, product.DescriptionAr),
            DescriptionEn = product.DescriptionEn,
            DescriptionAr = product.DescriptionAr,
            SKU = product.SKU,
            Barcode = product.Barcode,
            BasePrice = product.BasePrice,
            CostPrice = product.CostPrice,
            CompareAtPrice = product.CompareAtPrice,
            PictureUrl = product.PictureUrl,
            Tags = product.Tags,
            TrackStock = product.TrackStock,
            IsActive = product.IsActive,
            IsFeatured = product.IsFeatured,
            TotalStock = product.BranchStocks.Sum(bs => bs.QuantityOnHand),
            StockByInventory = product.BranchStocks.Select(bs => new InventoryStockBreakdownDto
            {
                InventoryId = bs.InventoryId,
                InventoryCode = bs.Inventory?.Code ?? string.Empty,
                InventoryName = bs.Inventory != null ? LocalizationHelper.Localize(bs.Inventory.NameEn, bs.Inventory.NameAr) : "N/A",
                BranchId = bs.BranchId,
                BranchName = bs.Branch != null ? LocalizationHelper.Localize(bs.Branch.NameEn, bs.Branch.NameAr) : null,
                QuantityOnHand = bs.QuantityOnHand,
                MinAlertLevel = bs.MinAlertLevel
            }).ToList(),
            Variants = product.Variants.Select(v => GetProductsQueryHandler.MapVariantDto(v, product)).ToList()
        };

        return APIBaseResponse<ProductDto>.Success(dto);
    }
}

public record GetProductByBarcodeQuery(string BarcodeOrSku) : IRequest<APIBaseResponse<ProductDto>>;

public class GetProductByBarcodeQueryHandler : IRequestHandler<GetProductByBarcodeQuery, APIBaseResponse<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProductByBarcodeQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<ProductDto>> Handle(GetProductByBarcodeQuery request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByBarcodeOrSkuAsync(request.BarcodeOrSku);
        if (product == null)
            throw new NotFoundException("Product", request.BarcodeOrSku);

        var dto = new ProductDto
        {
            Id = product.Id,
            CategoryId = product.CategoryId,
            CategoryName = LocalizationHelper.Localize(product.Category?.NameEn, product.Category?.NameAr),
            Name = LocalizationHelper.Localize(product.NameEn, product.NameAr),
            NameEn = product.NameEn,
            NameAr = product.NameAr,
            Slug = product.Slug,
            Description = LocalizationHelper.LocalizeNullable(product.DescriptionEn, product.DescriptionAr),
            DescriptionEn = product.DescriptionEn,
            DescriptionAr = product.DescriptionAr,
            SKU = product.SKU,
            Barcode = product.Barcode,
            BasePrice = product.BasePrice,
            CostPrice = product.CostPrice,
            CompareAtPrice = product.CompareAtPrice,
            PictureUrl = product.PictureUrl,
            Tags = product.Tags,
            TrackStock = product.TrackStock,
            IsActive = product.IsActive,
            IsFeatured = product.IsFeatured,
            TotalStock = product.BranchStocks.Sum(bs => bs.QuantityOnHand),
            StockByInventory = product.BranchStocks.Select(bs => new InventoryStockBreakdownDto
            {
                InventoryId = bs.InventoryId,
                InventoryCode = bs.Inventory?.Code ?? string.Empty,
                InventoryName = bs.Inventory != null ? LocalizationHelper.Localize(bs.Inventory.NameEn, bs.Inventory.NameAr) : "N/A",
                BranchId = bs.BranchId,
                BranchName = bs.Branch != null ? LocalizationHelper.Localize(bs.Branch.NameEn, bs.Branch.NameAr) : null,
                QuantityOnHand = bs.QuantityOnHand,
                MinAlertLevel = bs.MinAlertLevel
            }).ToList(),
            Variants = product.Variants.Select(v => GetProductsQueryHandler.MapVariantDto(v, product)).ToList()
        };

        return APIBaseResponse<ProductDto>.Success(dto);
    }
}

public record GetPosCatalogQuery(int? BranchId, int? CategoryId, string? SearchTerm) : IRequest<APIBaseResponse<IEnumerable<PosProductDto>>>;

public class GetPosCatalogQueryHandler : IRequestHandler<GetPosCatalogQuery, APIBaseResponse<IEnumerable<PosProductDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPosCatalogQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<IEnumerable<PosProductDto>>> Handle(GetPosCatalogQuery request, CancellationToken cancellationToken)
    {
        var (items, _) = await _unitOfWork.Products.SearchAsync(
            request.SearchTerm,
            request.CategoryId,
            true, // active only
            1,
            100);

        var dtos = items.Select(p =>
        {
            var branchStock = request.BranchId.HasValue 
                ? p.BranchStocks.Where(bs => bs.BranchId == request.BranchId.Value).Sum(bs => bs.QuantityOnHand)
                : p.BranchStocks.Sum(bs => bs.QuantityOnHand);

            return new PosProductDto
            {
                Id = p.Id,
                CategoryId = p.CategoryId,
                CategoryName = LocalizationHelper.Localize(p.Category?.NameEn, p.Category?.NameAr),
                Name = LocalizationHelper.Localize(p.NameEn, p.NameAr),
                Barcode = p.Barcode,
                SKU = p.SKU,
                Price = p.BasePrice,
                PictureUrl = p.PictureUrl,
                AvailableStock = branchStock,
                Variants = p.Variants.Where(v => v.IsActive).Select(v => GetProductsQueryHandler.MapVariantDto(v, p)).ToList()
            };
        }).ToList();

        return APIBaseResponse<IEnumerable<PosProductDto>>.Success(dtos);
    }
}
