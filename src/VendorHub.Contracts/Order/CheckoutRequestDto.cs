namespace VendorHub.Contracts.Order;

public class CheckoutRequestDto
{
    // Items are supplied directly by the caller for now. When the Cart module lands,
    // this request becomes empty and the items are read from the customer's cart.
    public List<CheckoutItemDto> Items { get; set; } = new();
}
