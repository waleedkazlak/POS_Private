using POS_Private.Application.Commands.Inventory;
using POS_Private.Application.DTOs;
using POS_Private.Application.Queries.Inventory;
using POS_Private.Domain.Interfaces;
using POS_Private.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace POS_Private.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IExcelExportService _excelExport;
    private readonly IUnitOfWork _unitOfWork;

    public InventoryController(IMediator mediator, IExcelExportService excelExport, IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _excelExport = excelExport;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Get paginated stock levels with optional filtering by inventory, branch, product, or low stock status
    /// </summary>
    [HttpGet("stocks")]
    [ProducesResponseType(typeof(APIBaseResponse<PaginatedResultDto<BranchStockDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllStocks([FromQuery] StockSearchFilterDto filter)
    {
        var response = await _mediator.Send(new GetBranchStocksQuery(filter));
        return Ok(response);
    }

    /// <summary>
    /// Get paginated stock levels for a specific branch
    /// </summary>
    [HttpGet("stocks/{branchId:int}")]
    [ProducesResponseType(typeof(APIBaseResponse<PaginatedResultDto<BranchStockDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBranchStocks(int branchId, [FromQuery] StockSearchFilterDto filter)
    {
        filter.BranchId = branchId;
        var response = await _mediator.Send(new GetBranchStocksQuery(filter));
        return Ok(response);
    }

    /// <summary>
    /// Check stock availability for a product (and optional variant) across ALL inventories
    /// </summary>
    [HttpGet("availability/{productId:int}")]
    [ProducesResponseType(typeof(APIBaseResponse<ProductStockAvailabilityDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductStockAvailability(int productId, [FromQuery] int? variantId)
    {
        var response = await _mediator.Send(new GetProductStockAvailabilityQuery(productId, variantId));
        return Ok(response);
    }

    /// <summary>
    /// Get paginated low stock alerts
    /// </summary>
    [HttpGet("low-stock-alerts")]
    [ProducesResponseType(typeof(APIBaseResponse<PaginatedResultDto<LowStockAlertDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLowStockAlerts([FromQuery] int? branchId, [FromQuery] int? inventoryId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 15)
    {
        var response = await _mediator.Send(new GetLowStockAlertsQuery(branchId, inventoryId, pageNumber, pageSize));
        return Ok(response);
    }

    /// <summary>
    /// Adjust inventory stock quantity (Damage, Write-off, Transfer, Correction)
    /// </summary>
    [Authorize(Roles = "Admin,StoreManager,InventoryManager")]
    [HttpPost("adjust")]
    [ProducesResponseType(typeof(APIBaseResponse<BranchStockDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AdjustStock([FromBody] AdjustStockDto dto)
    {
        var response = await _mediator.Send(new AdjustStockCommand(dto));
        return Ok(response);
    }

    /// <summary>
    /// Transfer stock between two inventories
    /// </summary>
    [Authorize(Roles = "Admin,StoreManager,InventoryManager")]
    [HttpPost("transfer")]
    [ProducesResponseType(typeof(APIBaseResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> TransferStock([FromBody] TransferStockDto dto)
    {
        var response = await _mediator.Send(new TransferStockCommand(dto));
        return Ok(response);
    }

    /// <summary>
    /// Get paginated stock audit trail / movement history
    /// </summary>
    [HttpGet("movements")]
    [ProducesResponseType(typeof(APIBaseResponse<PaginatedResultDto<StockMovementDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMovements([FromQuery] int? branchId, [FromQuery] int? inventoryId, [FromQuery] int? productId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var response = await _mediator.Send(new GetStockMovementsQuery(branchId, productId, fromDate, toDate, pageNumber, pageSize, inventoryId));
        return Ok(response);
    }

    /// <summary>
    /// Export stock sheet to Excel
    /// </summary>
    [HttpGet("stocks/{branchId:int}/export")]
    public async Task<IActionResult> ExportStockToExcel(int branchId, [FromQuery] int? inventoryId)
    {
        var stocks = await _unitOfWork.BranchStocks.GetAllStocksAsync(inventoryId, branchId);
        var dtos = stocks.Select(s => new BranchStockDto
        {
            Id = s.Id,
            InventoryId = s.InventoryId,
            InventoryName = s.Inventory?.NameEn ?? "N/A",
            InventoryCode = s.Inventory?.Code,
            BranchId = s.BranchId,
            BranchName = s.Branch?.NameEn ?? s.Inventory?.Branch?.NameEn,
            ProductId = s.ProductId,
            ProductName = s.Product?.NameEn ?? "N/A",
            SKU = s.Variant?.SKU ?? s.Product?.SKU,
            Barcode = s.Variant?.Barcode ?? s.Product?.Barcode,
            VariantId = s.VariantId,
            VariantName = GetBranchStocksQueryHandler.FormatVariantName(s.Variant),
            Price = s.Variant?.EffectivePrice ?? (s.Product?.BasePrice ?? 0),
            QuantityOnHand = s.QuantityOnHand,
            MinAlertLevel = s.MinAlertLevel
        }).ToList();

        var bytes = _excelExport.ExportInventoryToExcel(dtos);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"POS_Private_Stock_{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }
}
