using POS_Private.Application.Common;
using POS_Private.Application.DTOs;
using POS_Private.Application.Exceptions;
using POS_Private.Domain.Entities;
using POS_Private.Domain.Interfaces;
using MediatR;

namespace POS_Private.Application.Queries.Invoice;

public record GetInvoiceByIdQuery(int Id) : IRequest<APIBaseResponse<InvoiceDto>>;

public class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, APIBaseResponse<InvoiceDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetInvoiceByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<InvoiceDto>> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _unitOfWork.Invoices.GetByIdWithDetailsAsync(request.Id);
        if (invoice == null)
            throw new NotFoundException(nameof(Domain.Entities.Invoice), request.Id);

        var dto = MapToDto(invoice);
        return APIBaseResponse<InvoiceDto>.Success(dto);
    }

    private static InvoiceDto MapToDto(Domain.Entities.Invoice invoice)
    {
        return new InvoiceDto
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            InvoiceType = invoice.InvoiceType,
            Status = invoice.Status,
            InvoiceDate = invoice.InvoiceDate,
            BranchId = invoice.BranchId,
            BranchName = LocalizationHelper.Localize(invoice.Branch?.NameEn, invoice.Branch?.NameAr),
            RegisterId = invoice.RegisterId,
            RegisterName = invoice.Register?.Name,
            ShiftId = invoice.ShiftId,
            CashierUserId = invoice.CashierUserId,
            CashierName = invoice.CashierUser?.FullName ?? string.Empty,
            CustomerId = invoice.CustomerId,
            CustomerName = invoice.Customer?.FullName,
            CustomerPhone = invoice.Customer?.PhoneNumber,
            CouponId = invoice.CouponId,
            CouponCode = invoice.Coupon?.Code,
            SubTotal = invoice.SubTotal,
            DiscountAmount = invoice.DiscountAmount,
            DiscountType = invoice.DiscountType,
            TaxPercentage = invoice.TaxPercentage,
            TaxAmount = invoice.TaxAmount,
            TotalAmount = invoice.TotalAmount,
            PaidAmount = invoice.PaidAmount,
            ChangeAmount = invoice.ChangeAmount,
            PaymentStatus = invoice.PaymentStatus,
            Notes = invoice.Notes,
            Lines = invoice.Lines.Select(l => new InvoiceLineDto
            {
                Id = l.Id,
                ProductId = l.ProductId,
                ProductName = l.ProductName,
                VariantId = l.VariantId,
                VariantName = l.VariantName,
                SKU = l.SKU,
                UnitPrice = l.UnitPrice,
                Quantity = l.Quantity,
                DiscountAmount = l.DiscountAmount,
                TaxAmount = l.TaxAmount,
                TotalLineAmount = l.TotalLineAmount,
                Notes = l.Notes
            }).ToList(),
            Payments = invoice.Payments.Select(p => new PaymentDto
            {
                Id = p.Id,
                PaymentMethod = p.PaymentMethod,
                Amount = p.Amount,
                ReferenceNumber = p.ReferenceNumber,
                PaidAt = p.PaidAt
            }).ToList()
        };
    }
}

public record GetInvoiceByNumberQuery(string InvoiceNumber) : IRequest<APIBaseResponse<InvoiceDto>>;

public class GetInvoiceByNumberQueryHandler : IRequestHandler<GetInvoiceByNumberQuery, APIBaseResponse<InvoiceDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetInvoiceByNumberQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<InvoiceDto>> Handle(GetInvoiceByNumberQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _unitOfWork.Invoices.GetByInvoiceNumberAsync(request.InvoiceNumber);
        if (invoice == null)
            throw new NotFoundException("Invoice", request.InvoiceNumber);

        var dto = new InvoiceDto
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            InvoiceType = invoice.InvoiceType,
            Status = invoice.Status,
            InvoiceDate = invoice.InvoiceDate,
            BranchId = invoice.BranchId,
            BranchName = LocalizationHelper.Localize(invoice.Branch?.NameEn, invoice.Branch?.NameAr),
            RegisterId = invoice.RegisterId,
            RegisterName = invoice.Register?.Name,
            ShiftId = invoice.ShiftId,
            CashierUserId = invoice.CashierUserId,
            CashierName = invoice.CashierUser?.FullName ?? string.Empty,
            CustomerId = invoice.CustomerId,
            CustomerName = invoice.Customer?.FullName,
            CustomerPhone = invoice.Customer?.PhoneNumber,
            CouponId = invoice.CouponId,
            CouponCode = invoice.Coupon?.Code,
            SubTotal = invoice.SubTotal,
            DiscountAmount = invoice.DiscountAmount,
            DiscountType = invoice.DiscountType,
            TaxPercentage = invoice.TaxPercentage,
            TaxAmount = invoice.TaxAmount,
            TotalAmount = invoice.TotalAmount,
            PaidAmount = invoice.PaidAmount,
            ChangeAmount = invoice.ChangeAmount,
            PaymentStatus = invoice.PaymentStatus,
            Notes = invoice.Notes,
            Lines = invoice.Lines.Select(l => new InvoiceLineDto
            {
                Id = l.Id,
                ProductId = l.ProductId,
                ProductName = l.ProductName,
                VariantId = l.VariantId,
                VariantName = l.VariantName,
                SKU = l.SKU,
                UnitPrice = l.UnitPrice,
                Quantity = l.Quantity,
                DiscountAmount = l.DiscountAmount,
                TaxAmount = l.TaxAmount,
                TotalLineAmount = l.TotalLineAmount,
                Notes = l.Notes
            }).ToList(),
            Payments = invoice.Payments.Select(p => new PaymentDto
            {
                Id = p.Id,
                PaymentMethod = p.PaymentMethod,
                Amount = p.Amount,
                ReferenceNumber = p.ReferenceNumber,
                PaidAt = p.PaidAt
            }).ToList()
        };

        return APIBaseResponse<InvoiceDto>.Success(dto);
    }
}

