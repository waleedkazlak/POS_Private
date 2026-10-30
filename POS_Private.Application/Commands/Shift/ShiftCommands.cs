using POS_Private.Application.DTOs;
using POS_Private.Application.Exceptions;
using POS_Private.Application.Services;
using POS_Private.Domain.Entities;
using POS_Private.Domain.Enums;
using POS_Private.Domain.Interfaces;
using MediatR;

namespace POS_Private.Application.Commands.Shift;

public record OpenShiftCommand(OpenShiftDto Dto) : IRequest<APIBaseResponse<int>>;

public class OpenShiftCommandHandler : IRequestHandler<OpenShiftCommand, APIBaseResponse<int>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public OpenShiftCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<APIBaseResponse<int>> Handle(OpenShiftCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? 1;

        // Check if register exists
        var register = await _unitOfWork.PosRegisters.GetByIdAsync(request.Dto.RegisterId);
        if (register == null)
            throw new NotFoundException(nameof(PosRegister), request.Dto.RegisterId);

        // Check if there's already an active shift for this user
        var activeUserShift = await _unitOfWork.CashierShifts.GetActiveShiftForUserAsync(userId);
        if (activeUserShift != null)
            throw new BadRequestException($"User already has an open shift (Shift #{activeUserShift.Id}). Please close it first.");

        // Check if register is currently occupied by another open shift
        var activeRegisterShift = await _unitOfWork.CashierShifts.GetActiveShiftForRegisterAsync(request.Dto.RegisterId);
        if (activeRegisterShift != null)
            throw new BadRequestException($"Register '{register.Name}' already has an open shift (Shift #{activeRegisterShift.Id}) in progress.");

        var shift = new CashierShift
        {
            RegisterId = request.Dto.RegisterId,
            CashierUserId = userId,
            OpenedAt = DateTime.UtcNow,
            StartingCash = request.Dto.StartingCash,
            ExpectedCash = request.Dto.StartingCash,
            Status = ShiftStatus.Open,
            Notes = request.Dto.Notes
        };

        var id = await _unitOfWork.CashierShifts.AddAsync(shift);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return APIBaseResponse<int>.Success(id, "Shift opened successfully");
    }
}

public record CloseShiftCommand(CloseShiftDto Dto) : IRequest<APIBaseResponse<ShiftSummaryReportDto>>;

