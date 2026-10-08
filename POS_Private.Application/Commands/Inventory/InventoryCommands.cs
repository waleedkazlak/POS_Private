using POS_Private.Application.Common;
using POS_Private.Application.DTOs;
using POS_Private.Application.Exceptions;
using POS_Private.Application.Services;
using POS_Private.Domain.Entities;
using POS_Private.Domain.Enums;
using POS_Private.Domain.Interfaces;
using MediatR;

namespace POS_Private.Application.Commands.Inventory;

public record CreateInventoryCommand(CreateInventoryDto Dto) : IRequest<APIBaseResponse<int>>;

public class CreateInventoryCommandHandler : IRequestHandler<CreateInventoryCommand, APIBaseResponse<int>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateInventoryCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<int>> Handle(CreateInventoryCommand request, CancellationToken cancellationToken)
    {
        var existing = await _unitOfWork.Inventories.GetByCodeAsync(request.Dto.Code);
        if (existing != null)
            throw new BadRequestException("An inventory with this code already exists");

        var inventory = new POS_Private.Domain.Entities.Inventory
        {
            Code = request.Dto.Code,
            NameEn = request.Dto.NameEn,
            NameAr = request.Dto.NameAr,
            DescriptionEn = request.Dto.DescriptionEn,
            DescriptionAr = request.Dto.DescriptionAr,
            Address = request.Dto.Address,
            BranchId = request.Dto.BranchId,
            IsActive = request.Dto.IsActive,
            IsDefault = request.Dto.IsDefault
        };

        await _unitOfWork.Inventories.AddAsync(inventory);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return APIBaseResponse<int>.Success(inventory.Id, "Inventory created successfully");
    }
}

public record UpdateInventoryCommand(int Id, UpdateInventoryDto Dto) : IRequest<APIBaseResponse<bool>>;

public class UpdateInventoryCommandHandler : IRequestHandler<UpdateInventoryCommand, APIBaseResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateInventoryCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<bool>> Handle(UpdateInventoryCommand request, CancellationToken cancellationToken)
    {
        var inventory = await _unitOfWork.Inventories.GetByIdAsync(request.Id);
        if (inventory == null)
            throw new NotFoundException(nameof(POS_Private.Domain.Entities.Inventory), request.Id);

        inventory.Code = request.Dto.Code;
        inventory.NameEn = request.Dto.NameEn;
        inventory.NameAr = request.Dto.NameAr;
        inventory.DescriptionEn = request.Dto.DescriptionEn;
        inventory.DescriptionAr = request.Dto.DescriptionAr;
        inventory.Address = request.Dto.Address;
        inventory.BranchId = request.Dto.BranchId;
        inventory.IsActive = request.Dto.IsActive;
        inventory.IsDefault = request.Dto.IsDefault;

        await _unitOfWork.Inventories.UpdateAsync(inventory);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return APIBaseResponse<bool>.Success(true, "Inventory updated successfully");
    }
}

public record DeleteInventoryCommand(int Id) : IRequest<APIBaseResponse<bool>>;

public class DeleteInventoryCommandHandler : IRequestHandler<DeleteInventoryCommand, APIBaseResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteInventoryCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<bool>> Handle(DeleteInventoryCommand request, CancellationToken cancellationToken)
    {
        var inventory = await _unitOfWork.Inventories.GetByIdAsync(request.Id);
        if (inventory == null)
            throw new NotFoundException(nameof(POS_Private.Domain.Entities.Inventory), request.Id);

        await _unitOfWork.Inventories.DeleteAsync(request.Id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return APIBaseResponse<bool>.Success(true, "Inventory deleted successfully");
    }
}

public record AdjustStockCommand(AdjustStockDto Dto) : IRequest<APIBaseResponse<BranchStockDto>>;

