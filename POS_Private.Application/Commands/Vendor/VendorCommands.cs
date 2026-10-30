using POS_Private.Application.DTOs;
using POS_Private.Application.Services;
using POS_Private.Domain.Entities;
using POS_Private.Domain.Enums;
using POS_Private.Domain.Interfaces;
using MediatR;

namespace POS_Private.Application.Commands.Vendor;

public record CreateVendorCommand(CreateVendorDto Dto) : IRequest<APIBaseResponse<int>>;

public class CreateVendorCommandHandler : IRequestHandler<CreateVendorCommand, APIBaseResponse<int>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public CreateVendorCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<APIBaseResponse<int>> Handle(CreateVendorCommand request, CancellationToken cancellationToken)
    {
        var code = !string.IsNullOrWhiteSpace(request.Dto.Code)
            ? request.Dto.Code.Trim().ToUpperInvariant()
            : await _unitOfWork.Vendors.GenerateNextVendorCodeAsync();

        var existing = await _unitOfWork.Vendors.GetByCodeAsync(code);
        if (existing != null)
        {
            return APIBaseResponse<int>.Failure("Vendor code already exists");
        }

        var vendor = new Domain.Entities.Vendor
        {
            Code = code,
            Name = request.Dto.Name.Trim(),
            ContactPerson = request.Dto.ContactPerson,
            PhoneNumber = request.Dto.PhoneNumber,
            Email = request.Dto.Email,
            TaxNumber = request.Dto.TaxNumber,
            CommercialRegister = request.Dto.CommercialRegister,
            Address = request.Dto.Address,
            City = request.Dto.City,
            OpeningBalance = request.Dto.OpeningBalance,
            CurrentBalance = request.Dto.OpeningBalance, // Starting balance
            CreditLimit = request.Dto.CreditLimit,
            PaymentTermsDays = request.Dto.PaymentTermsDays,
            Notes = request.Dto.Notes,
            IsActive = true
        };

        var id = await _unitOfWork.Vendors.AddAsync(vendor);

        // Record opening balance transaction if > 0
        if (vendor.OpeningBalance != 0)
        {
            var tx = new VendorTransaction
            {
                VendorId = id,
                TransactionType = VendorTransactionType.OpeningBalance,
                TransactionDate = DateTime.UtcNow,
                ReferenceNumber = $"OB-{vendor.Code}",
                DebitAmount = vendor.OpeningBalance < 0 ? Math.Abs(vendor.OpeningBalance) : 0,
                CreditAmount = vendor.OpeningBalance > 0 ? vendor.OpeningBalance : 0,
                BalanceAfter = vendor.OpeningBalance,
                Notes = "Initial Opening Balance",
                CreatedByUserId = _currentUser.UserId
            };
            await _unitOfWork.VendorTransactions.AddAsync(tx);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return APIBaseResponse<int>.Success(id, "Vendor created successfully");
    }
}

public record UpdateVendorCommand(int Id, UpdateVendorDto Dto) : IRequest<APIBaseResponse<bool>>;

public class UpdateVendorCommandHandler : IRequestHandler<UpdateVendorCommand, APIBaseResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateVendorCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<bool>> Handle(UpdateVendorCommand request, CancellationToken cancellationToken)
    {
        var vendor = await _unitOfWork.Vendors.GetByIdAsync(request.Id);
        if (vendor == null)
            return APIBaseResponse<bool>.Failure("Vendor not found");

        vendor.Name = request.Dto.Name.Trim();
        vendor.ContactPerson = request.Dto.ContactPerson;
        vendor.PhoneNumber = request.Dto.PhoneNumber;
        vendor.Email = request.Dto.Email;
        vendor.TaxNumber = request.Dto.TaxNumber;
        vendor.CommercialRegister = request.Dto.CommercialRegister;
        vendor.Address = request.Dto.Address;
        vendor.City = request.Dto.City;
        vendor.CreditLimit = request.Dto.CreditLimit;
        vendor.PaymentTermsDays = request.Dto.PaymentTermsDays;
        vendor.Notes = request.Dto.Notes;
        vendor.IsActive = request.Dto.IsActive;

        await _unitOfWork.Vendors.UpdateAsync(vendor);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return APIBaseResponse<bool>.Success(true, "Vendor updated successfully");
    }
}

public record DeleteVendorCommand(int Id) : IRequest<APIBaseResponse<bool>>;

public class DeleteVendorCommandHandler : IRequestHandler<DeleteVendorCommand, APIBaseResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteVendorCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<bool>> Handle(DeleteVendorCommand request, CancellationToken cancellationToken)
    {
        var vendor = await _unitOfWork.Vendors.GetByIdAsync(request.Id);
        if (vendor == null)
            return APIBaseResponse<bool>.Failure("Vendor not found");

        if (vendor.CurrentBalance != 0)
            return APIBaseResponse<bool>.Failure("Cannot delete vendor with non-zero outstanding balance. Deactivate instead.");

        await _unitOfWork.Vendors.DeleteAsync(request.Id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return APIBaseResponse<bool>.Success(true, "Vendor deleted successfully");
    }
}

