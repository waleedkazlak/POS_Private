using POS_Private.Application.DTOs;
using POS_Private.Application.Services;
using POS_Private.Domain.Entities;
using POS_Private.Domain.Enums;
using POS_Private.Domain.Interfaces;
using MediatR;

namespace POS_Private.Application.Commands.Expense;

public record CreateExpenseCategoryCommand(CreateExpenseCategoryDto Dto) : IRequest<APIBaseResponse<int>>;

public class CreateExpenseCategoryCommandHandler : IRequestHandler<CreateExpenseCategoryCommand, APIBaseResponse<int>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateExpenseCategoryCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<int>> Handle(CreateExpenseCategoryCommand request, CancellationToken cancellationToken)
    {
        var code = !string.IsNullOrWhiteSpace(request.Dto.Code)
            ? request.Dto.Code.Trim().ToUpperInvariant()
            : "EXP-" + request.Dto.NameEn.Trim().ToUpperInvariant().Replace(" ", "_");

        var existing = await _unitOfWork.ExpenseCategories.GetByCodeAsync(code);
        if (existing != null)
            return APIBaseResponse<int>.Failure("Expense category code already exists");

        var category = new ExpenseCategory
        {
            Code = code,
            NameEn = request.Dto.NameEn.Trim(),
            NameAr = request.Dto.NameAr.Trim(),
            Description = request.Dto.Description,
            DisplayOrder = request.Dto.DisplayOrder,
            IsActive = true
        };

        var id = await _unitOfWork.ExpenseCategories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return APIBaseResponse<int>.Success(id, "Expense category created successfully");
    }
}

public record CreateExpenseCommand(CreateExpenseDto Dto) : IRequest<APIBaseResponse<int>>;

public class CreateExpenseCommandHandler : IRequestHandler<CreateExpenseCommand, APIBaseResponse<int>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IJournalPostingService _journalPostingService;

    public CreateExpenseCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IJournalPostingService journalPostingService)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _journalPostingService = journalPostingService;
    }

    public async Task<APIBaseResponse<int>> Handle(CreateExpenseCommand request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.ExpenseCategories.GetByIdAsync(request.Dto.ExpenseCategoryId);
        if (category == null)
            return APIBaseResponse<int>.Failure("Expense category not found");

        if (request.Dto.Amount <= 0)
            return APIBaseResponse<int>.Failure("Expense amount must be greater than zero");

        var expenseNumber = await _unitOfWork.Expenses.GenerateNextExpenseNumberAsync();

        int? linkedShiftId = request.Dto.ShiftId;

        // If drawn from cashier drawer, verify active shift and log ShiftTransaction CashOut
        if (request.Dto.IsDrawerExpense)
        {
            if (!linkedShiftId.HasValue && _currentUser.UserId.HasValue)
            {
                var activeShift = await _unitOfWork.CashierShifts.GetActiveShiftForUserAsync(_currentUser.UserId.Value);
                if (activeShift != null)
                {
                    linkedShiftId = activeShift.Id;
                }
            }

            if (linkedShiftId.HasValue)
            {
                var shift = await _unitOfWork.CashierShifts.GetByIdAsync(linkedShiftId.Value);
                if (shift != null && shift.Status == ShiftStatus.Open)
                {
                    var shiftTx = new ShiftTransaction
                    {
                        ShiftId = shift.Id,
                        Type = ShiftTransactionType.CashOut,
                        Amount = request.Dto.Amount,
                        Reason = $"Petty Cash Expense: {category.NameEn} - {request.Dto.Description} ({expenseNumber})",
                        RecordedByUserId = _currentUser.UserId ?? shift.CashierUserId,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.CashierShifts.AddTransactionAsync(shiftTx);
                }
            }
        }

        var expense = new Domain.Entities.Expense
        {
            ExpenseNumber = expenseNumber,
            ExpenseCategoryId = category.Id,
            BranchId = request.Dto.BranchId,
            ShiftId = linkedShiftId,
            Amount = request.Dto.Amount,
            PaymentMethod = request.Dto.PaymentMethod,
            ExpenseDate = request.Dto.ExpenseDate,
            PaidTo = request.Dto.PaidTo,
            Description = request.Dto.Description.Trim(),
            AttachmentUrl = request.Dto.AttachmentUrl,
            ReferenceNumber = request.Dto.ReferenceNumber,
            IsDrawerExpense = request.Dto.IsDrawerExpense,
            CreatedByUserId = _currentUser.UserId
        };

        var id = await _unitOfWork.Expenses.AddAsync(expense);

        // Egyptian Standard: Post automated General Ledger entry for operational expense
        await _journalPostingService.PostExpenseAsync(expense, category, _currentUser.UserId, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return APIBaseResponse<int>.Success(id, "Expense recorded successfully");
    }
}

public record DeleteExpenseCommand(int Id) : IRequest<APIBaseResponse<bool>>;

public class DeleteExpenseCommandHandler : IRequestHandler<DeleteExpenseCommand, APIBaseResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteExpenseCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<bool>> Handle(DeleteExpenseCommand request, CancellationToken cancellationToken)
    {
        var expense = await _unitOfWork.Expenses.GetByIdAsync(request.Id);
        if (expense == null)
            return APIBaseResponse<bool>.Failure("Expense not found");

        await _unitOfWork.Expenses.DeleteAsync(request.Id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return APIBaseResponse<bool>.Success(true, "Expense deleted successfully");
    }
}
