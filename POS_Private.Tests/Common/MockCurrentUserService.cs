using POS_Private.Application.Services;

namespace POS_Private.Tests.Common;

public class MockCurrentUserService : ICurrentUserService
{
    public int? UserId { get; set; } = 1;
    public string? Username { get; set; } = "admin";
    public string? Role { get; set; } = "Admin";
    public int? BranchId { get; set; } = 1;
}