public record SearchInvoicesQuery(InvoiceSearchFilterDto Filter) : IRequest<APIBaseResponse<PaginatedResultDto<InvoiceDto>>>;

public class SearchInvoicesQueryHandler : IRequestHandler<SearchInvoicesQuery, APIBaseResponse<PaginatedResultDto<InvoiceDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public SearchInvoicesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<PaginatedResultDto<InvoiceDto>>> Handle(SearchInvoicesQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _unitOfWork.Invoices.SearchAsync(
            request.Filter.SearchTerm,
            request.Filter.BranchId,
            request.Filter.RegisterId,
            request.Filter.ShiftId,
            request.Filter.CashierUserId,
            request.Filter.CustomerId,
            request.Filter.Type,
            request.Filter.Status,
            request.Filter.PaymentStatus,
            request.Filter.FromDate,
            request.Filter.ToDate,
            request.Filter.PageNumber,
            request.Filter.PageSize);

        var dtos = items.Select(invoice => new InvoiceDto
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            InvoiceType = invoice.InvoiceType,
            Status = invoice.Status,
            InvoiceDate = invoice.InvoiceDate,
            BranchId = invoice.BranchId,
            BranchName = LocalizationHelper.Localize(invoice.Branch?.NameEn, invoice.Branch?.NameAr),
            RegisterId = invoice.RegisterId,
            RegisterName = invoice.Register?.Name,
            ShiftId = invoice.ShiftId,
            CashierUserId = invoice.CashierUserId,
            CashierName = invoice.CashierUser?.FullName ?? string.Empty,
            CustomerId = invoice.CustomerId,
            CustomerName = invoice.Customer?.FullName,
            CustomerPhone = invoice.Customer?.PhoneNumber,
            SubTotal = invoice.SubTotal,
            DiscountAmount = invoice.DiscountAmount,
            TaxPercentage = invoice.TaxPercentage,
            TaxAmount = invoice.TaxAmount,
            TotalAmount = invoice.TotalAmount,
            PaidAmount = invoice.PaidAmount,
            ChangeAmount = invoice.ChangeAmount,
            PaymentStatus = invoice.PaymentStatus,
            Notes = invoice.Notes,
            Lines = invoice.Lines.Select(l => new InvoiceLineDto
            {
                Id = l.Id,
                ProductId = l.ProductId,
                ProductName = l.ProductName,
                UnitPrice = l.UnitPrice,
                Quantity = l.Quantity,
                TotalLineAmount = l.TotalLineAmount
            }).ToList(),
            Payments = invoice.Payments.Select(p => new PaymentDto
            {
                Id = p.Id,
                PaymentMethod = p.PaymentMethod,
                Amount = p.Amount,
                ReferenceNumber = p.ReferenceNumber,
                PaidAt = p.PaidAt
            }).ToList()
        }).ToList();

        var result = new PaginatedResultDto<InvoiceDto>(dtos, totalCount, request.Filter.PageNumber, request.Filter.PageSize);
        return APIBaseResponse<PaginatedResultDto<InvoiceDto>>.Success(result);
    }
}

public record GetReceiptQuery(int InvoiceId) : IRequest<APIBaseResponse<ReceiptDto>>;

public class GetReceiptQueryHandler : IRequestHandler<GetReceiptQuery, APIBaseResponse<ReceiptDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetReceiptQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<ReceiptDto>> Handle(GetReceiptQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _unitOfWork.Invoices.GetByIdWithDetailsAsync(request.InvoiceId);
        if (invoice == null)
            throw new NotFoundException(nameof(Domain.Entities.Invoice), request.InvoiceId);

        var branch = invoice.Branch;
        var receipt = new ReceiptDto
        {
            StoreName = "POS_Private",
            StoreAddress = branch?.Address ?? "Cairo, Egypt",
            StorePhone = branch?.Phone ?? "+20 100 000 0000",
            TaxNumber = branch?.TaxNumber ?? "EG-VAT-123456789",
            CommercialRegistration = branch?.CommercialRegistration ?? "CR-98765",
            InvoiceNumber = invoice.InvoiceNumber,
            InvoiceDate = invoice.InvoiceDate,
            CashierName = invoice.CashierUser?.FullName ?? "Cashier",
            CustomerName = invoice.Customer?.FullName,
            CustomerPhone = invoice.Customer?.PhoneNumber,
            SubTotal = invoice.SubTotal,
            DiscountAmount = invoice.DiscountAmount,
            VatPercentage = invoice.TaxPercentage,
            VatAmount = invoice.TaxAmount,
            GrandTotal = invoice.TotalAmount,
            ChangeAmount = invoice.ChangeAmount,
            Items = invoice.Lines.Select(l => new ReceiptItemDto
            {
                Name = l.ProductName,
                Variant = l.VariantName,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                Total = l.TotalLineAmount
            }).ToList(),
            Payments = invoice.Payments.Select(p => new PaymentDto
            {
                Id = p.Id,
                PaymentMethod = p.PaymentMethod,
                Amount = p.Amount,
                ReferenceNumber = p.ReferenceNumber,
                PaidAt = p.PaidAt
            }).ToList()
        };

        return APIBaseResponse<ReceiptDto>.Success(receipt);
    }
}
