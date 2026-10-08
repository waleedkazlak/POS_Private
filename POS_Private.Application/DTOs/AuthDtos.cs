namespace POS_Private.Application.DTOs;

public class JwtSettingsDto
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
    public int RefreshTokenExpirationDays { get; set; } = 30;
}

public class LoginRequestDto
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class LoginResponseDto
{
    public string Token { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;
}

public class RefreshTokenRequestDto
{
    public string Token { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}

public class RevokeTokenRequestDto
{
    public string RefreshToken { get; set; } = null!;
}

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public int RoleId { get; set; }
    public string RoleName { get; set; } = null!;
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public string PreferredLanguage { get; set; } = "en";
    public bool IsActive { get; set; }
}

public class UpdateProfilePictureDto
{
    public string ProfilePictureUrl { get; set; } = null!;
}

public class ChangePasswordDto
{
    public string CurrentPassword { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
    public string ConfirmNewPassword { get; set; } = null!;
}

public class CreateUserDto
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public int RoleId { get; set; }
    public int? BranchId { get; set; }
    public string PreferredLanguage { get; set; } = "en";
}

public class UpdateUserDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public int RoleId { get; set; }
    public int? BranchId { get; set; }
    public string PreferredLanguage { get; set; } = "en";
    public bool IsActive { get; set; }
}

public class RoleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public List<RolePermissionDto> Permissions { get; set; } = new();
}

public class RolePermissionDto
{
    public int ScreenId { get; set; }
    public string ScreenName { get; set; } = null!;
    public string ScreenCode { get; set; } = null!;
    public bool CanView { get; set; }
    public bool CanAdd { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}
