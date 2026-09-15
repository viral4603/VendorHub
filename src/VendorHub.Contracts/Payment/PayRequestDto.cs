namespace VendorHub.Contracts.Payment;

public class PayRequestDto
{
    public List<int> OrderIds { get; set; } = new();

    // Parsed against PaymentMethod by the service so the body reads
    // { "method": "CreditCard" }, matching UpdateOrderStatusRequestDto.
    public string Method { get; set; } = string.Empty;

    // Deliberately no Amount property: the charge is always summed from the orders
    // server-side, so a client cannot under-pay by supplying its own figure.
}
