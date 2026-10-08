namespace POS_Private.Domain.Entities;

public class Customer : BaseEntity
{
    public string Code { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public int LoyaltyPoints { get; set; } = 0;
    public decimal TotalSpent { get; set; } = 0;
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
