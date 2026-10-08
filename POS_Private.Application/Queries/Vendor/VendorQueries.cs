using POS_Private.Application.DTOs;
using POS_Private.Domain.Enums;
using POS_Private.Domain.Interfaces;
using MediatR;

namespace POS_Private.Application.Queries.Vendor;

public record GetVendorsQuery(
    string? SearchTerm,
    bool? IsActive,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<APIBaseResponse<PaginatedResultDto<VendorDto>>>;

public class GetVendorsQueryHandler : IRequestHandler<GetVendorsQuery, APIBaseResponse<PaginatedResultDto<VendorDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetVendorsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<PaginatedResultDto<VendorDto>>> Handle(GetVendorsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _unitOfWork.Vendors.SearchAsync(
            request.SearchTerm,
            request.IsActive,
            request.PageNumber,
            request.PageSize);

        var dtos = items.Select(v => new VendorDto
        {
            Id = v.Id,
            Code = v.Code,
            Name = v.Name,
            ContactPerson = v.ContactPerson,
            PhoneNumber = v.PhoneNumber,
            Email = v.Email,
            TaxNumber = v.TaxNumber,
            CommercialRegister = v.CommercialRegister,
            Address = v.Address,
            City = v.City,
            OpeningBalance = v.OpeningBalance,
            CurrentBalance = v.CurrentBalance,
            CreditLimit = v.CreditLimit,
            PaymentTermsDays = v.PaymentTermsDays,
            Notes = v.Notes,
            IsActive = v.IsActive,
            CreatedAt = v.CreatedAt
        }).ToList();

        var result = new PaginatedResultDto<VendorDto>(dtos, totalCount, request.PageNumber, request.PageSize);
        return APIBaseResponse<PaginatedResultDto<VendorDto>>.Success(result);
    }
}

public record GetVendorByIdQuery(int Id) : IRequest<APIBaseResponse<VendorDto>>;

public class GetVendorByIdQueryHandler : IRequestHandler<GetVendorByIdQuery, APIBaseResponse<VendorDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetVendorByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<VendorDto>> Handle(GetVendorByIdQuery request, CancellationToken cancellationToken)
    {
        var v = await _unitOfWork.Vendors.GetByIdAsync(request.Id);
        if (v == null)
            return APIBaseResponse<VendorDto>.Failure("Vendor not found");

        var dto = new VendorDto
        {
            Id = v.Id,
            Code = v.Code,
            Name = v.Name,
            ContactPerson = v.ContactPerson,
            PhoneNumber = v.PhoneNumber,
            Email = v.Email,
            TaxNumber = v.TaxNumber,
            CommercialRegister = v.CommercialRegister,
            Address = v.Address,
            City = v.City,
            OpeningBalance = v.OpeningBalance,
            CurrentBalance = v.CurrentBalance,
            CreditLimit = v.CreditLimit,
            PaymentTermsDays = v.PaymentTermsDays,
            Notes = v.Notes,
            IsActive = v.IsActive,
            CreatedAt = v.CreatedAt
        };

        return APIBaseResponse<VendorDto>.Success(dto);
    }
}

public record GetVendorStatementQuery(
    int VendorId,
    DateTime? FromDate,
    DateTime? ToDate) : IRequest<APIBaseResponse<VendorStatementDto>>;

