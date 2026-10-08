using POS_Private.Application.Common;
using POS_Private.Application.DTOs;
using POS_Private.Application.Exceptions;
using POS_Private.Domain.Entities;
using POS_Private.Domain.Interfaces;
using MediatR;

namespace POS_Private.Application.Queries.Inventory;

public record GetInventoriesQuery(InventorySearchFilterDto Filter) : IRequest<APIBaseResponse<PaginatedResultDto<InventoryDto>>>;

public class GetInventoriesQueryHandler : IRequestHandler<GetInventoriesQuery, APIBaseResponse<PaginatedResultDto<InventoryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetInventoriesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<PaginatedResultDto<InventoryDto>>> Handle(GetInventoriesQuery request, CancellationToken cancellationToken)
    {
        var f = request.Filter;
        var (inventories, totalCount) = await _unitOfWork.Inventories.SearchAsync(
            f.SearchTerm,
            f.IncludeInactive,
            f.BranchId,
            f.PageNumber,
            f.PageSize);

        var dtos = inventories.Select(i => new InventoryDto
        {
            Id = i.Id,
            Code = i.Code,
            Name = LocalizationHelper.Localize(i.NameEn, i.NameAr),
            NameEn = i.NameEn,
            NameAr = i.NameAr,
            Description = LocalizationHelper.LocalizeNullable(i.DescriptionEn, i.DescriptionAr),
            DescriptionEn = i.DescriptionEn,
            DescriptionAr = i.DescriptionAr,
            Address = i.Address,
            BranchId = i.BranchId,
            BranchName = i.Branch != null ? LocalizationHelper.Localize(i.Branch.NameEn, i.Branch.NameAr) : null,
            IsActive = i.IsActive,
            IsDefault = i.IsDefault,
            TotalStockCount = i.Stocks?.Sum(s => s.QuantityOnHand) ?? 0
        }).ToList();

        var result = new PaginatedResultDto<InventoryDto>(dtos, totalCount, f.PageNumber, f.PageSize);
        return APIBaseResponse<PaginatedResultDto<InventoryDto>>.Success(result);
    }
}

public record GetInventoryByIdQuery(int Id) : IRequest<APIBaseResponse<InventoryDto>>;

public class GetInventoryByIdQueryHandler : IRequestHandler<GetInventoryByIdQuery, APIBaseResponse<InventoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetInventoryByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<InventoryDto>> Handle(GetInventoryByIdQuery request, CancellationToken cancellationToken)
    {
        var i = await _unitOfWork.Inventories.GetByIdAsync(request.Id);
        if (i == null)
            throw new NotFoundException(nameof(POS_Private.Domain.Entities.Inventory), request.Id);

        var dto = new InventoryDto
        {
            Id = i.Id,
            Code = i.Code,
            Name = LocalizationHelper.Localize(i.NameEn, i.NameAr),
            NameEn = i.NameEn,
            NameAr = i.NameAr,
            Description = LocalizationHelper.LocalizeNullable(i.DescriptionEn, i.DescriptionAr),
            DescriptionEn = i.DescriptionEn,
            DescriptionAr = i.DescriptionAr,
            Address = i.Address,
            BranchId = i.BranchId,
            BranchName = i.Branch != null ? LocalizationHelper.Localize(i.Branch.NameEn, i.Branch.NameAr) : null,
            IsActive = i.IsActive,
            IsDefault = i.IsDefault,
            TotalStockCount = i.Stocks?.Sum(s => s.QuantityOnHand) ?? 0
        };

        return APIBaseResponse<InventoryDto>.Success(dto);
    }
}

public record GetBranchStocksQuery(StockSearchFilterDto Filter) : IRequest<APIBaseResponse<PaginatedResultDto<BranchStockDto>>>;

