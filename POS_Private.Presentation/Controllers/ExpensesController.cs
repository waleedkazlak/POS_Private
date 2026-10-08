using POS_Private.Application.Commands.Expense;
using POS_Private.Application.DTOs;
using POS_Private.Application.Queries.Expense;
using POS_Private.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace POS_Private.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExpensesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExpensesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Search and list operational expenses with filters and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(APIBaseResponse<PaginatedResultDto<ExpenseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpenses(
        [FromQuery] string? searchTerm,
        [FromQuery] int? categoryId,
        [FromQuery] int? branchId,
        [FromQuery] int? shiftId,
        [FromQuery] ExpensePaymentMethod? paymentMethod,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var response = await _mediator.Send(new GetExpensesQuery(
            searchTerm, categoryId, branchId, shiftId, paymentMethod, fromDate, toDate, pageNumber, pageSize));
        return Ok(response);
    }

    /// <summary>
    /// Get expense details by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(APIBaseResponse<ExpenseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpenseById(int id)
    {
        var response = await _mediator.Send(new GetExpenseByIdQuery(id));
        return Ok(response);
    }

    /// <summary>
    /// Record a new expense (optionally deducted from Cashier shift drawer)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(APIBaseResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateExpense([FromBody] CreateExpenseDto dto)
    {
        var response = await _mediator.Send(new CreateExpenseCommand(dto));
        return Ok(response);
    }

    /// <summary>
    /// Delete expense record
    /// </summary>
    [Authorize(Roles = "Admin,StoreManager")]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(APIBaseResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteExpense(int id)
    {
        var response = await _mediator.Send(new DeleteExpenseCommand(id));
        return Ok(response);
    }

    /// <summary>
    /// List all expense categories
    /// </summary>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(APIBaseResponse<IEnumerable<ExpenseCategoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpenseCategories([FromQuery] bool? isActive)
    {
        var response = await _mediator.Send(new GetExpenseCategoriesQuery(isActive));
        return Ok(response);
    }

    /// <summary>
    /// Create new expense category
    /// </summary>
    [Authorize(Roles = "Admin,StoreManager")]
    [HttpPost("categories")]
    [ProducesResponseType(typeof(APIBaseResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateExpenseCategory([FromBody] CreateExpenseCategoryDto dto)
    {
        var response = await _mediator.Send(new CreateExpenseCategoryCommand(dto));
        return Ok(response);
    }

    /// <summary>
    /// Get expense analytics breakdown and financial metrics
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(APIBaseResponse<ExpenseMetricsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpenseMetrics(
        [FromQuery] int? branchId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        var response = await _mediator.Send(new GetExpenseMetricsQuery(branchId, fromDate, toDate));
        return Ok(response);
    }
}
