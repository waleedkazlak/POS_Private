namespace POS_Private.Domain.Entities;

public class Branch : BaseEntity
{
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string? NameAr { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? TaxNumber { get; set; }
    public string? CommercialRegistration { get; set; }
    public bool IsActive { get; set; } = true;

    public string Name
    {
        get => NameEn;
        set => NameEn = value;
    }

    public ICollection<PosRegister> Registers { get; set; } = new List<PosRegister>();
    public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    public ICollection<BranchStock> BranchStocks { get; set; } = new List<BranchStock>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
