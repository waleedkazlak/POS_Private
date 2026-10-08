using POS_Private.Application.DTOs;
using POS_Private.Domain.Enums;
using POS_Private.Domain.Interfaces;
using MediatR;

namespace POS_Private.Application.Queries.Expense;

public record GetExpenseCategoriesQuery(bool? IsActive) : IRequest<APIBaseResponse<IEnumerable<ExpenseCategoryDto>>>;

public class GetExpenseCategoriesQueryHandler : IRequestHandler<GetExpenseCategoriesQuery, APIBaseResponse<IEnumerable<ExpenseCategoryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetExpenseCategoriesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<IEnumerable<ExpenseCategoryDto>>> Handle(GetExpenseCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _unitOfWork.ExpenseCategories.GetAllAsync(request.IsActive);
        var dtos = categories.Select(c => new ExpenseCategoryDto
        {
            Id = c.Id,
            Code = c.Code,
            NameEn = c.NameEn,
            NameAr = c.NameAr,
            Description = c.Description,
            DisplayOrder = c.DisplayOrder,
            IsActive = c.IsActive,
            ExpensesCount = c.Expenses?.Count ?? 0
        });

        return APIBaseResponse<IEnumerable<ExpenseCategoryDto>>.Success(dtos);
    }
}

public record GetExpensesQuery(
    string? SearchTerm,
    int? CategoryId,
    int? BranchId,
    int? ShiftId,
    ExpensePaymentMethod? PaymentMethod,
    DateTime? FromDate,
    DateTime? ToDate,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<APIBaseResponse<PaginatedResultDto<ExpenseDto>>>;

public class GetExpensesQueryHandler : IRequestHandler<GetExpensesQuery, APIBaseResponse<PaginatedResultDto<ExpenseDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetExpensesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<PaginatedResultDto<ExpenseDto>>> Handle(GetExpensesQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _unitOfWork.Expenses.SearchAsync(
            request.SearchTerm,
            request.CategoryId,
            request.BranchId,
            request.ShiftId,
            request.PaymentMethod,
            request.FromDate,
            request.ToDate,
            request.PageNumber,
            request.PageSize);

        var dtos = items.Select(e => new ExpenseDto
        {
            Id = e.Id,
            ExpenseNumber = e.ExpenseNumber,
            ExpenseCategoryId = e.ExpenseCategoryId,
            CategoryNameEn = e.ExpenseCategory?.NameEn ?? "",
            CategoryNameAr = e.ExpenseCategory?.NameAr ?? "",
            CategoryCode = e.ExpenseCategory?.Code ?? "",
            BranchId = e.BranchId,
            BranchName = e.Branch?.NameEn,
            ShiftId = e.ShiftId,
            Amount = e.Amount,
            PaymentMethod = e.PaymentMethod,
            ExpenseDate = e.ExpenseDate,
            PaidTo = e.PaidTo,
            Description = e.Description,
            AttachmentUrl = e.AttachmentUrl,
            ReferenceNumber = e.ReferenceNumber,
            IsDrawerExpense = e.IsDrawerExpense,
            CreatedByName = e.CreatedByUser?.FullName,
            CreatedAt = e.CreatedAt
        }).ToList();

        var result = new PaginatedResultDto<ExpenseDto>(dtos, totalCount, request.PageNumber, request.PageSize);
        return APIBaseResponse<PaginatedResultDto<ExpenseDto>>.Success(result);
    }
}

public record GetExpenseByIdQuery(int Id) : IRequest<APIBaseResponse<ExpenseDto>>;

