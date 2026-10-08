using POS_Private.Application.DTOs;
using POS_Private.Application.Services;
using MediatR;

namespace POS_Private.Application.Commands.Auth;

public record LoginCommand(string Username, string Password) : IRequest<APIBaseResponse<LoginResponseDto>>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, APIBaseResponse<LoginResponseDto>>
{
    private readonly IAuthenticationService _authService;

    public LoginCommandHandler(IAuthenticationService authService)
    {
        _authService = authService;
    }

    public async Task<APIBaseResponse<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var response = await _authService.AuthenticateAsync(request.Username, request.Password);
        return APIBaseResponse<LoginResponseDto>.Success(response, "Login successful");
    }
}

public record RefreshTokenCommand(string Token, string RefreshToken) : IRequest<APIBaseResponse<LoginResponseDto>>;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, APIBaseResponse<LoginResponseDto>>
{
    private readonly IAuthenticationService _authService;

    public RefreshTokenCommandHandler(IAuthenticationService authService)
    {
        _authService = authService;
    }

    public async Task<APIBaseResponse<LoginResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var response = await _authService.RefreshTokenAsync(request.Token, request.RefreshToken);
        return APIBaseResponse<LoginResponseDto>.Success(response, "Token refreshed successfully");
    }
}

public record RevokeTokenCommand(string RefreshToken) : IRequest<APIBaseResponse<bool>>;

public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, APIBaseResponse<bool>>
{
    private readonly IAuthenticationService _authService;

    public RevokeTokenCommandHandler(IAuthenticationService authService)
    {
        _authService = authService;
    }

    public async Task<APIBaseResponse<bool>> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        await _authService.RevokeTokenAsync(request.RefreshToken);
        return APIBaseResponse<bool>.Success(true, "Token revoked successfully");
    }
}
