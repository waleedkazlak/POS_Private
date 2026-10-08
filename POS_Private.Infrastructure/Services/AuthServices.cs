using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using POS_Private.Application.DTOs;
using POS_Private.Application.Exceptions;
using POS_Private.Application.Services;
using POS_Private.Domain.Entities;
using POS_Private.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace POS_Private.Infrastructure.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettingsDto _jwtSettings;

    public AuthenticationService(IUnitOfWork unitOfWork, JwtSettingsDto jwtSettings)
    {
        _unitOfWork = unitOfWork;
        _jwtSettings = jwtSettings;
    }

    public async Task<LoginResponseDto> AuthenticateAsync(string username, string password)
    {
        var user = await _unitOfWork.Users.GetByUsernameAsync(username.Trim());
        if (user == null || !user.IsActive)
            throw new UnauthorizedException("Invalid username or password.");

        bool isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        if (!isValid)
            throw new UnauthorizedException("Invalid username or password.");

        var (jwtToken, jwtId, expiresAt) = GenerateJwtToken(user);
        var refreshTokenString = GenerateRefreshTokenString();

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenString,
            JwtId = jwtId,
            IsUsed = false,
            IsRevoked = false,
            ExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
        };

        await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
        await _unitOfWork.SaveChangesAsync();

        return new LoginResponseDto
        {
            Token = jwtToken,
            RefreshToken = refreshTokenString,
            ExpiresAt = expiresAt,
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                RoleId = user.RoleId,
                RoleName = user.Role?.Name ?? "User",
                BranchId = user.BranchId,
                BranchName = POS_Private.Application.Common.LocalizationHelper.LocalizeNullable(user.Branch?.NameEn, user.Branch?.NameAr),
                PreferredLanguage = user.PreferredLanguage,
                IsActive = user.IsActive
            }
        };
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(string token, string refreshToken)
    {
        var storedToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken);
        if (storedToken == null || storedToken.IsRevoked || storedToken.IsUsed || storedToken.ExpiryDate < DateTime.UtcNow)
            throw new UnauthorizedException("Invalid or expired refresh token.");

        var user = await _unitOfWork.Users.GetByIdAsync(storedToken.UserId);
        if (user == null || !user.IsActive)
            throw new UnauthorizedException("User not found or inactive.");

        storedToken.IsUsed = true;
        await _unitOfWork.RefreshTokens.UpdateAsync(storedToken);

        var (newJwtToken, newJwtId, expiresAt) = GenerateJwtToken(user);
        var newRefreshTokenString = GenerateRefreshTokenString();

        var newRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshTokenString,
            JwtId = newJwtId,
            IsUsed = false,
            IsRevoked = false,
            ExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
        };

        await _unitOfWork.RefreshTokens.AddAsync(newRefreshToken);
        await _unitOfWork.SaveChangesAsync();

        return new LoginResponseDto
        {
            Token = newJwtToken,
            RefreshToken = newRefreshTokenString,
            ExpiresAt = expiresAt,
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                RoleId = user.RoleId,
                RoleName = user.Role?.Name ?? "User",
                BranchId = user.BranchId,
                BranchName = POS_Private.Application.Common.LocalizationHelper.LocalizeNullable(user.Branch?.NameEn, user.Branch?.NameAr),
                PreferredLanguage = user.PreferredLanguage,
                IsActive = user.IsActive
            }
        };
    }

    public async Task RevokeTokenAsync(string refreshToken)
    {
        var storedToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken);
        if (storedToken != null)
        {
            storedToken.IsRevoked = true;
            await _unitOfWork.RefreshTokens.UpdateAsync(storedToken);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    private (string token, string jwtId, DateTime expiresAt) GenerateJwtToken(User user)
    {
        var jwtId = Guid.NewGuid().ToString();
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, jwtId),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role?.Name ?? "Cashier"),
            new("fullName", user.FullName),
            new("preferredLanguage", user.PreferredLanguage)
        };

        if (user.BranchId.HasValue)
        {
            claims.Add(new Claim("branchId", user.BranchId.Value.ToString()));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), jwtId, expiresAt);
    }

    private static string GenerateRefreshTokenString()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? UserId
    {
        get
        {
            var idClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? _httpContextAccessor.HttpContext?.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return int.TryParse(idClaim, out var id) ? id : null;
        }
    }

    public string? Username => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;
    public string? Role => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;
    public int? BranchId
    {
        get
        {
            var branchClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("branchId")?.Value;
            return int.TryParse(branchClaim, out var id) ? id : null;
        }
    }
}
