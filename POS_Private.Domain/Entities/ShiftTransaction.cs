using POS_Private.Domain.Enums;

namespace POS_Private.Domain.Entities;

public class ShiftTransaction : BaseEntity
{
    public int ShiftId { get; set; }
    public CashierShift? Shift { get; set; }

    public ShiftTransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = null!;
    
    public int RecordedByUserId { get; set; }
    public User? RecordedByUser { get; set; }
}
