namespace VendorHub.Contracts.Order;

public class OrderResponseDto
{
    public int Id { get; set; }
    public int CustomerUserId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int VendorId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemResponseDto> Items { get; set; } = new();
}
