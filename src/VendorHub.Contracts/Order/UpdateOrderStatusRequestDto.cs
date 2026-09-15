namespace VendorHub.Contracts.Order;

public class UpdateOrderStatusRequestDto
{
    // Accepted as a string so the request body reads { "status": "Shipped" };
    // the service parses it against OrderStatus and reports an invalid value.
    public string Status { get; set; } = string.Empty;
}
