namespace VendorHub.Contracts.Payment;

public class PaymentResponseDto
{
    public int Id { get; set; }
    public int CustomerUserId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string TransactionReference { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<PaymentOrderSummaryDto> Orders { get; set; } = new();
}
