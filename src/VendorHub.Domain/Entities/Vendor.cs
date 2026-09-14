using VendorHub.Domain.Enums;

namespace VendorHub.Domain.Entities;

public class Vendor
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string ShopName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public VendorStatus Status { get; set; } = VendorStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
