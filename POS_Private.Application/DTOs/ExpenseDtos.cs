using POS_Private.Domain.Enums;

namespace POS_Private.Application.DTOs;

public class ExpenseCategoryDto
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public int ExpensesCount { get; set; }
}

public class CreateExpenseCategoryDto
{
    public string? Code { get; set; }
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; } = 0;
}

public class ExpenseDto
{
    public int Id { get; set; }
    public string ExpenseNumber { get; set; } = null!;
    public int ExpenseCategoryId { get; set; }
    public string CategoryNameEn { get; set; } = null!;
    public string CategoryNameAr { get; set; } = null!;
    public string CategoryCode { get; set; } = null!;
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public int? ShiftId { get; set; }
    public decimal Amount { get; set; }
    public ExpensePaymentMethod PaymentMethod { get; set; }
    public string PaymentMethodName => PaymentMethod.ToString();
    public DateTime ExpenseDate { get; set; }
    public string? PaidTo { get; set; }
    public string Description { get; set; } = null!;
    public string? AttachmentUrl { get; set; }
    public string? ReferenceNumber { get; set; }
    public bool IsDrawerExpense { get; set; }
    public int? ShiftTransactionId { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateExpenseDto
{
    public int ExpenseCategoryId { get; set; }
    public int? BranchId { get; set; }
    public int? ShiftId { get; set; }
    public decimal Amount { get; set; }
    public ExpensePaymentMethod PaymentMethod { get; set; } = ExpensePaymentMethod.Cash;
    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
    public string? PaidTo { get; set; }
    public string Description { get; set; } = null!;
    public string? AttachmentUrl { get; set; }
    public string? ReferenceNumber { get; set; }
    public bool IsDrawerExpense { get; set; } = false; // Deduct from Cashier shift cash drawer!
}

public class UpdateExpenseDto : CreateExpenseDto
{
}

public class ExpenseMetricsDto
{
    public decimal TotalAmount { get; set; }
    public decimal TodayTotal { get; set; }
    public decimal ThisMonthTotal { get; set; }
    public decimal DrawerExpensesTotal { get; set; }
    public int TotalCount { get; set; }
    public List<ExpenseCategorySummaryDto> ByCategory { get; set; } = new();
    public List<ExpensePaymentMethodSummaryDto> ByPaymentMethod { get; set; } = new();
}

public class ExpenseCategorySummaryDto
{
    public int CategoryId { get; set; }
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

public class ExpensePaymentMethodSummaryDto
{
    public ExpensePaymentMethod PaymentMethod { get; set; }
    public string MethodName => PaymentMethod.ToString();
    public decimal TotalAmount { get; set; }
    public int Count { get; set; }
}
