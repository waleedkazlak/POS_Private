using POS_Private.Application.DTOs;
using POS_Private.Application.Exceptions;
using POS_Private.Application.Services;
using POS_Private.Domain.Entities;
using POS_Private.Domain.Enums;
using POS_Private.Domain.Interfaces;
using MediatR;

namespace POS_Private.Application.Commands.Invoice;

public record ProcessReturnCommand(CreateReturnDto Dto) : IRequest<APIBaseResponse<InvoiceReturnDto>>;

public class ProcessReturnCommandHandler : IRequestHandler<ProcessReturnCommand, APIBaseResponse<InvoiceReturnDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ProcessReturnCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<APIBaseResponse<InvoiceReturnDto>> Handle(ProcessReturnCommand request, CancellationToken cancellationToken)
    {
        var originalInvoice = await _unitOfWork.Invoices.GetByIdWithDetailsAsync(request.Dto.OriginalInvoiceId);
        if (originalInvoice == null)
            throw new NotFoundException(nameof(Domain.Entities.Invoice), request.Dto.OriginalInvoiceId);

        if (!request.Dto.Lines.Any())
            throw new BadRequestException("At least one line item must be returned.");

        var userId = _currentUser.UserId ?? 1;
        var returnNumber = await _unitOfWork.InvoiceReturns.GenerateNextReturnNumberAsync();

        decimal totalRefund = 0;
        var returnEntity = new InvoiceReturn
        {
            OriginalInvoiceId = originalInvoice.Id,
            ReturnNumber = returnNumber,
            ReturnReason = request.Dto.ReturnReason,
            RefundMethod = request.Dto.RefundMethod,
            ProcessedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var returnLineDto in request.Dto.Lines)
        {
            var originalLine = originalInvoice.Lines.FirstOrDefault(l => l.Id == returnLineDto.InvoiceLineId);
            if (originalLine == null)
                throw new BadRequestException($"Invoice line ID {returnLineDto.InvoiceLineId} not found in original invoice.");

            if (returnLineDto.Quantity > originalLine.Quantity)
                throw new BadRequestException($"Returned quantity ({returnLineDto.Quantity}) cannot exceed sold quantity ({originalLine.Quantity}).");

            var unitPriceWithTax = originalLine.UnitPrice + (originalLine.TaxAmount / (originalLine.Quantity > 0 ? originalLine.Quantity : 1));
            var lineRefund = Math.Round(unitPriceWithTax * returnLineDto.Quantity, 2);
            totalRefund += lineRefund;

            returnEntity.Lines.Add(new InvoiceReturnLine
            {
                InvoiceLineId = originalLine.Id,
                ProductId = originalLine.ProductId,
                VariantId = originalLine.VariantId,
                Quantity = returnLineDto.Quantity,
                RefundAmount = lineRefund,
                Restock = returnLineDto.Restock
            });

            // Restock if specified
            if (returnLineDto.Restock)
            {
                var invId = originalLine.InventoryId ?? originalInvoice.InventoryId;
                BranchStock? stock = null;
                if (invId.HasValue)
                {
                    stock = await _unitOfWork.BranchStocks.GetInventoryStockAsync(invId.Value, originalLine.ProductId, originalLine.VariantId);
                }
                if (stock == null)
                {
                    stock = await _unitOfWork.BranchStocks.GetStockAsync(originalInvoice.BranchId, originalLine.ProductId, originalLine.VariantId);
                }

                int currentQty = stock?.QuantityOnHand ?? 0;
                int newQty = currentQty + returnLineDto.Quantity;

                if (stock != null)
                {
                    stock.QuantityOnHand = newQty;
                    await _unitOfWork.BranchStocks.AddOrUpdateStockAsync(stock);
                }
                else
                {
                    var inv = invId.HasValue ? await _unitOfWork.Inventories.GetByIdAsync(invId.Value) : (await _unitOfWork.Inventories.GetAllAsync(false, originalInvoice.BranchId)).FirstOrDefault();
                    stock = new BranchStock
                    {
                        InventoryId = inv?.Id ?? 1,
                        BranchId = inv?.BranchId ?? originalInvoice.BranchId,
                        ProductId = originalLine.ProductId,
                        VariantId = originalLine.VariantId,
                        QuantityOnHand = newQty,
                        MinAlertLevel = 2
                    };
                    await _unitOfWork.BranchStocks.AddOrUpdateStockAsync(stock);
                }

                var movement = new StockMovement
                {
                    InventoryId = stock.InventoryId,
                    BranchId = stock.BranchId ?? originalInvoice.BranchId,
                    ProductId = originalLine.ProductId,
                    VariantId = originalLine.VariantId,
                    MovementType = StockMovementType.Return,
                    QuantityChange = returnLineDto.Quantity,
                    BalanceAfter = newQty,
                    ReferenceNumber = returnNumber,
                    Notes = $"POS Return #{returnNumber} for Invoice #{originalInvoice.InvoiceNumber}",
                    CreatedByUserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.BranchStocks.AddStockMovementAsync(movement);
            }
        }

        returnEntity.RefundAmount = totalRefund;
        await _unitOfWork.InvoiceReturns.AddAsync(returnEntity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new InvoiceReturnDto
        {
            Id = returnEntity.Id,
            OriginalInvoiceId = originalInvoice.Id,
            OriginalInvoiceNumber = originalInvoice.InvoiceNumber,
            ReturnNumber = returnEntity.ReturnNumber,
            ReturnReason = returnEntity.ReturnReason,
            RefundMethod = returnEntity.RefundMethod,
            RefundAmount = returnEntity.RefundAmount,
            ProcessedByUserId = returnEntity.ProcessedByUserId,
            ProcessedByName = _currentUser.Username ?? "Cashier",
            CreatedAt = returnEntity.CreatedAt,
            Lines = returnEntity.Lines.Select(l => new InvoiceReturnLineDto
            {
                Id = l.Id,
                InvoiceLineId = l.InvoiceLineId,
                ProductId = l.ProductId,
                ProductName = originalInvoice.Lines.FirstOrDefault(ol => ol.Id == l.InvoiceLineId)?.ProductName ?? "Product",
                Quantity = l.Quantity,
                RefundAmount = l.RefundAmount,
                Restock = l.Restock
            }).ToList()
        };

        return APIBaseResponse<InvoiceReturnDto>.Success(dto, $"Return #{returnNumber} processed successfully");
    }
}
