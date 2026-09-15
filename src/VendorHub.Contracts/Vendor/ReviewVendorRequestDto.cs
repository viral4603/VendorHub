namespace VendorHub.Contracts.Vendor;

public class ReviewVendorRequestDto
{
    // Accepted as a string so the request body reads { "status": "Approved" },
    // matching UpdateOrderStatusRequestDto; the service parses it against
    // VendorStatus and rejects anything other than Approved or Rejected.
    public string Status { get; set; } = string.Empty;
}
