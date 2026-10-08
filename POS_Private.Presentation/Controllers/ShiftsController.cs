using POS_Private.Application.Commands.Shift;
using POS_Private.Application.DTOs;
using POS_Private.Application.Queries.Shift;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace POS_Private.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ShiftsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ShiftsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get current active shift for logged-in cashier
    /// </summary>
    [HttpGet("current")]
    [ProducesResponseType(typeof(APIBaseResponse<CashierShiftDto?>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentShift()
    {
        var response = await _mediator.Send(new GetCurrentShiftQuery());
        return Ok(response);
    }

    /// <summary>
    /// Open a new cashier shift with opening cash drawer float
    /// </summary>
    [HttpPost("open")]
    [ProducesResponseType(typeof(APIBaseResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> OpenShift([FromBody] OpenShiftDto dto)
    {
        var response = await _mediator.Send(new OpenShiftCommand(dto));
        return Ok(response);
    }

    /// <summary>
    /// Close cashier shift, perform cash drawer count reconciliation, and generate Z-Report
    /// </summary>
    [HttpPost("close")]
    [ProducesResponseType(typeof(APIBaseResponse<ShiftSummaryReportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CloseShift([FromBody] CloseShiftDto dto)
    {
        var response = await _mediator.Send(new CloseShiftCommand(dto));
        return Ok(response);
    }

    /// <summary>
    /// Record cash drawer transaction (Cash In, Cash Out, Cash Drop)
    /// </summary>
    [HttpPost("transactions")]
    [ProducesResponseType(typeof(APIBaseResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddTransaction([FromBody] AddShiftTransactionDto dto)
    {
        var response = await _mediator.Send(new AddShiftTransactionCommand(dto));
        return Ok(response);
    }

    /// <summary>
    /// Query shift history
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(APIBaseResponse<IEnumerable<CashierShiftDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShifts([FromQuery] int? branchId, [FromQuery] int? registerId, [FromQuery] int? cashierUserId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var response = await _mediator.Send(new GetShiftsQuery(branchId, registerId, cashierUserId, fromDate, toDate));
        return Ok(response);
    }

    /// <summary>
    /// Get detailed X-Report / Z-Report for a specific shift
    /// </summary>
    [HttpGet("{shiftId:int}/report")]
    [ProducesResponseType(typeof(APIBaseResponse<ShiftSummaryReportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShiftReport(int shiftId)
    {
        var response = await _mediator.Send(new GetShiftReportQuery(shiftId));
        return Ok(response);
    }
}
