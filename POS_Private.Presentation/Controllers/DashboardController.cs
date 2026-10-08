using POS_Private.Application.DTOs;
using POS_Private.Application.Queries.Dashboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace POS_Private.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get executive POS dashboard metrics, sales breakdown, and performance charts
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(APIBaseResponse<DashboardMetricsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardMetrics([FromQuery] int? branchId)
    {
        var response = await _mediator.Send(new GetDashboardMetricsQuery(branchId));
        return Ok(response);
    }
}
