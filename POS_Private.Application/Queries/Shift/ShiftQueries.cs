using POS_Private.Application.Common;
using POS_Private.Application.DTOs;
using POS_Private.Application.Exceptions;
using POS_Private.Application.Services;
using POS_Private.Domain.Entities;
using POS_Private.Domain.Enums;
using POS_Private.Domain.Interfaces;
using MediatR;

namespace POS_Private.Application.Queries.Shift;

public record GetCurrentShiftQuery() : IRequest<APIBaseResponse<CashierShiftDto?>>;

public class GetCurrentShiftQueryHandler : IRequestHandler<GetCurrentShiftQuery, APIBaseResponse<CashierShiftDto?>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetCurrentShiftQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<APIBaseResponse<CashierShiftDto?>> Handle(GetCurrentShiftQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? 1;
        var shift = await _unitOfWork.CashierShifts.GetActiveShiftForUserAsync(userId);
        if (shift == null)
            return APIBaseResponse<CashierShiftDto?>.Success(null, "No active shift found for current user");

        var completedInvoices = shift.Invoices.Where(i => i.Status == InvoiceStatus.Completed).ToList();

        var dto = new CashierShiftDto
        {
            Id = shift.Id,
            RegisterId = shift.RegisterId,
            RegisterName = shift.Register?.Name ?? string.Empty,
            BranchId = shift.Register?.BranchId ?? 0,
            BranchName = LocalizationHelper.Localize(shift.Register?.Branch?.NameEn, shift.Register?.Branch?.NameAr),
            CashierUserId = shift.CashierUserId,
            CashierName = shift.CashierUser?.FullName ?? string.Empty,
            OpenedAt = shift.OpenedAt,
            ClosedAt = shift.ClosedAt,
            StartingCash = shift.StartingCash,
            ExpectedCash = shift.ExpectedCash,
            ActualCash = shift.ActualCash,
            Difference = shift.Difference,
            Status = shift.Status,
            Notes = shift.Notes,
            InvoicesCount = completedInvoices.Count,
            TotalSalesAmount = completedInvoices.Where(i => i.InvoiceType == InvoiceType.Sale).Sum(i => i.TotalAmount)
        };

        return APIBaseResponse<CashierShiftDto?>.Success(dto);
    }
}

public record GetShiftsQuery(int? BranchId, int? RegisterId, int? CashierUserId, DateTime? FromDate, DateTime? ToDate) 
    : IRequest<APIBaseResponse<IEnumerable<CashierShiftDto>>>;

public class GetShiftsQueryHandler : IRequestHandler<GetShiftsQuery, APIBaseResponse<IEnumerable<CashierShiftDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetShiftsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<IEnumerable<CashierShiftDto>>> Handle(GetShiftsQuery request, CancellationToken cancellationToken)
    {
        var shifts = await _unitOfWork.CashierShifts.GetShiftsAsync(
            request.BranchId,
            request.RegisterId,
            request.CashierUserId,
            request.FromDate,
            request.ToDate);

        var dtos = shifts.Select(s =>
        {
            var completedInvoices = s.Invoices.Where(i => i.Status == InvoiceStatus.Completed).ToList();
            return new CashierShiftDto
            {
                Id = s.Id,
                RegisterId = s.RegisterId,
                RegisterName = s.Register?.Name ?? string.Empty,
                BranchId = s.Register?.BranchId ?? 0,
                BranchName = LocalizationHelper.Localize(s.Register?.Branch?.NameEn, s.Register?.Branch?.NameAr),
                CashierUserId = s.CashierUserId,
                CashierName = s.CashierUser?.FullName ?? string.Empty,
                OpenedAt = s.OpenedAt,
                ClosedAt = s.ClosedAt,
                StartingCash = s.StartingCash,
                ExpectedCash = s.ExpectedCash,
                ActualCash = s.ActualCash,
                Difference = s.Difference,
                Status = s.Status,
                Notes = s.Notes,
                InvoicesCount = completedInvoices.Count,
                TotalSalesAmount = completedInvoices.Where(i => i.InvoiceType == InvoiceType.Sale).Sum(i => i.TotalAmount)
            };
        }).ToList();

        return APIBaseResponse<IEnumerable<CashierShiftDto>>.Success(dtos);
    }
}

public record GetShiftReportQuery(int ShiftId) : IRequest<APIBaseResponse<ShiftSummaryReportDto>>;

public class GetShiftReportQueryHandler : IRequestHandler<GetShiftReportQuery, APIBaseResponse<ShiftSummaryReportDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetShiftReportQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<ShiftSummaryReportDto>> Handle(GetShiftReportQuery request, CancellationToken cancellationToken)
    {
        var shift = await _unitOfWork.CashierShifts.GetByIdAsync(request.ShiftId);
        if (shift == null)
            throw new NotFoundException(nameof(CashierShift), request.ShiftId);

        var completedInvoices = shift.Invoices.Where(i => i.Status == InvoiceStatus.Completed).ToList();
        var totalSales = completedInvoices.Where(i => i.InvoiceType == InvoiceType.Sale).Sum(i => i.TotalAmount);
        var totalReturns = completedInvoices.Where(i => i.InvoiceType == InvoiceType.Return).Sum(i => i.TotalAmount);

        var allPayments = completedInvoices.SelectMany(i => i.Payments).ToList();
        var cashSales = allPayments.Where(p => p.PaymentMethod == PaymentMethod.Cash).Sum(p => p.Amount);
        var visaSales = allPayments.Where(p => p.PaymentMethod == PaymentMethod.Visa || p.PaymentMethod == PaymentMethod.MasterCard).Sum(p => p.Amount);
        var instaPaySales = allPayments.Where(p => p.PaymentMethod == PaymentMethod.InstaPay).Sum(p => p.Amount);
        var walletSales = allPayments.Where(p => p.PaymentMethod == PaymentMethod.VodafoneCash || p.PaymentMethod == PaymentMethod.OrangeMoney || p.PaymentMethod == PaymentMethod.EtisalatCash).Sum(p => p.Amount);

        var cashInTotal = shift.Transactions.Where(t => t.Type == ShiftTransactionType.CashIn).Sum(t => t.Amount);
        var cashOutTotal = shift.Transactions.Where(t => t.Type == ShiftTransactionType.CashOut || t.Type == ShiftTransactionType.Drop).Sum(t => t.Amount);

        var expectedCash = shift.StartingCash + cashSales + cashInTotal - cashOutTotal;

        var report = new ShiftSummaryReportDto
        {
            ShiftId = shift.Id,
            CashierName = shift.CashierUser?.FullName ?? string.Empty,
            RegisterName = shift.Register?.Name ?? string.Empty,
            BranchName = LocalizationHelper.Localize(shift.Register?.Branch?.NameEn, shift.Register?.Branch?.NameAr),
            OpenedAt = shift.OpenedAt,
            ClosedAt = shift.ClosedAt,
            StartingCash = shift.StartingCash,
            TotalSales = totalSales,
            TotalReturns = totalReturns,
            TotalCashSales = cashSales,
            TotalVisaSales = visaSales,
            TotalInstaPaySales = instaPaySales,
            TotalWalletSales = walletSales,
            CashInTotal = cashInTotal,
            CashOutTotal = cashOutTotal,
            ActualDrawerCash = shift.ActualCash,
            Difference = shift.Difference,
            InvoicesCount = completedInvoices.Count,
            ItemsSoldCount = completedInvoices.SelectMany(i => i.Lines).Sum(l => l.Quantity)
        };

        return APIBaseResponse<ShiftSummaryReportDto>.Success(report);
    }
}
