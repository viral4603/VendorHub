namespace VendorHub.Contracts.Vendor;

public class VendorResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string ShopName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