public record CreateVendorBillCommand(CreateVendorBillDto Dto) : IRequest<APIBaseResponse<int>>;

public class CreateVendorBillCommandHandler : IRequestHandler<CreateVendorBillCommand, APIBaseResponse<int>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IJournalPostingService _journalPostingService;

    public CreateVendorBillCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IJournalPostingService journalPostingService)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _journalPostingService = journalPostingService;
    }

    public async Task<APIBaseResponse<int>> Handle(CreateVendorBillCommand request, CancellationToken cancellationToken)
    {
        var vendor = await _unitOfWork.Vendors.GetByIdAsync(request.Dto.VendorId);
        if (vendor == null)
            return APIBaseResponse<int>.Failure("Vendor not found");

        if (request.Dto.Lines == null || !request.Dto.Lines.Any())
            return APIBaseResponse<int>.Failure("Bill must contain at least one item line");

        var billNumber = await _unitOfWork.VendorBills.GenerateNextBillNumberAsync();
        
        decimal subTotal = 0m;
        var billLines = new List<VendorBillLine>();

        foreach (var item in request.Dto.Lines)
        {
            var lineTax = Math.Round((item.UnitPrice * item.Quantity) * (item.TaxRate / 100m), 2);
            var lineTotal = (item.UnitPrice * item.Quantity) + lineTax;
            subTotal += (item.UnitPrice * item.Quantity);

            billLines.Add(new VendorBillLine
            {
                ProductId = item.ProductId,
                VariantId = item.VariantId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TaxRate = item.TaxRate,
                TaxAmount = lineTax,
                LineTotal = lineTotal,
                ReceivedQuantity = request.Dto.AutoReceiveStock ? item.Quantity : 0,
                Notes = item.Notes
            });
        }

        var totalTax = request.Dto.TaxAmount > 0 ? request.Dto.TaxAmount : billLines.Sum(l => l.TaxAmount);
        var totalAmount = subTotal + totalTax - request.Dto.DiscountAmount;

        var bill = new VendorBill
        {
            BillNumber = billNumber,
            VendorId = vendor.Id,
            BranchId = request.Dto.BranchId,
            InventoryId = request.Dto.InventoryId,
            BillDate = request.Dto.BillDate,
            DueDate = request.Dto.DueDate,
            SubTotal = subTotal,
            TaxAmount = totalTax,
            DiscountAmount = request.Dto.DiscountAmount,
            TotalAmount = totalAmount,
            PaidAmount = 0m,
            RemainingAmount = totalAmount,
            Status = VendorBillStatus.Approved,
            Notes = request.Dto.Notes,
            VendorInvoiceNumber = request.Dto.VendorInvoiceNumber,
            CreatedByUserId = _currentUser.UserId,
            Lines = billLines
        };

        var billId = await _unitOfWork.VendorBills.AddAsync(bill);

        // Update vendor balance (Credit increases what we owe)
        vendor.CurrentBalance += totalAmount;
        await _unitOfWork.Vendors.UpdateAsync(vendor);

        // Write vendor transaction
        var tx = new VendorTransaction
        {
            VendorId = vendor.Id,
            TransactionType = VendorTransactionType.PurchaseBill,
            TransactionDate = request.Dto.BillDate,
            ReferenceNumber = billNumber,
            DebitAmount = 0m,
            CreditAmount = totalAmount,
            BalanceAfter = vendor.CurrentBalance,
            Notes = $"Purchase Bill: {billNumber}" + (!string.IsNullOrWhiteSpace(request.Dto.VendorInvoiceNumber) ? $" (Ref: {request.Dto.VendorInvoiceNumber})" : ""),
            BranchId = request.Dto.BranchId,
            CreatedByUserId = _currentUser.UserId
        };
        await _unitOfWork.VendorTransactions.AddAsync(tx);

        // If AutoReceiveStock, update inventory & stock movements
        if (request.Dto.AutoReceiveStock && request.Dto.BranchId.HasValue)
        {
            int targetInventoryId = 1;
            if (request.Dto.InventoryId.HasValue)
            {
                targetInventoryId = request.Dto.InventoryId.Value;
            }
            else
            {
                var branchInventories = await _unitOfWork.Inventories.GetAllAsync(branchId: request.Dto.BranchId.Value);
                var defaultInv = branchInventories.FirstOrDefault(i => i.IsDefault) ?? branchInventories.FirstOrDefault();
                if (defaultInv != null)
                {
                    targetInventoryId = defaultInv.Id;
                }
            }

            foreach (var line in billLines)
            {
                var stock = await _unitOfWork.BranchStocks.GetStockAsync(request.Dto.BranchId.Value, line.ProductId, line.VariantId);
                if (stock == null)
                {
                    stock = new BranchStock
                    {
                        BranchId = request.Dto.BranchId.Value,
                        InventoryId = targetInventoryId,
                        ProductId = line.ProductId,
                        VariantId = line.VariantId,
                        QuantityOnHand = (int)line.Quantity,
                        MinAlertLevel = 5
                    };
                    await _unitOfWork.BranchStocks.AddOrUpdateStockAsync(stock);
                }
                else
                {
                    stock.QuantityOnHand += (int)line.Quantity;
                    await _unitOfWork.BranchStocks.AddOrUpdateStockAsync(stock);
                }

                await _unitOfWork.BranchStocks.AddStockMovementAsync(new StockMovement
                {
                    BranchId = request.Dto.BranchId.Value,
                    InventoryId = targetInventoryId,
                    ProductId = line.ProductId,
                    VariantId = line.VariantId,
                    MovementType = StockMovementType.VendorPurchase,
                    QuantityChange = (int)line.Quantity,
                    BalanceAfter = stock.QuantityOnHand,
                    ReferenceNumber = billNumber,
                    Notes = $"Received from vendor: {vendor.Name}",
                    CreatedByUserId = _currentUser.UserId,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        // Egyptian Standard: Real-time automated General Ledger posting
        await _journalPostingService.PostVendorBillAsync(bill, vendor, _currentUser.UserId, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return APIBaseResponse<int>.Success(billId, "Vendor bill recorded successfully");
    }
}

public record CreateVendorPaymentCommand(CreateVendorPaymentDto Dto) : IRequest<APIBaseResponse<int>>;

public class CreateVendorPaymentCommandHandler : IRequestHandler<CreateVendorPaymentCommand, APIBaseResponse<int>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IJournalPostingService _journalPostingService;

    public CreateVendorPaymentCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IJournalPostingService journalPostingService)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _journalPostingService = journalPostingService;
    }

    public async Task<APIBaseResponse<int>> Handle(CreateVendorPaymentCommand request, CancellationToken cancellationToken)
    {
        var vendor = await _unitOfWork.Vendors.GetByIdAsync(request.Dto.VendorId);
        if (vendor == null)
            return APIBaseResponse<int>.Failure("Vendor not found");

        if (request.Dto.Amount <= 0)
            return APIBaseResponse<int>.Failure("Payment amount must be greater than zero");

        var paymentNumber = await _unitOfWork.VendorPayments.GenerateNextPaymentNumberAsync();

        var payment = new VendorPayment
        {
            PaymentNumber = paymentNumber,
            VendorId = vendor.Id,
            BranchId = request.Dto.BranchId,
            VendorBillId = request.Dto.VendorBillId,
            Amount = request.Dto.Amount,
            PaymentMethod = request.Dto.PaymentMethod,
            PaymentDate = request.Dto.PaymentDate,
            ReferenceNo = request.Dto.ReferenceNo,
            Notes = request.Dto.Notes,
            CreatedByUserId = _currentUser.UserId
        };

        var paymentId = await _unitOfWork.VendorPayments.AddAsync(payment);

        // If paying against a specific bill, update the bill
        if (request.Dto.VendorBillId.HasValue)
        {
            var bill = await _unitOfWork.VendorBills.GetByIdAsync(request.Dto.VendorBillId.Value);
            if (bill != null)
            {
                bill.PaidAmount += request.Dto.Amount;
                bill.RemainingAmount = Math.Max(0, bill.TotalAmount - bill.PaidAmount);
                if (bill.RemainingAmount == 0)
                    bill.Status = VendorBillStatus.Paid;
                else
                    bill.Status = VendorBillStatus.PartiallyPaid;

                await _unitOfWork.VendorBills.UpdateAsync(bill);
            }
        }

        // Update vendor balance (Debit decreases what we owe)
        vendor.CurrentBalance -= request.Dto.Amount;
        await _unitOfWork.Vendors.UpdateAsync(vendor);

        // Record vendor transaction
        var tx = new VendorTransaction
        {
            VendorId = vendor.Id,
            TransactionType = VendorTransactionType.Payment,
            TransactionDate = request.Dto.PaymentDate,
            ReferenceNumber = paymentNumber,
            DebitAmount = request.Dto.Amount,
            CreditAmount = 0m,
            BalanceAfter = vendor.CurrentBalance,
            Notes = $"Payment Voucher: {paymentNumber}" + (!string.IsNullOrWhiteSpace(request.Dto.ReferenceNo) ? $" (Ref: {request.Dto.ReferenceNo})" : ""),
            BranchId = request.Dto.BranchId,
            CreatedByUserId = _currentUser.UserId
        };
        await _unitOfWork.VendorTransactions.AddAsync(tx);

        // Egyptian Standard: Real-time automated General Ledger posting
        await _journalPostingService.PostVendorPaymentAsync(payment, vendor, _currentUser.UserId, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return APIBaseResponse<int>.Success(paymentId, "Vendor payment recorded successfully");
    }
}
