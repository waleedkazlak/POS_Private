namespace POS_Private.Domain.Entities;

public class PosRegister : BaseEntity
{
    public string RegisterCode { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }
    public int? InventoryId { get; set; }
    public Inventory? Inventory { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<CashierShift> Shifts { get; set; } = new List<CashierShift>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
