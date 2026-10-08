using POS_Private.Application.Commands.Inventory;
using POS_Private.Application.DTOs;
using POS_Private.Application.Queries.Inventory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace POS_Private.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public InventoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get paginated inventories / warehouses
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(APIBaseResponse<PaginatedResultDto<InventoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInventories([FromQuery] InventorySearchFilterDto filter)
    {
        var response = await _mediator.Send(new GetInventoriesQuery(filter));
        return Ok(response);
    }

    /// <summary>
    /// Get inventory by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(APIBaseResponse<InventoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInventoryById(int id)
    {
        var response = await _mediator.Send(new GetInventoryByIdQuery(id));
        return Ok(response);
    }

    /// <summary>
    /// Create new inventory / warehouse (Admin, InventoryManager)
    /// </summary>
    [Authorize(Roles = "Admin,InventoryManager")]
    [HttpPost]
    [ProducesResponseType(typeof(APIBaseResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateInventory([FromBody] CreateInventoryDto dto)
    {
        var response = await _mediator.Send(new CreateInventoryCommand(dto));
        return Ok(response);
    }

    /// <summary>
    /// Update inventory / warehouse (Admin, InventoryManager)
    /// </summary>
    [Authorize(Roles = "Admin,InventoryManager")]
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(APIBaseResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateInventory(int id, [FromBody] UpdateInventoryDto dto)
    {
        var response = await _mediator.Send(new UpdateInventoryCommand(id, dto));
        return Ok(response);
    }

    /// <summary>
    /// Delete inventory (Admin)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(APIBaseResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteInventory(int id)
    {
        var response = await _mediator.Send(new DeleteInventoryCommand(id));
        return Ok(response);
    }
}