public class AdjustStockCommandHandler : IRequestHandler<AdjustStockCommand, APIBaseResponse<BranchStockDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public AdjustStockCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<APIBaseResponse<BranchStockDto>> Handle(AdjustStockCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.Dto.ProductId);
        if (product == null)
            throw new NotFoundException(nameof(Product), request.Dto.ProductId);

        // Determine inventory
        POS_Private.Domain.Entities.Inventory? inventory = null;
        if (request.Dto.InventoryId.HasValue)
        {
            inventory = await _unitOfWork.Inventories.GetByIdAsync(request.Dto.InventoryId.Value);
        }
        else if (request.Dto.BranchId.HasValue)
        {
            var inventories = await _unitOfWork.Inventories.GetAllAsync(false, request.Dto.BranchId.Value);
            inventory = inventories.FirstOrDefault(i => i.IsDefault) ?? inventories.FirstOrDefault();
        }

        if (inventory == null)
        {
            // fallback: get first active inventory
            var allInv = await _unitOfWork.Inventories.GetAllAsync(false);
            inventory = allInv.FirstOrDefault();
            if (inventory == null)
                throw new NotFoundException(nameof(POS_Private.Domain.Entities.Inventory), request.Dto.InventoryId ?? 0);
        }

        var stock = await _unitOfWork.BranchStocks.GetInventoryStockAsync(inventory.Id, request.Dto.ProductId, request.Dto.VariantId);
        int currentQty = stock?.QuantityOnHand ?? 0;
        int newQty = currentQty + request.Dto.QuantityChange;
        if (newQty < 0) newQty = 0;

        if (stock == null)
        {
            stock = new BranchStock
            {
                InventoryId = inventory.Id,
                BranchId = inventory.BranchId ?? request.Dto.BranchId,
                ProductId = request.Dto.ProductId,
                VariantId = request.Dto.VariantId,
                QuantityOnHand = newQty,
                MinAlertLevel = 2
            };
        }
        else
        {
            stock.QuantityOnHand = newQty;
        }

        await _unitOfWork.BranchStocks.AddOrUpdateStockAsync(stock);

        var userId = _currentUser.UserId ?? 1;
        var movement = new StockMovement
        {
            InventoryId = inventory.Id,
            BranchId = inventory.BranchId ?? request.Dto.BranchId,
            ProductId = request.Dto.ProductId,
            VariantId = request.Dto.VariantId,
            MovementType = request.Dto.MovementType,
            QuantityChange = request.Dto.QuantityChange,
            BalanceAfter = newQty,
            ReferenceNumber = request.Dto.ReferenceNumber ?? "MANUAL-ADJUST",
            Notes = request.Dto.Notes ?? "Manual Inventory Adjustment",
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.BranchStocks.AddStockMovementAsync(movement);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new BranchStockDto
        {
            Id = stock.Id,
            InventoryId = inventory.Id,
            InventoryName = LocalizationHelper.Localize(inventory.NameEn, inventory.NameAr),
            InventoryCode = inventory.Code,
            BranchId = stock.BranchId,
            BranchName = inventory.Branch != null ? LocalizationHelper.Localize(inventory.Branch.NameEn, inventory.Branch.NameAr) : null,
            ProductId = stock.ProductId,
            ProductName = LocalizationHelper.Localize(product.NameEn, product.NameAr),
            SKU = product.SKU,
            Barcode = product.Barcode,
            VariantId = stock.VariantId,
            Price = product.BasePrice,
            QuantityOnHand = stock.QuantityOnHand,
            MinAlertLevel = stock.MinAlertLevel
        };

        return APIBaseResponse<BranchStockDto>.Success(dto, "Stock adjusted successfully");
    }
}

public record TransferStockCommand(TransferStockDto Dto) : IRequest<APIBaseResponse<bool>>;

