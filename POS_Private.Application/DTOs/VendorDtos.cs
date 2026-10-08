using POS_Private.Domain.Enums;

namespace POS_Private.Application.DTOs;

public class VendorDto
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? ContactPerson { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Phone => PhoneNumber;
    public string? Email { get; set; }
    public string? TaxNumber { get; set; }
    public string? CommercialRegister { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal CurrentBalance { get; set; }
    public decimal? CreditLimit { get; set; }
    public int? PaymentTermsDays { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateVendorDto
{
    public string? Code { get; set; }
    public string Name { get; set; } = null!;
    public string? ContactPerson { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Phone { get => PhoneNumber; set => PhoneNumber = value ?? PhoneNumber; }
    public string? Email { get; set; }
    public string? TaxNumber { get; set; }
    public string? CommercialRegister { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public decimal OpeningBalance { get; set; } = 0m;
    public decimal? CreditLimit { get; set; }
    public int? PaymentTermsDays { get; set; }
    public string? Notes { get; set; }
}

public class UpdateVendorDto : CreateVendorDto
{
    public bool IsActive { get; set; } = true;
}

public class VendorTransactionDto
{
    public int Id { get; set; }
    public int VendorId { get; set; }
    public VendorTransactionType TransactionType { get; set; }
    public string TransactionTypeName => TransactionType.ToString();
    public DateTime TransactionDate { get; set; }
    public string ReferenceNumber { get; set; } = null!;
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string? Notes { get; set; }
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public string? CreatedByName { get; set; }
}

public class VendorStatementDto
{
    public int VendorId { get; set; }
    public string VendorName { get; set; } = null!;
    public string VendorCode { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? TaxNumber { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal TotalDebit { get; set; }   // Payments made
    public decimal TotalCredit { get; set; }  // New bills
    public decimal NetMovement => TotalCredit - TotalDebit;
    public decimal ClosingBalance { get; set; }
    public List<VendorTransactionDto> Transactions { get; set; } = new();
}

public class VendorBillDto
{
    public int Id { get; set; }
    public string BillNumber { get; set; } = null!;
    public int VendorId { get; set; }
    public string VendorName { get; set; } = null!;
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public int? InventoryId { get; set; }
    public string? InventoryName { get; set; }
    public DateTime BillDate { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public VendorBillStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public string? Notes { get; set; }
    public string? VendorInvoiceNumber { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<VendorBillLineDto> Lines { get; set; } = new();
}

public class VendorBillLineDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public int? VariantId { get; set; }
    public string? VariantName { get; set; }
    public string? SKU { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public string? Notes { get; set; }
}

public class CreateVendorBillDto
{
    public int VendorId { get; set; }
    public int? BranchId { get; set; }
    public int? InventoryId { get; set; }
    public DateTime BillDate { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public decimal TaxAmount { get; set; } = 0m;
    public decimal DiscountAmount { get; set; } = 0m;
    public string? Notes { get; set; }
    public string? VendorInvoiceNumber { get; set; }
    public bool AutoReceiveStock { get; set; } = true; // Auto add quantities to BranchStock & StockMovements
    public List<CreateVendorBillLineDto> Lines { get; set; } = new();
}

public class CreateVendorBillLineDto
{
    public int ProductId { get; set; }
    public int? VariantId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; } = 0m;
    public string? Notes { get; set; }
}

public class VendorPaymentDto
{
    public int Id { get; set; }
    public string PaymentNumber { get; set; } = null!;
    public int VendorId { get; set; }
    public string VendorName { get; set; } = null!;
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public int? VendorBillId { get; set; }
    public string? BillNumber { get; set; }
    public decimal Amount { get; set; }
    public ExpensePaymentMethod PaymentMethod { get; set; }
    public string PaymentMethodName => PaymentMethod.ToString();
    public DateTime PaymentDate { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Notes { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateVendorPaymentDto
{
    public int VendorId { get; set; }
    public int? BranchId { get; set; }
    public int? VendorBillId { get; set; }
    public decimal Amount { get; set; }
    public ExpensePaymentMethod PaymentMethod { get; set; } = ExpensePaymentMethod.BankTransfer;
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public string? ReferenceNo { get; set; }
    public string? Notes { get; set; }
}
