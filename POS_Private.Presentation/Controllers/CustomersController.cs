using POS_Private.Application.Commands.Customer;
using POS_Private.Application.DTOs;
using POS_Private.Application.Queries.Customer;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace POS_Private.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Search customers by phone or name
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(APIBaseResponse<PaginatedResultDto<CustomerDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchCustomers([FromQuery] CustomerSearchFilterDto filter)
    {
        var response = await _mediator.Send(new SearchCustomersQuery(filter));
        return Ok(response);
    }

    /// <summary>
    /// Get customer details with purchase and loyalty history
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(APIBaseResponse<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerById(int id)
    {
        var response = await _mediator.Send(new GetCustomerByIdQuery(id));
        return Ok(response);
    }

    /// <summary>
    /// Register new customer
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(APIBaseResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerDto dto)
    {
        var response = await _mediator.Send(new CreateCustomerCommand(dto));
        return Ok(response);
    }

    /// <summary>
    /// Update customer details
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(APIBaseResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerDto dto)
    {
        dto.Id = id;
        var response = await _mediator.Send(new UpdateCustomerCommand(dto));
        return Ok(response);
    }
}
