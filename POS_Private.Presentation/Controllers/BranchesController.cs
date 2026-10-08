using POS_Private.Application.Commands.Branch;
using POS_Private.Application.DTOs;
using POS_Private.Application.Queries.Branch;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace POS_Private.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BranchesController : ControllerBase
{
    private readonly IMediator _mediator;

    public BranchesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all branches and their POS registers
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(APIBaseResponse<IEnumerable<BranchDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBranches()
    {
        var response = await _mediator.Send(new GetBranchesQuery());
        return Ok(response);
    }

    /// <summary>
    /// Create new branch (Admin)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(APIBaseResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateBranch([FromBody] CreateBranchDto dto)
    {
        var response = await _mediator.Send(new CreateBranchCommand(dto));
        return Ok(response);
    }

    /// <summary>
    /// Get POS registers by branch
    /// </summary>
    [HttpGet("registers")]
    [ProducesResponseType(typeof(APIBaseResponse<IEnumerable<PosRegisterDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRegisters([FromQuery] int? branchId)
    {
        var response = await _mediator.Send(new GetPosRegistersQuery(branchId));
        return Ok(response);
    }

    /// <summary>
    /// Add POS register to a branch
    /// </summary>
    [Authorize(Roles = "Admin,StoreManager")]
    [HttpPost("registers")]
    [ProducesResponseType(typeof(APIBaseResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateRegister([FromBody] CreatePosRegisterDto dto)
    {
        var response = await _mediator.Send(new CreatePosRegisterCommand(dto));
        return Ok(response);
    }
}
