using POS_Private.Application.DTOs;
using POS_Private.Application.Exceptions;
using POS_Private.Application.Services;
using POS_Private.Domain.Interfaces;
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

public record UpdateProfilePictureCommand(int UserId, string ProfilePictureUrl) : IRequest<APIBaseResponse<string>>;

public class UpdateProfilePictureCommandHandler : IRequestHandler<UpdateProfilePictureCommand, APIBaseResponse<string>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProfilePictureCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<string>> Handle(UpdateProfilePictureCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
        if (user == null)
            throw new NotFoundException(nameof(Domain.Entities.User), request.UserId);

        user.ProfilePictureUrl = request.ProfilePictureUrl?.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return APIBaseResponse<string>.Success(user.ProfilePictureUrl ?? string.Empty, "Profile picture updated successfully");
    }
}

public record ChangePasswordCommand(int UserId, ChangePasswordDto Dto) : IRequest<APIBaseResponse<bool>>;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, APIBaseResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public ChangePasswordCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<bool>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
        if (user == null)
            throw new NotFoundException(nameof(Domain.Entities.User), request.UserId);

        if (string.IsNullOrWhiteSpace(request.Dto.CurrentPassword))
            throw new BadRequestException("Current password is required.");

        bool isCurrentValid = BCrypt.Net.BCrypt.Verify(request.Dto.CurrentPassword, user.PasswordHash);
        if (!isCurrentValid)
            throw new BadRequestException("The current password provided is incorrect.");

        if (string.IsNullOrWhiteSpace(request.Dto.NewPassword) || request.Dto.NewPassword.Length < 6)
            throw new BadRequestException("New password must be at least 6 characters long.");

        if (request.Dto.NewPassword != request.Dto.ConfirmNewPassword)
            throw new BadRequestException("New password and confirmation password do not match.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return APIBaseResponse<bool>.Success(true, "Password changed successfully");
    }
}
