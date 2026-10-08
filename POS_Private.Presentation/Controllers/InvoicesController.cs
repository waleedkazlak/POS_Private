using POS_Private.Application.Commands.Invoice;
using POS_Private.Application.DTOs;
using POS_Private.Application.Queries.Invoice;
using POS_Private.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace POS_Private.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IExcelExportService _excelExport;

    public InvoicesController(IMediator mediator, IExcelExportService excelExport)
    {
        _mediator = mediator;
        _excelExport = excelExport;
    }

    /// <summary>
    /// Search and filter sales invoices
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(APIBaseResponse<PaginatedResultDto<InvoiceDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchInvoices([FromQuery] InvoiceSearchFilterDto filter)
    {
        var response = await _mediator.Send(new SearchInvoicesQuery(filter));
        return Ok(response);
    }

    /// <summary>
    /// Get invoice details by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(APIBaseResponse<InvoiceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInvoiceById(int id)
    {
        var response = await _mediator.Send(new GetInvoiceByIdQuery(id));
        return Ok(response);
    }

    /// <summary>
    /// Get invoice details by invoice number (e.g. INV-202609-00001)
    /// </summary>
    [HttpGet("by-number/{invoiceNumber}")]
    [ProducesResponseType(typeof(APIBaseResponse<InvoiceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInvoiceByNumber(string invoiceNumber)
    {
        var response = await _mediator.Send(new GetInvoiceByNumberQuery(invoiceNumber));
        return Ok(response);
    }

    /// <summary>
    /// Process sales return / credit note and restore inventory
    /// </summary>
    [Authorize]
    [HttpPost("returns")]
    [ProducesResponseType(typeof(APIBaseResponse<InvoiceReturnDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ProcessReturn([FromBody] CreateReturnDto dto)
    {
        var response = await _mediator.Send(new ProcessReturnCommand(dto));
        return Ok(response);
    }

    /// <summary>
    /// Export invoices to Excel file
    /// </summary>
    [HttpGet("export/excel")]
    public async Task<IActionResult> ExportToExcel([FromQuery] InvoiceSearchFilterDto filter)
    {
        filter.PageNumber = 1;
        filter.PageSize = 10000;
        var response = await _mediator.Send(new SearchInvoicesQuery(filter));
        var items = response.Data?.Items ?? new List<InvoiceDto>();

        var bytes = _excelExport.ExportInvoicesToExcel(items);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"POS_Private_Invoices_{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }
}