public class GetBranchStocksQueryHandler : IRequestHandler<GetBranchStocksQuery, APIBaseResponse<PaginatedResultDto<BranchStockDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetBranchStocksQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<PaginatedResultDto<BranchStockDto>>> Handle(GetBranchStocksQuery request, CancellationToken cancellationToken)
    {
        var f = request.Filter;
        var (stocks, totalCount) = await _unitOfWork.BranchStocks.SearchStocksAsync(
            f.InventoryId,
            f.BranchId,
            f.ProductId,
            f.SearchTerm,
            f.IsLowStock,
            f.PageNumber,
            f.PageSize);

        var dtos = stocks.Select(s => new BranchStockDto
        {
            Id = s.Id,
            InventoryId = s.InventoryId,
            InventoryName = s.Inventory != null ? LocalizationHelper.Localize(s.Inventory.NameEn, s.Inventory.NameAr) : "N/A",
            InventoryCode = s.Inventory?.Code,
            BranchId = s.BranchId,
            BranchName = s.Branch != null ? LocalizationHelper.Localize(s.Branch.NameEn, s.Branch.NameAr) : (s.Inventory?.Branch != null ? LocalizationHelper.Localize(s.Inventory.Branch.NameEn, s.Inventory.Branch.NameAr) : null),
            ProductId = s.ProductId,
            ProductName = LocalizationHelper.Localize(s.Product?.NameEn, s.Product?.NameAr),
            SKU = s.Variant != null ? (s.Variant.SKU ?? s.Product?.SKU) : s.Product?.SKU,
            Barcode = s.Variant != null ? (s.Variant.Barcode ?? s.Product?.Barcode) : s.Product?.Barcode,
            VariantId = s.VariantId,
            VariantName = FormatVariantName(s.Variant),
            Price = s.Variant?.EffectivePrice ?? (s.Product?.BasePrice ?? 0),
            QuantityOnHand = s.QuantityOnHand,
            MinAlertLevel = s.MinAlertLevel
        }).ToList();

        var result = new PaginatedResultDto<BranchStockDto>(dtos, totalCount, f.PageNumber, f.PageSize);
        return APIBaseResponse<PaginatedResultDto<BranchStockDto>>.Success(result);
    }

    public static string? FormatVariantName(ProductVariant? v)
    {
        if (v == null) return null;
        var colorName = v.Color != null ? LocalizationHelper.Localize(v.Color.NameEn, v.Color.NameAr) : null;
        var sizeCode = v.Size != null ? (LocalizationHelper.IsArabic ? v.Size.NameAr : v.Size.Code) : null;

        if (!string.IsNullOrEmpty(colorName) && !string.IsNullOrEmpty(sizeCode))
            return $"{colorName} ({sizeCode})";
        if (!string.IsNullOrEmpty(colorName))
            return colorName;
        if (!string.IsNullOrEmpty(sizeCode))
            return sizeCode;
        return null;
    }
}

public record GetProductStockAvailabilityQuery(int ProductId, int? VariantId = null) : IRequest<APIBaseResponse<ProductStockAvailabilityDto>>;

public class GetProductStockAvailabilityQueryHandler : IRequestHandler<GetProductStockAvailabilityQuery, APIBaseResponse<ProductStockAvailabilityDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProductStockAvailabilityQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<ProductStockAvailabilityDto>> Handle(GetProductStockAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId);
        if (product == null)
            throw new NotFoundException(nameof(Product), request.ProductId);

        ProductVariant? variant = null;
        if (request.VariantId.HasValue)
        {
            variant = await _unitOfWork.ProductVariants.GetByIdAsync(request.VariantId.Value);
        }

        var stocks = await _unitOfWork.BranchStocks.GetProductStockAvailabilityAsync(request.ProductId, request.VariantId);
        var inventories = await _unitOfWork.Inventories.GetAllAsync(false);

        var breakdownList = new List<InventoryStockBreakdownDto>();
        foreach (var inv in inventories)
        {
            var matchStock = stocks.FirstOrDefault(s => s.InventoryId == inv.Id);
            breakdownList.Add(new InventoryStockBreakdownDto
            {
                InventoryId = inv.Id,
                InventoryCode = inv.Code,
                InventoryName = LocalizationHelper.Localize(inv.NameEn, inv.NameAr),
                BranchId = inv.BranchId,
                BranchName = inv.Branch != null ? LocalizationHelper.Localize(inv.Branch.NameEn, inv.Branch.NameAr) : null,
                QuantityOnHand = matchStock?.QuantityOnHand ?? 0,
                MinAlertLevel = matchStock?.MinAlertLevel ?? 2
            });
        }

        var result = new ProductStockAvailabilityDto
        {
            ProductId = product.Id,
            ProductName = LocalizationHelper.Localize(product.NameEn, product.NameAr),
            VariantId = request.VariantId,
            VariantName = GetBranchStocksQueryHandler.FormatVariantName(variant),
            SKU = variant?.SKU ?? product.SKU,
            TotalAvailableAcrossInventories = breakdownList.Sum(b => b.QuantityOnHand),
            InventoriesStock = breakdownList
        };

        return APIBaseResponse<ProductStockAvailabilityDto>.Success(result);
    }
}