public class CloseShiftCommandHandler : IRequestHandler<CloseShiftCommand, APIBaseResponse<ShiftSummaryReportDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IJournalPostingService _journalPostingService;

    public CloseShiftCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IJournalPostingService journalPostingService)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _journalPostingService = journalPostingService;
    }

    public async Task<APIBaseResponse<ShiftSummaryReportDto>> Handle(CloseShiftCommand request, CancellationToken cancellationToken)
    {
        var shift = await _unitOfWork.CashierShifts.GetByIdAsync(request.Dto.ShiftId);
        if (shift == null)
            throw new NotFoundException(nameof(CashierShift), request.Dto.ShiftId);

        if (shift.Status == ShiftStatus.Closed)
            throw new BadRequestException("This shift is already closed.");

        var completedInvoices = shift.Invoices.Where(i => i.Status == InvoiceStatus.Completed).ToList();
        
        var saleInvoices = completedInvoices.Where(i => i.InvoiceType == InvoiceType.Sale).ToList();
        var returnInvoices = completedInvoices.Where(i => i.InvoiceType == InvoiceType.Return).ToList();

        var totalSales = saleInvoices.Sum(i => i.TotalAmount);
        var totalReturns = returnInvoices.Sum(i => i.TotalAmount);
        
        var salePayments = saleInvoices.SelectMany(i => i.Payments).ToList();
        var returnPayments = returnInvoices.SelectMany(i => i.Payments).ToList();

        var cashSales = salePayments.Where(p => p.PaymentMethod == PaymentMethod.Cash).Sum(p => p.Amount);
        var cashReturns = returnPayments.Where(p => p.PaymentMethod == PaymentMethod.Cash).Sum(p => p.Amount);
        var netCashSales = cashSales - cashReturns;

        var visaSales = salePayments.Where(p => p.PaymentMethod == PaymentMethod.Visa || p.PaymentMethod == PaymentMethod.MasterCard).Sum(p => p.Amount);
        var instaPaySales = salePayments.Where(p => p.PaymentMethod == PaymentMethod.InstaPay).Sum(p => p.Amount);
        var walletSales = salePayments.Where(p => p.PaymentMethod == PaymentMethod.VodafoneCash || p.PaymentMethod == PaymentMethod.OrangeMoney || p.PaymentMethod == PaymentMethod.EtisalatCash).Sum(p => p.Amount);

        var cashInTotal = shift.Transactions.Where(t => t.Type == ShiftTransactionType.CashIn).Sum(t => t.Amount);
        var cashOutTotal = shift.Transactions.Where(t => t.Type == ShiftTransactionType.CashOut || t.Type == ShiftTransactionType.Drop).Sum(t => t.Amount);

        var expectedCash = shift.StartingCash + netCashSales + cashInTotal - cashOutTotal;
        var diff = request.Dto.ActualCash - expectedCash;

        shift.ClosedAt = DateTime.UtcNow;
        shift.ExpectedCash = expectedCash;
        shift.ActualCash = request.Dto.ActualCash;
        shift.Difference = diff;
        shift.Status = ShiftStatus.Closed;
        shift.Notes = !string.IsNullOrWhiteSpace(request.Dto.Notes) 
            ? (string.IsNullOrWhiteSpace(shift.Notes) ? request.Dto.Notes : $"{shift.Notes} | {request.Dto.Notes}")
            : shift.Notes;

        await _unitOfWork.CashierShifts.UpdateAsync(shift);

        // Egyptian Retail POS Standard: Post consolidated Z-Report daily sales summary to General Ledger
        await _journalPostingService.PostShiftSalesSummaryAsync(
            shift, 
            completedInvoices, 
            request.Dto.ActualCash, 
            diff, 
            _currentUser.UserId, 
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var report = new ShiftSummaryReportDto
        {
            ShiftId = shift.Id,
            CashierName = shift.CashierUser?.FullName ?? string.Empty,
            RegisterName = shift.Register?.Name ?? string.Empty,
            BranchName = shift.Register?.Branch?.NameEn ?? string.Empty,
            OpenedAt = shift.OpenedAt,
            ClosedAt = shift.ClosedAt,
            StartingCash = shift.StartingCash,
            TotalSales = totalSales,
            TotalReturns = totalReturns,
            TotalCashSales = cashSales,
            TotalCashReturns = cashReturns,
            TotalVisaSales = visaSales,
            TotalInstaPaySales = instaPaySales,
            TotalWalletSales = walletSales,
            CashInTotal = cashInTotal,
            CashOutTotal = cashOutTotal,
            ActualDrawerCash = shift.ActualCash,
            Difference = diff,
            InvoicesCount = completedInvoices.Count,
            ItemsSoldCount = saleInvoices.SelectMany(i => i.Lines).Sum(l => l.Quantity)
        };

        return APIBaseResponse<ShiftSummaryReportDto>.Success(report, "Shift closed and reconciled successfully (Z-Report generated)");
    }
}

public record AddShiftTransactionCommand(AddShiftTransactionDto Dto) : IRequest<APIBaseResponse<int>>;

public class AddShiftTransactionCommandHandler : IRequestHandler<AddShiftTransactionCommand, APIBaseResponse<int>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public AddShiftTransactionCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<APIBaseResponse<int>> Handle(AddShiftTransactionCommand request, CancellationToken cancellationToken)
    {
        var shift = await _unitOfWork.CashierShifts.GetByIdAsync(request.Dto.ShiftId);
        if (shift == null)
            throw new NotFoundException(nameof(CashierShift), request.Dto.ShiftId);

        if (shift.Status != ShiftStatus.Open)
            throw new BadRequestException("Cannot add transaction to a closed shift.");

        var userId = _currentUser.UserId ?? 1;

        var transaction = new ShiftTransaction
        {
            ShiftId = request.Dto.ShiftId,
            Type = request.Dto.Type,
            Amount = request.Dto.Amount,
            Reason = request.Dto.Reason,
            RecordedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.CashierShifts.AddTransactionAsync(transaction);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return APIBaseResponse<int>.Success(transaction.Id, "Shift transaction recorded successfully");
    }
}