public class TransferStockCommandHandler : IRequestHandler<TransferStockCommand, APIBaseResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public TransferStockCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<APIBaseResponse<bool>> Handle(TransferStockCommand request, CancellationToken cancellationToken)
    {
        if (request.Dto.Quantity <= 0)
            throw new BadRequestException("Transfer quantity must be greater than zero");

        if (request.Dto.FromInventoryId == request.Dto.ToInventoryId)
            throw new BadRequestException("Source and destination inventories cannot be the same");

        var fromInventory = await _unitOfWork.Inventories.GetByIdAsync(request.Dto.FromInventoryId);
        if (fromInventory == null)
            throw new NotFoundException(nameof(POS_Private.Domain.Entities.Inventory), request.Dto.FromInventoryId);

        var toInventory = await _unitOfWork.Inventories.GetByIdAsync(request.Dto.ToInventoryId);
        if (toInventory == null)
            throw new NotFoundException(nameof(POS_Private.Domain.Entities.Inventory), request.Dto.ToInventoryId);

        var product = await _unitOfWork.Products.GetByIdAsync(request.Dto.ProductId);
        if (product == null)
            throw new NotFoundException(nameof(Product), request.Dto.ProductId);

        var fromStock = await _unitOfWork.BranchStocks.GetInventoryStockAsync(fromInventory.Id, request.Dto.ProductId, request.Dto.VariantId);
        int fromCurrentQty = fromStock?.QuantityOnHand ?? 0;
        if (fromCurrentQty < request.Dto.Quantity)
            throw new BadRequestException($"Insufficient stock in {fromInventory.NameEn}. Available: {fromCurrentQty}");

        // Deduct from source
        int fromNewQty = fromCurrentQty - request.Dto.Quantity;
        fromStock!.QuantityOnHand = fromNewQty;
        await _unitOfWork.BranchStocks.AddOrUpdateStockAsync(fromStock);

        // Add to destination
        var toStock = await _unitOfWork.BranchStocks.GetInventoryStockAsync(toInventory.Id, request.Dto.ProductId, request.Dto.VariantId);
        int toCurrentQty = toStock?.QuantityOnHand ?? 0;
        int toNewQty = toCurrentQty + request.Dto.Quantity;

        if (toStock == null)
        {
            toStock = new BranchStock
            {
                InventoryId = toInventory.Id,
                BranchId = toInventory.BranchId,
                ProductId = request.Dto.ProductId,
                VariantId = request.Dto.VariantId,
                QuantityOnHand = toNewQty,
                MinAlertLevel = fromStock.MinAlertLevel
            };
        }
        else
        {
            toStock.QuantityOnHand = toNewQty;
        }
        await _unitOfWork.BranchStocks.AddOrUpdateStockAsync(toStock);

        var userId = _currentUser.UserId ?? 1;
        var transferRef = request.Dto.ReferenceNumber ?? $"TRF-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

        // Record movements for both sides
        var outMovement = new StockMovement
        {
            InventoryId = fromInventory.Id,
            ToInventoryId = toInventory.Id,
            BranchId = fromInventory.BranchId,
            ProductId = request.Dto.ProductId,
            VariantId = request.Dto.VariantId,
            MovementType = StockMovementType.TransferOut,
            QuantityChange = -request.Dto.Quantity,
            BalanceAfter = fromNewQty,
            ReferenceNumber = transferRef,
            Notes = request.Dto.Notes ?? $"Transfer out to {toInventory.NameEn}",
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        var inMovement = new StockMovement
        {
            InventoryId = toInventory.Id,
            ToInventoryId = fromInventory.Id,
            BranchId = toInventory.BranchId,
            ProductId = request.Dto.ProductId,
            VariantId = request.Dto.VariantId,
            MovementType = StockMovementType.TransferIn,
            QuantityChange = request.Dto.Quantity,
            BalanceAfter = toNewQty,
            ReferenceNumber = transferRef,
            Notes = request.Dto.Notes ?? $"Transfer in from {fromInventory.NameEn}",
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.BranchStocks.AddStockMovementAsync(outMovement);
        await _unitOfWork.BranchStocks.AddStockMovementAsync(inMovement);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return APIBaseResponse<bool>.Success(true, "Stock transferred successfully");
    }
}