public record GetLowStockAlertsQuery(int? BranchId = null, int? InventoryId = null, int PageNumber = 1, int PageSize = 15) : IRequest<APIBaseResponse<PaginatedResultDto<LowStockAlertDto>>>;

public class GetLowStockAlertsQueryHandler : IRequestHandler<GetLowStockAlertsQuery, APIBaseResponse<PaginatedResultDto<LowStockAlertDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetLowStockAlertsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<PaginatedResultDto<LowStockAlertDto>>> Handle(GetLowStockAlertsQuery request, CancellationToken cancellationToken)
    {
        var (lowStocks, totalCount) = await _unitOfWork.BranchStocks.SearchLowStockAlertsAsync(request.BranchId, request.InventoryId, request.PageNumber, request.PageSize);
        var dtos = lowStocks.Select(s => new LowStockAlertDto
        {
            InventoryId = s.InventoryId,
            InventoryName = s.Inventory != null ? LocalizationHelper.Localize(s.Inventory.NameEn, s.Inventory.NameAr) : "N/A",
            BranchId = s.BranchId,
            BranchName = s.Branch != null ? LocalizationHelper.Localize(s.Branch.NameEn, s.Branch.NameAr) : null,
            ProductId = s.ProductId,
            ProductName = LocalizationHelper.Localize(s.Product?.NameEn, s.Product?.NameAr),
            VariantId = s.VariantId,
            VariantName = GetBranchStocksQueryHandler.FormatVariantName(s.Variant),
            QuantityOnHand = s.QuantityOnHand,
            MinAlertLevel = s.MinAlertLevel
        }).ToList();

        var result = new PaginatedResultDto<LowStockAlertDto>(dtos, totalCount, request.PageNumber, request.PageSize);
        return APIBaseResponse<PaginatedResultDto<LowStockAlertDto>>.Success(result);
    }
}

public record GetStockMovementsQuery(int? BranchId, int? ProductId, DateTime? FromDate, DateTime? ToDate, int PageNumber = 1, int PageSize = 20, int? InventoryId = null)
    : IRequest<APIBaseResponse<PaginatedResultDto<StockMovementDto>>>;

public class GetStockMovementsQueryHandler : IRequestHandler<GetStockMovementsQuery, APIBaseResponse<PaginatedResultDto<StockMovementDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetStockMovementsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<PaginatedResultDto<StockMovementDto>>> Handle(GetStockMovementsQuery request, CancellationToken cancellationToken)
    {
        var (movements, totalCount) = await _unitOfWork.BranchStocks.SearchMovementsAsync(
            request.BranchId,
            request.ProductId,
            request.FromDate,
            request.ToDate,
            request.PageNumber,
            request.PageSize,
            request.InventoryId);

        var dtos = movements.Select(m => new StockMovementDto
        {
            Id = m.Id,
            InventoryId = m.InventoryId,
            InventoryName = m.Inventory != null ? LocalizationHelper.Localize(m.Inventory.NameEn, m.Inventory.NameAr) : "N/A",
            ToInventoryId = m.ToInventoryId,
            ToInventoryName = m.ToInventory != null ? LocalizationHelper.Localize(m.ToInventory.NameEn, m.ToInventory.NameAr) : null,
            BranchId = m.BranchId,
            BranchName = m.Branch != null ? LocalizationHelper.Localize(m.Branch.NameEn, m.Branch.NameAr) : null,
            ProductId = m.ProductId,
            ProductName = LocalizationHelper.Localize(m.Product?.NameEn, m.Product?.NameAr),
            VariantId = m.VariantId,
            VariantName = GetBranchStocksQueryHandler.FormatVariantName(m.Variant),
            MovementType = m.MovementType,
            QuantityChange = m.QuantityChange,
            BalanceAfter = m.BalanceAfter,
            ReferenceNumber = m.ReferenceNumber,
            Notes = m.Notes,
            CreatedByUserId = m.CreatedByUserId,
            CreatedByUserName = m.CreatedByUser?.FullName,
            CreatedAt = m.CreatedAt
        }).ToList();

        var result = new PaginatedResultDto<StockMovementDto>(dtos, totalCount, request.PageNumber, request.PageSize);
        return APIBaseResponse<PaginatedResultDto<StockMovementDto>>.Success(result);
    }
}
