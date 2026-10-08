using System.ComponentModel.DataAnnotations.Schema;

namespace POS_Private.Domain.Entities;

public class Inventory : BaseEntity
{
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string? NameAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? Address { get; set; }
    public int? BranchId { get; set; }
    public Branch? Branch { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false;

    [NotMapped]
    public string Name
    {
        get => NameEn;
        set => NameEn = value;
    }

    [NotMapped]
    public string? Description
    {
        get => DescriptionEn;
        set => DescriptionEn = value;
    }

    public ICollection<BranchStock> Stocks { get; set; } = new List<BranchStock>();
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
    public ICollection<PosRegister> PosRegisters { get; set; } = new List<PosRegister>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<InvoiceLine> InvoiceLines { get; set; } = new List<InvoiceLine>();
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<CashierShift> Shifts { get; set; } = new List<CashierShift>();
}
