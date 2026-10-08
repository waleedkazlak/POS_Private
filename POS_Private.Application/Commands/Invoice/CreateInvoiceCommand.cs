using POS_Private.Application.DTOs;
using POS_Private.Application.Exceptions;
using POS_Private.Application.Services;
using POS_Private.Domain.Entities;
using POS_Private.Domain.Enums;
using POS_Private.Domain.Interfaces;
using MediatR;

namespace POS_Private.Application.Commands.Invoice;

public record CreateInvoiceCommand(CreateInvoiceDto Dto) : IRequest<APIBaseResponse<InvoiceDto>>;

public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, APIBaseResponse<InvoiceDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public CreateInvoiceCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<APIBaseResponse<InvoiceDto>> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        if (dto.Lines == null || !dto.Lines.Any())
            throw new BadRequestException("Invoice must contain at least one item.");

        if (dto.Payments == null || !dto.Payments.Any())
            throw new BadRequestException("Invoice must have at least one payment method.");

        var cashierUserId = _currentUser.UserId ?? 1;

        // Verify Branch
        var branch = await _unitOfWork.Branches.GetByIdAsync(dto.BranchId);
        if (branch == null)
            throw new NotFoundException(nameof(Branch), dto.BranchId);

        // Resolve Shift (use provided shiftId or active shift for cashier)
        int? shiftId = dto.ShiftId;
        if (!shiftId.HasValue)
        {
            var activeShift = await _unitOfWork.CashierShifts.GetActiveShiftForUserAsync(cashierUserId);
            if (activeShift != null)
                shiftId = activeShift.Id;
        }

        // Customer handling
        int? customerId = dto.CustomerId;
        if (!customerId.HasValue && !string.IsNullOrWhiteSpace(dto.CustomerPhone))
        {
            var cleanPhone = dto.CustomerPhone.Trim();
            var existingCustomer = await _unitOfWork.Customers.GetByPhoneNumberAsync(cleanPhone);
            if (existingCustomer != null)
            {
                customerId = existingCustomer.Id;
            }
            else
            {
                var newCustomer = new Domain.Entities.Customer
                {
                    Code = $"CUST-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    FullName = !string.IsNullOrWhiteSpace(dto.CustomerName) ? dto.CustomerName : $"Customer {cleanPhone}",
                    PhoneNumber = cleanPhone,
                    IsActive = true
                };
                await _unitOfWork.Customers.AddAsync(newCustomer);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                customerId = newCustomer.Id;
            }
        }

        // Build Invoice Lines and calculate SubTotal
        decimal subTotal = 0;
        var invoiceLines = new List<InvoiceLine>();

        foreach (var lineDto in dto.Lines)
        {
            var product = await _unitOfWork.Products.GetByIdWithDetailsAsync(lineDto.ProductId);
            if (product == null)
                throw new NotFoundException(nameof(Product), lineDto.ProductId);

            decimal unitPrice = lineDto.CustomUnitPrice ?? product.BasePrice;
            string productName = product.NameEn;
            string? variantName = null;
            string? sku = product.SKU;

            if (lineDto.VariantId.HasValue)
            {
                var variant = product.Variants.FirstOrDefault(v => v.Id == lineDto.VariantId.Value);
                if (variant != null)
                {
                    if (lineDto.CustomUnitPrice == null && variant.EffectivePrice > 0)
                        unitPrice = variant.EffectivePrice;

                    var colorName = variant.Color?.NameEn;
                    var sizeCode = variant.Size?.Code ?? variant.Size?.NameEn;
                    if (!string.IsNullOrEmpty(colorName) && !string.IsNullOrEmpty(sizeCode))
                        variantName = $"{colorName} / {sizeCode}";
                    else if (!string.IsNullOrEmpty(colorName))
                        variantName = colorName;
                    else if (!string.IsNullOrEmpty(sizeCode))
                        variantName = sizeCode;

                    sku = variant.SKU ?? product.SKU;
                }
            }

            var lineDiscount = lineDto.DiscountAmount;
            var lineTotal = (unitPrice * lineDto.Quantity) - lineDiscount;
            if (lineTotal < 0) lineTotal = 0;

            subTotal += lineTotal;

            invoiceLines.Add(new InvoiceLine
            {
                ProductId = lineDto.ProductId,
                VariantId = lineDto.VariantId,
                ProductName = productName,
                VariantName = variantName,
                SKU = sku,
                UnitPrice = unitPrice,
                CostPrice = product.CostPrice,
                Quantity = lineDto.Quantity,
                DiscountAmount = lineDiscount,
                TotalLineAmount = lineTotal,
                Notes = lineDto.Notes
            });
        }

        // Coupon / Discount handling
        decimal discountAmount = dto.DiscountAmount;
        int? couponId = null;

        if (!string.IsNullOrWhiteSpace(dto.CouponCode))
        {
            var coupon = await _unitOfWork.Coupons.GetByCodeAsync(dto.CouponCode.Trim());
            if (coupon != null && coupon.IsActive)
            {
                var now = DateTime.UtcNow;
                var validDates = (!coupon.StartDate.HasValue || coupon.StartDate.Value <= now) &&
                                 (!coupon.EndDate.HasValue || coupon.EndDate.Value >= now);
                var validUsage = !coupon.UsageLimit.HasValue || coupon.UsageCount < coupon.UsageLimit.Value;
                var validMin = !coupon.MinOrderAmount.HasValue || subTotal >= coupon.MinOrderAmount.Value;

                if (validDates && validUsage && validMin)
                {
                    couponId = coupon.Id;
                    if (coupon.DiscountType == DiscountType.Percentage)
                    {
                        var calculated = subTotal * (coupon.DiscountValue / 100m);
                        if (coupon.MaxDiscountAmount.HasValue && calculated > coupon.MaxDiscountAmount.Value)
                            calculated = coupon.MaxDiscountAmount.Value;
                        discountAmount += calculated;
                    }
                    else
                    {
                        discountAmount += coupon.DiscountValue;
                    }

                    coupon.UsageCount++;
                    await _unitOfWork.Coupons.UpdateAsync(coupon);
                }
            }
        }

        if (discountAmount > subTotal)
            discountAmount = subTotal;

        var netAfterDiscount = subTotal - discountAmount;

        // Egyptian VAT 14%
        var taxPercentage = dto.TaxPercentage >= 0 ? dto.TaxPercentage : 14m;
        var taxAmount = Math.Round(netAfterDiscount * (taxPercentage / 100m), 2);
        var grandTotal = netAfterDiscount + taxAmount;

        // Process Payments
        decimal totalPaid = dto.Payments.Sum(p => p.Amount);
        if (totalPaid < grandTotal)
            throw new BadRequestException($"Insufficient payment: Total is {grandTotal:N2} EGP, but paid amount is {totalPaid:N2} EGP.");

        decimal changeAmount = totalPaid - grandTotal;

        var invoiceNumber = await _unitOfWork.Invoices.GenerateNextInvoiceNumberAsync();

        // Resolve Target Inventory
        POS_Private.Domain.Entities.Inventory? targetInventory = null;
        if (dto.InventoryId.HasValue)
        {
            targetInventory = await _unitOfWork.Inventories.GetByIdAsync(dto.InventoryId.Value);
        }
        else if (dto.RegisterId.HasValue)
        {
            var reg = await _unitOfWork.PosRegisters.GetByIdAsync(dto.RegisterId.Value);
            if (reg?.InventoryId != null)
                targetInventory = await _unitOfWork.Inventories.GetByIdAsync(reg.InventoryId.Value);
        }

        if (targetInventory == null)
        {
            var branchInventories = await _unitOfWork.Inventories.GetAllAsync(false, dto.BranchId);
            targetInventory = branchInventories.FirstOrDefault(i => i.IsDefault) ?? branchInventories.FirstOrDefault();
        }

        if (targetInventory == null)
        {
            var allInv = await _unitOfWork.Inventories.GetAllAsync(false);
            targetInventory = allInv.FirstOrDefault();
        }

        var invoice = new Domain.Entities.Invoice
        {
            InvoiceNumber = invoiceNumber,
            InvoiceType = InvoiceType.Sale,
            Status = InvoiceStatus.Completed,
            InvoiceDate = DateTime.UtcNow,
            BranchId = dto.BranchId,
            InventoryId = targetInventory?.Id,
            RegisterId = dto.RegisterId,
            ShiftId = shiftId,
            CashierUserId = cashierUserId,
            CustomerId = customerId,
            CouponId = couponId,
            SubTotal = subTotal,
            DiscountAmount = discountAmount,
            DiscountType = dto.DiscountType,
            TaxPercentage = taxPercentage,
            TaxAmount = taxAmount,
            TotalAmount = grandTotal,
            PaidAmount = totalPaid,
            ChangeAmount = changeAmount,
            PaymentStatus = PaymentStatus.Paid,
            Notes = dto.Notes,
            EtaUuid = Guid.NewGuid().ToString()
        };

        foreach (var line in invoiceLines)
        {
            line.InventoryId = targetInventory?.Id;
            line.TaxAmount = Math.Round(line.TotalLineAmount * (taxPercentage / 100m), 2);
            invoice.Lines.Add(line);
        }

        foreach (var p in dto.Payments)
        {
            invoice.Payments.Add(new Payment
            {
                PaymentMethod = p.PaymentMethod,
                Amount = p.Amount,
                ReferenceNumber = p.ReferenceNumber,
                PaidAt = DateTime.UtcNow
            });
        }

        // Save Invoice
        await _unitOfWork.Invoices.AddAsync(invoice);

        // Deduct inventory and record stock movements
        foreach (var line in invoiceLines)
        {
            var invId = targetInventory?.Id ?? 1;
            var stock = await _unitOfWork.BranchStocks.GetInventoryStockAsync(invId, line.ProductId, line.VariantId);
            int currentQty = stock?.QuantityOnHand ?? 0;
            int newQty = currentQty - line.Quantity;

            if (stock == null)
            {
                stock = new BranchStock
                {
                    InventoryId = invId,
                    BranchId = targetInventory?.BranchId ?? dto.BranchId,
                    ProductId = line.ProductId,
                    VariantId = line.VariantId,
                    QuantityOnHand = newQty,
                    MinAlertLevel = 2
                };
            }
            else
            {
                stock.QuantityOnHand = newQty;
            }

            await _unitOfWork.BranchStocks.AddOrUpdateStockAsync(stock);

            var movement = new StockMovement
            {
                InventoryId = invId,
                BranchId = targetInventory?.BranchId ?? dto.BranchId,
                ProductId = line.ProductId,
                VariantId = line.VariantId,
                MovementType = StockMovementType.Sale,
                QuantityChange = -line.Quantity,
                BalanceAfter = newQty,
                ReferenceNumber = invoiceNumber,
                Notes = $"POS Sale #{invoiceNumber}",
                CreatedByUserId = cashierUserId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.BranchStocks.AddStockMovementAsync(movement);
        }

        // Update customer statistics & loyalty points
        if (customerId.HasValue)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId.Value);
            if (customer != null)
            {
                customer.TotalSpent += grandTotal;
                customer.LoyaltyPoints += (int)(grandTotal / 50m); // 1 point per 50 EGP
                await _unitOfWork.Customers.UpdateAsync(customer);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Return DTO
        var resultDto = new InvoiceDto
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            InvoiceType = invoice.InvoiceType,
            Status = invoice.Status,
            InvoiceDate = invoice.InvoiceDate,
            BranchId = invoice.BranchId,
            BranchName = branch.NameEn,
            RegisterId = invoice.RegisterId,
            ShiftId = invoice.ShiftId,
            CashierUserId = invoice.CashierUserId,
            CashierName = _currentUser.Username ?? "Cashier",
            CustomerId = invoice.CustomerId,
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

        return APIBaseResponse<InvoiceDto>.Success(resultDto, $"Invoice {invoice.InvoiceNumber} created successfully");
    }
}
