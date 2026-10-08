using POS_Private.Application.Commands.Auth;
using POS_Private.Application.DTOs;
using POS_Private.Application.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace POS_Private.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public AuthController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Authenticate user and obtain JWT token
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(APIBaseResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var response = await _mediator.Send(new LoginCommand(dto.Username, dto.Password));
        return Ok(response);
    }

    /// <summary>
    /// Refresh an expired JWT token using refresh token
    /// </summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(APIBaseResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto)
    {
        var response = await _mediator.Send(new RefreshTokenCommand(dto.Token, dto.RefreshToken));
        return Ok(response);
    }

    /// <summary>
    /// Revoke a refresh token (Logout)
    /// </summary>
    [HttpPost("revoke-token")]
    [ProducesResponseType(typeof(APIBaseResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequestDto dto)
    {
        var response = await _mediator.Send(new RevokeTokenCommand(dto.RefreshToken));
        return Ok(response);
    }

    /// <summary>
    /// Get current authenticated user profile
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(APIBaseResponse<object>), StatusCodes.Status200OK)]
    public IActionResult GetCurrentUser()
    {
        var profile = new
        {
            UserId = _currentUser.UserId,
            Username = _currentUser.Username,
            Role = _currentUser.Role,
            BranchId = _currentUser.BranchId
        };

        return Ok(APIBaseResponse<object>.Success(profile));
    }
}
