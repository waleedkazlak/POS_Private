using POS_Private.Domain.Enums;

namespace POS_Private.Domain.Entities;

public class BranchStock : BaseEntity
{
    public int InventoryId { get; set; }
    public Inventory? Inventory { get; set; }

    public int? BranchId { get; set; }
    public Branch? Branch { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int? VariantId { get; set; }
    public ProductVariant? Variant { get; set; }

    public int QuantityOnHand { get; set; } = 0;
    public int MinAlertLevel { get; set; } = 2;
    public int? MaxLevel { get; set; }
}

public class StockMovement : BaseEntity
{
    public int InventoryId { get; set; }
    public Inventory? Inventory { get; set; }

    public int? ToInventoryId { get; set; }
    public Inventory? ToInventory { get; set; }

    public int? BranchId { get; set; }
    public Branch? Branch { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int? VariantId { get; set; }
    public ProductVariant? Variant { get; set; }

    public StockMovementType MovementType { get; set; }
    public int QuantityChange { get; set; }
    public int BalanceAfter { get; set; }
    
    public string? ReferenceNumber { get; set; } // e.g. Invoice #, Transfer #
    public string? Notes { get; set; }
    
    public int? CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }
}
