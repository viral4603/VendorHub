namespace VendorHub.Contracts.Payment;

public class PaymentStatusResponseDto
{
    public int OrderId { get; set; }
    public decimal OrderTotal { get; set; }

    // IsPaid is a convenience flag so callers need not compare status strings; it is
    // true only for a Completed payment, not a Pending cash-on-delivery one.
    public bool IsPaid { get; set; }

    // Everything below is null while the order has no payment linked to it yet.
    public int? PaymentId { get; set; }
    public string? PaymentStatus { get; set; }
    public string? Method { get; set; }
    public string? TransactionReference { get; set; }
    public DateTime? PaidAt { get; set; }
}