public class GetVendorStatementQueryHandler : IRequestHandler<GetVendorStatementQuery, APIBaseResponse<VendorStatementDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetVendorStatementQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<VendorStatementDto>> Handle(GetVendorStatementQuery request, CancellationToken cancellationToken)
    {
        var vendor = await _unitOfWork.Vendors.GetByIdAsync(request.VendorId);
        if (vendor == null)
            return APIBaseResponse<VendorStatementDto>.Failure("Vendor not found");

        var fromDate = request.FromDate ?? DateTime.UtcNow.AddMonths(-3);
        var toDate = request.ToDate ?? DateTime.UtcNow;

        // Opening balance before start date
        var openingBalance = await _unitOfWork.VendorTransactions.GetBalanceBeforeDateAsync(vendor.Id, fromDate);

        // Transactions in date range
        var transactions = await _unitOfWork.VendorTransactions.GetByVendorIdAsync(vendor.Id, fromDate, toDate);

        decimal running = openingBalance;
        var txDtos = new List<VendorTransactionDto>();
        decimal totalDebit = 0m;
        decimal totalCredit = 0m;

        foreach (var t in transactions.OrderBy(x => x.TransactionDate).ThenBy(x => x.Id))
        {
            totalDebit += t.DebitAmount;
            totalCredit += t.CreditAmount;
            running = running + t.CreditAmount - t.DebitAmount;

            txDtos.Add(new VendorTransactionDto
            {
                Id = t.Id,
                VendorId = t.VendorId,
                TransactionType = t.TransactionType,
                TransactionDate = t.TransactionDate,
                ReferenceNumber = t.ReferenceNumber,
                DebitAmount = t.DebitAmount,
                CreditAmount = t.CreditAmount,
                BalanceAfter = running,
                Notes = t.Notes,
                BranchId = t.BranchId,
                BranchName = t.Branch?.NameEn,
                CreatedByName = t.CreatedByUser?.FullName
            });
        }

        var statement = new VendorStatementDto
        {
            VendorId = vendor.Id,
            VendorName = vendor.Name,
            VendorCode = vendor.Code,
            PhoneNumber = vendor.PhoneNumber,
            TaxNumber = vendor.TaxNumber,
            FromDate = fromDate,
            ToDate = toDate,
            OpeningBalance = openingBalance,
            TotalDebit = totalDebit,
            TotalCredit = totalCredit,
            ClosingBalance = running,
            Transactions = txDtos
        };

        return APIBaseResponse<VendorStatementDto>.Success(statement);
    }
}

public record GetVendorBillsQuery(
    string? SearchTerm,
    int? VendorId,
    int? BranchId,
    VendorBillStatus? Status,
    DateTime? FromDate,
    DateTime? ToDate,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<APIBaseResponse<PaginatedResultDto<VendorBillDto>>>;

public class GetVendorBillsQueryHandler : IRequestHandler<GetVendorBillsQuery, APIBaseResponse<PaginatedResultDto<VendorBillDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetVendorBillsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<PaginatedResultDto<VendorBillDto>>> Handle(GetVendorBillsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _unitOfWork.VendorBills.SearchAsync(
            request.SearchTerm,
            request.VendorId,
            request.BranchId,
            request.Status,
            request.FromDate,
            request.ToDate,
            request.PageNumber,
            request.PageSize);

        var dtos = items.Select(b => new VendorBillDto
        {
            Id = b.Id,
            BillNumber = b.BillNumber,
            VendorId = b.VendorId,
            VendorName = b.Vendor?.Name ?? "",
            BranchId = b.BranchId,
            BranchName = b.Branch?.NameEn,
            InventoryId = b.InventoryId,
            InventoryName = b.Inventory?.NameEn,
            BillDate = b.BillDate,
            DueDate = b.DueDate,
            SubTotal = b.SubTotal,
            TaxAmount = b.TaxAmount,
            DiscountAmount = b.DiscountAmount,
            TotalAmount = b.TotalAmount,
            PaidAmount = b.PaidAmount,
            RemainingAmount = b.RemainingAmount,
            Status = b.Status,
            Notes = b.Notes,
            VendorInvoiceNumber = b.VendorInvoiceNumber,
            CreatedByName = b.CreatedByUser?.FullName,
            CreatedAt = b.CreatedAt,
            Lines = b.Lines.Select(l => new VendorBillLineDto
            {
                Id = l.Id,
                ProductId = l.ProductId,
                ProductName = l.Product?.NameEn ?? "",
                VariantId = l.VariantId,
                VariantName = l.Variant != null ? $"{l.Variant.Color?.NameEn} / {l.Variant.Size?.Code}" : null,
                SKU = l.Variant?.SKU ?? l.Product?.SKU,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                TaxRate = l.TaxRate,
                TaxAmount = l.TaxAmount,
                LineTotal = l.LineTotal,
                ReceivedQuantity = l.ReceivedQuantity,
                Notes = l.Notes
            }).ToList()
        }).ToList();

        var result = new PaginatedResultDto<VendorBillDto>(dtos, totalCount, request.PageNumber, request.PageSize);
        return APIBaseResponse<PaginatedResultDto<VendorBillDto>>.Success(result);
    }
}