public class GetExpenseByIdQueryHandler : IRequestHandler<GetExpenseByIdQuery, APIBaseResponse<ExpenseDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetExpenseByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<ExpenseDto>> Handle(GetExpenseByIdQuery request, CancellationToken cancellationToken)
    {
        var e = await _unitOfWork.Expenses.GetByIdWithDetailsAsync(request.Id);
        if (e == null)
            return APIBaseResponse<ExpenseDto>.Failure("Expense not found");

        var dto = new ExpenseDto
        {
            Id = e.Id,
            ExpenseNumber = e.ExpenseNumber,
            ExpenseCategoryId = e.ExpenseCategoryId,
            CategoryNameEn = e.ExpenseCategory?.NameEn ?? "",
            CategoryNameAr = e.ExpenseCategory?.NameAr ?? "",
            CategoryCode = e.ExpenseCategory?.Code ?? "",
            BranchId = e.BranchId,
            BranchName = e.Branch?.NameEn,
            ShiftId = e.ShiftId,
            Amount = e.Amount,
            PaymentMethod = e.PaymentMethod,
            ExpenseDate = e.ExpenseDate,
            PaidTo = e.PaidTo,
            Description = e.Description,
            AttachmentUrl = e.AttachmentUrl,
            ReferenceNumber = e.ReferenceNumber,
            IsDrawerExpense = e.IsDrawerExpense,
            CreatedByName = e.CreatedByUser?.FullName,
            CreatedAt = e.CreatedAt
        };

        return APIBaseResponse<ExpenseDto>.Success(dto);
    }
}

public record GetExpenseMetricsQuery(int? BranchId, DateTime? FromDate, DateTime? ToDate) : IRequest<APIBaseResponse<ExpenseMetricsDto>>;

public class GetExpenseMetricsQueryHandler : IRequestHandler<GetExpenseMetricsQuery, APIBaseResponse<ExpenseMetricsDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetExpenseMetricsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<ExpenseMetricsDto>> Handle(GetExpenseMetricsQuery request, CancellationToken cancellationToken)
    {
        var fromDate = request.FromDate ?? DateTime.UtcNow.AddMonths(-1);
        var toDate = request.ToDate ?? DateTime.UtcNow;

        var (items, count) = await _unitOfWork.Expenses.SearchAsync(
            null, null, request.BranchId, null, null, fromDate, toDate, 1, 10000);

        var totalAmount = items.Sum(e => e.Amount);
        var today = DateTime.UtcNow.Date;
        var todayTotal = items.Where(e => e.ExpenseDate.Date == today).Sum(e => e.Amount);
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
        var thisMonthTotal = items.Where(e => e.ExpenseDate >= firstDayOfMonth).Sum(e => e.Amount);
        var drawerTotal = items.Where(e => e.IsDrawerExpense).Sum(e => e.Amount);

        var byCat = items
            .GroupBy(e => new { e.ExpenseCategoryId, NameEn = e.ExpenseCategory?.NameEn ?? "Unknown", NameAr = e.ExpenseCategory?.NameAr ?? "غير محدد" })
            .Select(g => new ExpenseCategorySummaryDto
            {
                CategoryId = g.Key.ExpenseCategoryId,
                NameEn = g.Key.NameEn,
                NameAr = g.Key.NameAr,
                TotalAmount = g.Sum(x => x.Amount),
                Count = g.Count(),
                Percentage = totalAmount > 0 ? Math.Round((g.Sum(x => x.Amount) / totalAmount) * 100m, 1) : 0
            })
            .OrderByDescending(x => x.TotalAmount)
            .ToList();

        var byPay = items
            .GroupBy(e => e.PaymentMethod)
            .Select(g => new ExpensePaymentMethodSummaryDto
            {
                PaymentMethod = g.Key,
                TotalAmount = g.Sum(x => x.Amount),
                Count = g.Count()
            })
            .OrderByDescending(x => x.TotalAmount)
            .ToList();

        var metrics = new ExpenseMetricsDto
        {
            TotalAmount = totalAmount,
            TodayTotal = todayTotal,
            ThisMonthTotal = thisMonthTotal,
            DrawerExpensesTotal = drawerTotal,
            TotalCount = count,
            ByCategory = byCat,
            ByPaymentMethod = byPay
        };

        return APIBaseResponse<ExpenseMetricsDto>.Success(metrics);
    }
}
