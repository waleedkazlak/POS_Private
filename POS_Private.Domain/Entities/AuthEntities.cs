namespace POS_Private.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    
    public int RoleId { get; set; }
    public Role? Role { get; set; }

    public int? BranchId { get; set; }
    public Branch? Branch { get; set; }

    public int? InventoryId { get; set; }
    public Inventory? Inventory { get; set; }

    public string PreferredLanguage { get; set; } = "en"; // "en" or "ar"

    public bool IsActive { get; set; } = true;

    public ICollection<CashierShift> Shifts { get; set; } = new List<CashierShift>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

public class Role : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public class Screen : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; } = 0;

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public class RolePermission : BaseEntity
{
    public int RoleId { get; set; }
    public Role? Role { get; set; }

    public int ScreenId { get; set; }
    public Screen? Screen { get; set; }

    public bool CanView { get; set; } = true;
    public bool CanAdd { get; set; } = true;
    public bool CanEdit { get; set; } = true;
    public bool CanDelete { get; set; } = true;
}

public class RefreshToken : BaseEntity
{
    public int UserId { get; set; }
    public User? User { get; set; }

    public string Token { get; set; } = null!;
    public string JwtId { get; set; } = null!;
    public bool IsUsed { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime ExpiryDate { get; set; }
}