public record GetVendorBillByIdQuery(int Id) : IRequest<APIBaseResponse<VendorBillDto>>;

public class GetVendorBillByIdQueryHandler : IRequestHandler<GetVendorBillByIdQuery, APIBaseResponse<VendorBillDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetVendorBillByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<VendorBillDto>> Handle(GetVendorBillByIdQuery request, CancellationToken cancellationToken)
    {
        var b = await _unitOfWork.VendorBills.GetByIdWithDetailsAsync(request.Id);
        if (b == null)
            return APIBaseResponse<VendorBillDto>.Failure("Vendor bill not found");

        var dto = new VendorBillDto
        {
            Id = b.Id,
            BillNumber = b.BillNumber,
            VendorId = b.VendorId,
            VendorName = b.Vendor?.Name ?? "",
            BranchId = b.BranchId,
            BranchName = b.Branch?.NameEn,
            InventoryId = b.InventoryId,
            InventoryName = b.Inventory?.NameEn,
            BillDate = b.BillDate,
            DueDate = b.DueDate,
            SubTotal = b.SubTotal,
            TaxAmount = b.TaxAmount,
            DiscountAmount = b.DiscountAmount,
            TotalAmount = b.TotalAmount,
            PaidAmount = b.PaidAmount,
            RemainingAmount = b.RemainingAmount,
            Status = b.Status,
            Notes = b.Notes,
            VendorInvoiceNumber = b.VendorInvoiceNumber,
            CreatedByName = b.CreatedByUser?.FullName,
            CreatedAt = b.CreatedAt,
            Lines = b.Lines.Select(l => new VendorBillLineDto
            {
                Id = l.Id,
                ProductId = l.ProductId,
                ProductName = l.Product?.NameEn ?? "",
                VariantId = l.VariantId,
                VariantName = l.Variant != null ? $"{l.Variant.Color?.NameEn} / {l.Variant.Size?.Code}" : null,
                SKU = l.Variant?.SKU ?? l.Product?.SKU,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                TaxRate = l.TaxRate,
                TaxAmount = l.TaxAmount,
                LineTotal = l.LineTotal,
                ReceivedQuantity = l.ReceivedQuantity,
                Notes = l.Notes
            }).ToList()
        };

        return APIBaseResponse<VendorBillDto>.Success(dto);
    }
}

public record GetVendorPaymentsQuery(int VendorId) : IRequest<APIBaseResponse<IEnumerable<VendorPaymentDto>>>;

public class GetVendorPaymentsQueryHandler : IRequestHandler<GetVendorPaymentsQuery, APIBaseResponse<IEnumerable<VendorPaymentDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetVendorPaymentsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<IEnumerable<VendorPaymentDto>>> Handle(GetVendorPaymentsQuery request, CancellationToken cancellationToken)
    {
        var payments = await _unitOfWork.VendorPayments.GetByVendorIdAsync(request.VendorId);
        var dtos = payments.Select(p => new VendorPaymentDto
        {
            Id = p.Id,
            PaymentNumber = p.PaymentNumber,
            VendorId = p.VendorId,
            VendorName = p.Vendor?.Name ?? "",
            BranchId = p.BranchId,
            BranchName = p.Branch?.NameEn,
            VendorBillId = p.VendorBillId,
            BillNumber = p.VendorBill?.BillNumber,
            Amount = p.Amount,
            PaymentMethod = p.PaymentMethod,
            PaymentDate = p.PaymentDate,
            ReferenceNo = p.ReferenceNo,
            Notes = p.Notes,
            CreatedByName = p.CreatedByUser?.FullName,
            CreatedAt = p.CreatedAt
        });

        return APIBaseResponse<IEnumerable<VendorPaymentDto>>.Success(dtos);
    }
}
