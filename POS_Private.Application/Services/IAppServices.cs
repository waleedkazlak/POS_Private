using POS_Private.Application.DTOs;

namespace POS_Private.Application.Services;

public interface IAuthenticationService
{
    Task<LoginResponseDto> AuthenticateAsync(string username, string password);
    Task<LoginResponseDto> RefreshTokenAsync(string token, string refreshToken);
    Task RevokeTokenAsync(string refreshToken);
}

public interface ICurrentUserService
{
    int? UserId { get; }
    string? Username { get; }
    string? Role { get; }
    int? BranchId { get; }
}
