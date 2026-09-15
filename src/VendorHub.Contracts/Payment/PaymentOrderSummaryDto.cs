namespace VendorHub.Contracts.Payment;

/// <summary>
/// A lean view of an order covered by a payment. Payment views care about which
/// vendor was paid and how much, not about the individual lines, so this avoids
/// loading every OrderItem for a payment history listing.
/// </summary>
public class PaymentOrderSummaryDto
{
    public int OrderId { get; set; }
    public int VendorId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string OrderStatus { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}
