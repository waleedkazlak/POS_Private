namespace POS_Private.Application.DTOs;

public class DashboardMetricsDto
{
    public decimal TodayTotalSales { get; set; }
    public int TodayInvoicesCount { get; set; }
    public decimal TodayTotalReturns { get; set; }
    public decimal TodayNetSales => TodayTotalSales - TodayTotalReturns;

    public decimal MonthTotalSales { get; set; }
    public int MonthInvoicesCount { get; set; }

    public int TotalProductsCount { get; set; }
    public int TotalCustomersCount { get; set; }
    public int ActiveShiftsCount { get; set; }
    public int LowStockAlertsCount { get; set; }

    public List<PaymentMethodSummaryDto> PaymentBreakdown { get; set; } = new();
    public List<TopProductDto> TopSellingProducts { get; set; } = new();
    public List<SalesChartPointDto> RecentSalesChart { get; set; } = new();
}

public class PaymentMethodSummaryDto
{
    public string PaymentMethod { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public int TransactionsCount { get; set; }
    public double Percentage { get; set; }
}

public class TopProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? PictureUrl { get; set; }
    public int TotalQuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class SalesChartPointDto
{
    public string DateLabel { get; set; } = null!;
    public decimal SalesAmount { get; set; }
    public int InvoicesCount { get; set; }
}
