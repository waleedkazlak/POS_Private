using POS_Private.Domain.Enums;

namespace POS_Private.Application.DTOs;

public class BranchDto
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? TaxNumber { get; set; }
    public string? CommercialRegistration { get; set; }
    public bool IsActive { get; set; }
    public List<PosRegisterDto> Registers { get; set; } = new();
}

public class CreateBranchDto
{
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? TaxNumber { get; set; }
    public string? CommercialRegistration { get; set; }
    public bool IsActive { get; set; } = true;
}

public class PosRegisterDto
{
    public int Id { get; set; }
    public string RegisterCode { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = null!;
    public int? InventoryId { get; set; }
    public string? InventoryName { get; set; }
    public bool IsActive { get; set; }
}

public class CreatePosRegisterDto
{
    public string RegisterCode { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int BranchId { get; set; }
    public int? InventoryId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CashierShiftDto
{
    public int Id { get; set; }
    public int RegisterId { get; set; }
    public string RegisterName { get; set; } = null!;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = null!;
    public int CashierUserId { get; set; }
    public string CashierName { get; set; } = null!;
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public decimal StartingCash { get; set; }
    public decimal ExpectedCash { get; set; }
    public decimal? ActualCash { get; set; }
    public decimal? Difference { get; set; }
    public ShiftStatus Status { get; set; }
    public string? Notes { get; set; }
    public int InvoicesCount { get; set; }
    public decimal TotalSalesAmount { get; set; }
}

public class OpenShiftDto
{
    public int RegisterId { get; set; }
    public decimal StartingCash { get; set; }
    public string? Notes { get; set; }
}

public class CloseShiftDto
{
    public int ShiftId { get; set; }
    public decimal ActualCash { get; set; }
    public string? Notes { get; set; }
}

public class ShiftTransactionDto
{
    public int Id { get; set; }
    public int ShiftId { get; set; }
    public ShiftTransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = null!;
    public int RecordedByUserId { get; set; }
    public string RecordedByUserName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class AddShiftTransactionDto
{
    public int ShiftId { get; set; }
    public ShiftTransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = null!;
}

public class ShiftSummaryReportDto
{
    public int ShiftId { get; set; }
    public string CashierName { get; set; } = null!;
    public string RegisterName { get; set; } = null!;
    public string BranchName { get; set; } = null!;
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public decimal StartingCash { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalReturns { get; set; }
    public decimal NetSales => TotalSales - TotalReturns;
    public decimal TotalCashSales { get; set; }
    public decimal TotalCashReturns { get; set; }
    public decimal NetCashSales => TotalCashSales - TotalCashReturns;
    public decimal TotalVisaSales { get; set; }
    public decimal TotalInstaPaySales { get; set; }
    public decimal TotalWalletSales { get; set; }
    public decimal CashInTotal { get; set; }
    public decimal CashOutTotal { get; set; }
    public decimal ExpectedDrawerCash => StartingCash + NetCashSales + CashInTotal - CashOutTotal;
    public decimal? ActualDrawerCash { get; set; }
    public decimal? Difference { get; set; }
    public int InvoicesCount { get; set; }
    public int ItemsSoldCount { get; set; }
}
