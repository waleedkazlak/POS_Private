using POS_Private.Application.Commands.User;
using POS_Private.Application.DTOs;
using POS_Private.Application.Queries.User;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace POS_Private.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,StoreManager")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all system users
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(APIBaseResponse<IEnumerable<UserDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers()
    {
        var response = await _mediator.Send(new GetUsersQuery());
        return Ok(response);
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(APIBaseResponse<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserById(int id)
    {
        var response = await _mediator.Send(new GetUserByIdQuery(id));
        return Ok(response);
    }

    /// <summary>
    /// Create new user account (Cashier, Manager, Admin)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(APIBaseResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        var response = await _mediator.Send(new CreateUserCommand(dto));
        return Ok(response);
    }

    /// <summary>
    /// Update user account
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(APIBaseResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
    {
        dto.Id = id;
        var response = await _mediator.Send(new UpdateUserCommand(dto));
        return Ok(response);
    }

    /// <summary>
    /// Get all security roles and screen permissions
    /// </summary>
    [HttpGet("roles")]
    [ProducesResponseType(typeof(APIBaseResponse<IEnumerable<RoleDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoles()
    {
        var response = await _mediator.Send(new GetRolesQuery());
        return Ok(response);
    }
}
