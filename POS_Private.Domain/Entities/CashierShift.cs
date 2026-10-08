using POS_Private.Domain.Enums;

namespace POS_Private.Domain.Entities;

public class CashierShift : BaseEntity
{
    public int RegisterId { get; set; }
    public PosRegister? Register { get; set; }

    public int CashierUserId { get; set; }
    public User? CashierUser { get; set; }

    public int? InventoryId { get; set; }
    public Inventory? Inventory { get; set; }

    public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }

    public decimal StartingCash { get; set; }
    public decimal ExpectedCash { get; set; }
    public decimal? ActualCash { get; set; }
    public decimal? Difference { get; set; }

    public ShiftStatus Status { get; set; } = ShiftStatus.Open;
    public string? Notes { get; set; }

    public ICollection<ShiftTransaction> Transactions { get; set; } = new List<ShiftTransaction>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
