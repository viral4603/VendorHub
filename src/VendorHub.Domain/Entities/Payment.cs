using VendorHub.Domain.Enums;

namespace VendorHub.Domain.Entities;

public class Payment
{
    public int Id { get; set; }

    public int CustomerUserId { get; set; }
    public User CustomerUser { get; set; } = null!;

    // Total across every order this payment covers — a checkout split across several
    // vendors produces several orders but is paid for once.
    public decimal Amount { get; set; }

    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    // Simulated reference — there is no gateway integration behind this.
    public string TransactionReference { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
