using POS_Private.Application.Common;
using POS_Private.Application.DTOs;
using POS_Private.Domain.Entities;
using POS_Private.Domain.Enums;
using POS_Private.Domain.Interfaces;
using MediatR;

namespace POS_Private.Application.Queries.Dashboard;

public record GetDashboardMetricsQuery(int? BranchId) : IRequest<APIBaseResponse<DashboardMetricsDto>>;

public class GetDashboardMetricsQueryHandler : IRequestHandler<GetDashboardMetricsQuery, APIBaseResponse<DashboardMetricsDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDashboardMetricsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<DashboardMetricsDto>> Handle(GetDashboardMetricsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var startOfToday = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var (todayInvoices, _) = await _unitOfWork.Invoices.SearchAsync(
            null, request.BranchId, null, null, null, null, null, InvoiceStatus.Completed, null, startOfToday, null, 1, 1000);

        var (monthInvoices, _) = await _unitOfWork.Invoices.SearchAsync(
            null, request.BranchId, null, null, null, null, null, InvoiceStatus.Completed, null, startOfMonth, null, 1, 10000);

        var todaySales = todayInvoices.Where(i => i.InvoiceType == InvoiceType.Sale).Sum(i => i.TotalAmount);
        var todayReturns = todayInvoices.Where(i => i.InvoiceType == InvoiceType.Return).Sum(i => i.TotalAmount);

        var monthSales = monthInvoices.Where(i => i.InvoiceType == InvoiceType.Sale).Sum(i => i.TotalAmount);

        var allProducts = await _unitOfWork.Products.GetAllAsync();
        var (_, totalCustomers) = await _unitOfWork.Customers.SearchAsync(null, 1, 1);
        var lowStocks = await _unitOfWork.BranchStocks.GetLowStockAlertsAsync(request.BranchId);
        var activeShifts = await _unitOfWork.CashierShifts.GetShiftsAsync(request.BranchId, null, null, startOfToday, null);

        // Payment Breakdown for current month
        var payments = monthInvoices.SelectMany(i => i.Payments).ToList();
        var totalPaymentSum = payments.Sum(p => p.Amount);

        var paymentBreakdown = payments
            .GroupBy(p => p.PaymentMethod)
            .Select(g => new PaymentMethodSummaryDto
            {
                PaymentMethod = g.Key.ToString(),
                TotalAmount = g.Sum(p => p.Amount),
                TransactionsCount = g.Count(),
                Percentage = totalPaymentSum > 0 ? (double)Math.Round((g.Sum(p => p.Amount) / totalPaymentSum) * 100m, 2) : 0
            }).ToList();

        // Top Selling Products
        var topProducts = monthInvoices
            .SelectMany(i => i.Lines)
            .GroupBy(l => new { l.ProductId, l.ProductName })
            .Select(g => new TopProductDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.ProductName,
                TotalQuantitySold = g.Sum(l => l.Quantity),
                TotalRevenue = g.Sum(l => l.TotalLineAmount)
            })
            .OrderByDescending(p => p.TotalRevenue)
            .Take(5)
            .ToList();

        // Populate picture URLs & localized names for top products
        foreach (var tp in topProducts)
        {
            var p = allProducts.FirstOrDefault(prod => prod.Id == tp.ProductId);
            if (p != null)
            {
                tp.PictureUrl = p.PictureUrl;
                tp.ProductName = LocalizationHelper.Localize(p.NameEn, p.NameAr);
            }
        }

        // Recent 7 days sales chart
        var salesChart = new List<SalesChartPointDto>();
        for (int i = 6; i >= 0; i--)
        {
            var day = startOfToday.AddDays(-i);
            var nextDay = day.AddDays(1);
            var dayInvoices = monthInvoices.Where(inv => inv.InvoiceDate >= day && inv.InvoiceDate < nextDay).ToList();

            salesChart.Add(new SalesChartPointDto
            {
                DateLabel = day.ToString("ddd, dd MMM"),
                SalesAmount = dayInvoices.Where(inv => inv.InvoiceType == InvoiceType.Sale).Sum(inv => inv.TotalAmount),
                InvoicesCount = dayInvoices.Count
            });
        }

        var metrics = new DashboardMetricsDto
        {
            TodayTotalSales = todaySales,
            TodayInvoicesCount = todayInvoices.Count(),
            TodayTotalReturns = todayReturns,
            MonthTotalSales = monthSales,
            MonthInvoicesCount = monthInvoices.Count(),
            TotalProductsCount = allProducts.Count(),
            TotalCustomersCount = totalCustomers,
            ActiveShiftsCount = activeShifts.Count(s => s.Status == ShiftStatus.Open),
            LowStockAlertsCount = lowStocks.Count(),
            PaymentBreakdown = paymentBreakdown,
            TopSellingProducts = topProducts,
            RecentSalesChart = salesChart
        };

        return APIBaseResponse<DashboardMetricsDto>.Success(metrics);
    }
}
