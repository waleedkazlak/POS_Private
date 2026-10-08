using POS_Private.Application.Commands.Auth;
using POS_Private.Application.DTOs;
using POS_Private.Application.Queries.User;
using POS_Private.Application.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
    [ProducesResponseType(typeof(APIBaseResponse<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentUser()
    {
        if (_currentUser.UserId == null)
            return Unauthorized(APIBaseResponse<UserDto>.Failed(StatusCodes.Status401Unauthorized, "User is not authenticated."));

        var response = await _mediator.Send(new GetUserByIdQuery(_currentUser.UserId.Value));
        return Ok(response);
    }

    /// <summary>
    /// Update profile picture URL of the authenticated user
    /// </summary>
    [Authorize]
    [HttpPut("profile/picture")]
    [ProducesResponseType(typeof(APIBaseResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateProfilePicture([FromBody] UpdateProfilePictureDto dto)
    {
        if (_currentUser.UserId == null)
            return Unauthorized(APIBaseResponse<string>.Failed(StatusCodes.Status401Unauthorized, "User is not authenticated."));

        var response = await _mediator.Send(new UpdateProfilePictureCommand(_currentUser.UserId.Value, dto.ProfilePictureUrl));
        return Ok(response);
    }

    /// <summary>
    /// Upload and set a new profile picture for the authenticated user
    /// </summary>
    [Authorize]
    [HttpPost("profile/picture/upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(APIBaseResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UploadProfilePicture([FromForm] ImageUploadRequest request, [FromServices] IWebHostEnvironment environment)
    {
        if (_currentUser.UserId == null)
            return Unauthorized(APIBaseResponse<string>.Failed(StatusCodes.Status401Unauthorized, "User is not authenticated."));

        var file = request?.File;
        if (file == null || file.Length == 0)
            return BadRequest(APIBaseResponse<string>.Failed(StatusCodes.Status400BadRequest, "No image file provided."));

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(APIBaseResponse<string>.Failed(StatusCodes.Status400BadRequest, "Image file size exceeds the 5MB limit."));

        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) || !allowedExtensions.Contains(ext))
            return BadRequest(APIBaseResponse<string>.Failed(StatusCodes.Status400BadRequest, "Invalid image format. Supported formats: .jpg, .jpeg, .png, .webp, .gif"));

        var webRootPath = environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRootPath))
            webRootPath = Path.Combine(environment.ContentRootPath, "wwwroot");

        var avatarsFolder = Path.Combine(webRootPath, "uploads", "avatars");
        if (!Directory.Exists(avatarsFolder))
            Directory.CreateDirectory(avatarsFolder);

        var uniqueFileName = $"avatar_{_currentUser.UserId.Value}_{Guid.NewGuid():N}{ext}";
        var destinationPath = Path.Combine(avatarsFolder, uniqueFileName);

        using (var stream = new FileStream(destinationPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var httpRequest = HttpContext.Request;
        var relativeUrl = $"/uploads/avatars/{uniqueFileName}";
        var absoluteUrl = $"{httpRequest.Scheme}://{httpRequest.Host}{relativeUrl}";

        var response = await _mediator.Send(new UpdateProfilePictureCommand(_currentUser.UserId.Value, absoluteUrl));
        return Ok(response);
    }

    /// <summary>
    /// Change password of the authenticated user
    /// </summary>
    [Authorize]
    [HttpPut("change-password")]
    [ProducesResponseType(typeof(APIBaseResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        if (_currentUser.UserId == null)
            return Unauthorized(APIBaseResponse<bool>.Failed(StatusCodes.Status401Unauthorized, "User is not authenticated."));

        var response = await _mediator.Send(new ChangePasswordCommand(_currentUser.UserId.Value, dto));
        return Ok(response);
    }
}
