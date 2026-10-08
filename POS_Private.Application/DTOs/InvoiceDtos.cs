using POS_Private.Domain.Enums;

namespace POS_Private.Application.DTOs;

public class InvoiceDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = null!;
    public InvoiceType InvoiceType { get; set; }
    public InvoiceStatus Status { get; set; }
    public DateTime InvoiceDate { get; set; }

    public int BranchId { get; set; }
    public string BranchName { get; set; } = null!;

    public int? InventoryId { get; set; }
    public string? InventoryName { get; set; }

    public int? RegisterId { get; set; }
    public string? RegisterName { get; set; }

    public int? ShiftId { get; set; }

    public int CashierUserId { get; set; }
    public string CashierName { get; set; } = null!;

    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }

    public int? CouponId { get; set; }
    public string? CouponCode { get; set; }

    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public DiscountType? DiscountType { get; set; }

    public decimal TaxPercentage { get; set; }
    public decimal TaxAmount { get; set; }

    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal ChangeAmount { get; set; }

    public PaymentStatus PaymentStatus { get; set; }
    public string? Notes { get; set; }

    public List<InvoiceLineDto> Lines { get; set; } = new();
    public List<PaymentDto> Payments { get; set; } = new();
}

public class InvoiceLineDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public int? VariantId { get; set; }
    public string? VariantName { get; set; }
    public int? InventoryId { get; set; }
    public string? InventoryName { get; set; }
    public string? SKU { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalLineAmount { get; set; }
    public string? Notes { get; set; }
}

public class PaymentDto
{
    public int Id { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string PaymentMethodName => PaymentMethod.ToString();
    public decimal Amount { get; set; }
    public string? ReferenceNumber { get; set; }
    public DateTime PaidAt { get; set; }
}

public class CreateInvoiceDto
{
    public int BranchId { get; set; }
    public int? InventoryId { get; set; }
    public int? RegisterId { get; set; }
    public int? ShiftId { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerPhone { get; set; } // If customer not registered, auto-find or create by phone
    public string? CustomerName { get; set; }
    
    public string? CouponCode { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public DiscountType? DiscountType { get; set; }
    
    public decimal TaxPercentage { get; set; } = 14m; // 14% Egyptian VAT
    
    public string? Notes { get; set; }

    public List<CreateInvoiceLineDto> Lines { get; set; } = new();
    public List<CreatePaymentDto> Payments { get; set; } = new();
}

public class CreateInvoiceLineDto
{
    public int ProductId { get; set; }
    public int? VariantId { get; set; }
    public int? InventoryId { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal? CustomUnitPrice { get; set; } // Optional manual price override if authorized
    public decimal DiscountAmount { get; set; } = 0;
    public string? Notes { get; set; }
}

public class CreatePaymentDto
{
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public string? ReferenceNumber { get; set; } // InstaPay ref, Visa auth, etc.
}

public class InvoiceSearchFilterDto
{
    public string? SearchTerm { get; set; }
    public int? BranchId { get; set; }
    public int? RegisterId { get; set; }
    public int? ShiftId { get; set; }
    public int? CashierUserId { get; set; }
    public int? CustomerId { get; set; }
    public InvoiceType? Type { get; set; }
    public InvoiceStatus? Status { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ReceiptDto
{
    public string StoreName { get; set; } = "POS_Private";
    public string StoreAddress { get; set; } = null!;
    public string StorePhone { get; set; } = null!;
    public string TaxNumber { get; set; } = null!;
    public string CommercialRegistration { get; set; } = null!;
    public string InvoiceNumber { get; set; } = null!;
    public DateTime InvoiceDate { get; set; }
    public string CashierName { get; set; } = null!;
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public List<ReceiptItemDto> Items { get; set; } = new();
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal VatPercentage { get; set; } = 14m;
    public decimal VatAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public List<PaymentDto> Payments { get; set; } = new();
    public decimal ChangeAmount { get; set; }
    public string FooterMessage { get; set; } = "Thank you for shopping with POS_Private! Returns within 14 days with original receipt.";
}

public class ReceiptItemDto
{
    public string Name { get; set; } = null!;
    public string? Variant { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
}

public class InvoiceReturnDto
{
    public int Id { get; set; }
    public int OriginalInvoiceId { get; set; }
    public string OriginalInvoiceNumber { get; set; } = null!;
    public string ReturnNumber { get; set; } = null!;
    public string? ReturnReason { get; set; }
    public PaymentMethod RefundMethod { get; set; }
    public decimal RefundAmount { get; set; }
    public int ProcessedByUserId { get; set; }
    public string ProcessedByName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public List<InvoiceReturnLineDto> Lines { get; set; } = new();
}

public class InvoiceReturnLineDto
{
    public int Id { get; set; }
    public int InvoiceLineId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal RefundAmount { get; set; }
    public bool Restock { get; set; }
}

public class CreateReturnDto
{
    public int OriginalInvoiceId { get; set; }
    public string? ReturnReason { get; set; }
    public PaymentMethod RefundMethod { get; set; } = PaymentMethod.Cash;
    public List<CreateReturnLineDto> Lines { get; set; } = new();
}

public class CreateReturnLineDto
{
    public int InvoiceLineId { get; set; }
    public int Quantity { get; set; }
    public bool Restock { get; set; } = true;
}
