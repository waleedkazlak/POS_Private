using POS_Private.Application.Commands.Invoice;
using POS_Private.Application.DTOs;
using POS_Private.Application.Queries.Invoice;
using POS_Private.Application.Queries.Product;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace POS_Private.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PosController : ControllerBase
{
    private readonly IMediator _mediator;

    public PosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Fast product grid for cashier POS terminal
    /// </summary>
    [HttpGet("catalog")]
    [ProducesResponseType(typeof(APIBaseResponse<IEnumerable<PosProductDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCatalog([FromQuery] int? branchId, [FromQuery] int? categoryId, [FromQuery] string? searchTerm)
    {
        var response = await _mediator.Send(new GetPosCatalogQuery(branchId, categoryId, searchTerm));
        return Ok(response);
    }

    /// <summary>
    /// Instant barcode scan lookup
    /// </summary>
    [HttpGet("barcode/{barcode}")]
    [ProducesResponseType(typeof(APIBaseResponse<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ScanBarcode(string barcode)
    {
        var response = await _mediator.Send(new GetProductByBarcodeQuery(barcode));
        return Ok(response);
    }

    /// <summary>
    /// Issue Point of Sale sales invoice with 14% Egyptian VAT, loyalty, and payments
    /// </summary>
    [Authorize]
    [HttpPost("checkout")]
    [ProducesResponseType(typeof(APIBaseResponse<InvoiceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Checkout([FromBody] CreateInvoiceDto dto)
    {
        var response = await _mediator.Send(new CreateInvoiceCommand(dto));
        return Ok(response);
    }

    /// <summary>
    /// Get print-ready thermal receipt data for invoice
    /// </summary>
    [HttpGet("receipt/{invoiceId:int}")]
    [ProducesResponseType(typeof(APIBaseResponse<ReceiptDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReceipt(int invoiceId)
    {
        var response = await _mediator.Send(new GetReceiptQuery(invoiceId));
        return Ok(response);
    }
}
