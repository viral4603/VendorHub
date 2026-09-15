namespace VendorHub.Contracts.Catalog;

public class ProductCreateDto
{
    // Optional: a vendor who does not pick a category gets the default "Others"
    // category, which is created on first use.
    public int? CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? ImageUrl { get; set; }
}
