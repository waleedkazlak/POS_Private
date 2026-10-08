using POS_Private.Domain.Enums;

namespace POS_Private.Domain.Entities;

public class ExpenseCategory : BaseEntity
{
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}

public class Expense : BaseEntity
{
    public string ExpenseNumber { get; set; } = null!;

    public int ExpenseCategoryId { get; set; }
    public ExpenseCategory? ExpenseCategory { get; set; }

    public int? BranchId { get; set; }
    public Branch? Branch { get; set; }

    public int? ShiftId { get; set; }
    public CashierShift? Shift { get; set; }

    public decimal Amount { get; set; }
    public ExpensePaymentMethod PaymentMethod { get; set; } = ExpensePaymentMethod.Cash;
    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;

    public string? PaidTo { get; set; }
    public string Description { get; set; } = null!;
    public string? AttachmentUrl { get; set; }
    public string? ReferenceNumber { get; set; } // Bill / receipt reference number

    public bool IsDrawerExpense { get; set; } = false; // If true, drawn from cashier drawer float
    public int? ShiftTransactionId { get; set; } // Linked ShiftTransaction if drawer expense

    public int? CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }
}
