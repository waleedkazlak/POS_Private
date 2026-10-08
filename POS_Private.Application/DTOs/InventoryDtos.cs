using POS_Private.Domain.Enums;

namespace POS_Private.Application.DTOs;

public class InventoryDto
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string? NameAr { get; set; }
    public string? Description { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? Address { get; set; }
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public int TotalStockCount { get; set; }
}

public class CreateInventoryDto
{
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string? NameAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? Address { get; set; }
    public int? BranchId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false;
}

public class UpdateInventoryDto
{
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string? NameAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? Address { get; set; }
    public int? BranchId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false;
}

public class BranchStockDto
{
    public int Id { get; set; }
    public int InventoryId { get; set; }
    public string InventoryName { get; set; } = null!;
    public string? InventoryCode { get; set; }
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? SKU { get; set; }
    public string? Barcode { get; set; }
    public int? VariantId { get; set; }
    public string? VariantName { get; set; }
    public decimal Price { get; set; }
    public int QuantityOnHand { get; set; }
    public int MinAlertLevel { get; set; }
    public bool IsLowStock => QuantityOnHand <= MinAlertLevel;
}

public class ProductStockAvailabilityDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public int? VariantId { get; set; }
    public string? VariantName { get; set; }
    public string? SKU { get; set; }
    public int TotalAvailableAcrossInventories { get; set; }
    public List<InventoryStockBreakdownDto> InventoriesStock { get; set; } = new();
}

public class InventoryStockBreakdownDto
{
    public int InventoryId { get; set; }
    public string InventoryCode { get; set; } = null!;
    public string InventoryName { get; set; } = null!;
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public int QuantityOnHand { get; set; }
    public int MinAlertLevel { get; set; }
    public bool IsLowStock => QuantityOnHand <= MinAlertLevel;
}

public class AdjustStockDto
{
    public int? InventoryId { get; set; }
    public int? BranchId { get; set; }
    public int ProductId { get; set; }
    public int? VariantId { get; set; }
    public StockMovementType MovementType { get; set; } = StockMovementType.Adjustment;
    public int QuantityChange { get; set; } // positive or negative
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}

public class TransferStockDto
{
    public int FromInventoryId { get; set; }
    public int ToInventoryId { get; set; }
    public int ProductId { get; set; }
    public int? VariantId { get; set; }
    public int Quantity { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}

public class StockMovementDto
{
    public int Id { get; set; }
    public int InventoryId { get; set; }
    public string InventoryName { get; set; } = null!;
    public int? ToInventoryId { get; set; }
    public string? ToInventoryName { get; set; }
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public int? VariantId { get; set; }
    public string? VariantName { get; set; }
    public StockMovementType MovementType { get; set; }
    public string MovementTypeName => MovementType.ToString();
    public int QuantityChange { get; set; }
    public int BalanceAfter { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public int? CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LowStockAlertDto
{
    public int InventoryId { get; set; }
    public string InventoryName { get; set; } = null!;
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public int? VariantId { get; set; }
    public string? VariantName { get; set; }
    public int QuantityOnHand { get; set; }
    public int MinAlertLevel { get; set; }
}

public class InventorySearchFilterDto
{
    public string? SearchTerm { get; set; }
    public int? BranchId { get; set; }
    public bool? IncludeInactive { get; set; } = false;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 15;
}

public class StockSearchFilterDto
{
    public string? SearchTerm { get; set; }
    public int? BranchId { get; set; }
    public int? InventoryId { get; set; }
    public int? ProductId { get; set; }
    public bool? IsLowStock { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 15;
}
