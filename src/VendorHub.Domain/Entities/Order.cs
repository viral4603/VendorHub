using VendorHub.Domain.Enums;

namespace VendorHub.Domain.Entities;

public class Order
{
    public int Id { get; set; }

    public int CustomerUserId { get; set; }
    public User CustomerUser { get; set; } = null!;

    // An order always belongs to exactly one vendor — a checkout spanning several
    // vendors is split into one Order per vendor.
    public int VendorId { get; set; }
    public Vendor Vendor { get; set; } = null!;

    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; set; }

    // Null until the customer pays. One payment can cover several orders, because a
    // single checkout is split per vendor but paid for in one go.
    public int? PaymentId { get; set; }
    public Payment? Payment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
