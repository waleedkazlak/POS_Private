using POS_Private.Application.Commands.Category;
using POS_Private.Application.DTOs;
using POS_Private.Application.Queries.Category;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace POS_Private.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all product categories with child categories and product counts
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(APIBaseResponse<IEnumerable<CategoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories()
    {
        var response = await _mediator.Send(new GetCategoriesQuery());
        return Ok(response);
    }

    /// <summary>
    /// Get category by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(APIBaseResponse<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategoryById(int id)
    {
        var response = await _mediator.Send(new GetCategoryByIdQuery(id));
        return Ok(response);
    }

    /// <summary>
    /// Create new category
    /// </summary>
    [Authorize(Roles = "Admin,StoreManager,InventoryManager")]
    [HttpPost]
    [ProducesResponseType(typeof(APIBaseResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        var response = await _mediator.Send(new CreateCategoryCommand(dto));
        return Ok(response);
    }

    /// <summary>
    /// Update category
    /// </summary>
    [Authorize(Roles = "Admin,StoreManager,InventoryManager")]
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(APIBaseResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto dto)
    {
        dto.Id = id;
        var response = await _mediator.Send(new UpdateCategoryCommand(dto));
        return Ok(response);
    }

    /// <summary>
    /// Delete category
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(APIBaseResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var response = await _mediator.Send(new DeleteCategoryCommand(id));
        return Ok(response);
    }
}
