namespace VendorHub.Contracts.Catalog;

public class ProductUpdateDto
{
    // Optional: omitting it leaves the product in the category it is already in,
    // rather than moving it to the default.
    public int? CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
}
