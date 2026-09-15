namespace VendorHub.Domain.Constants;

public static class CategoryDefaults
{
    // Root category every product without an explicit one is filed under. Seeded with
    // a fixed id well above the identity range so it never collides with a category
    // an admin created, and so the id is the same on every database.
    public const int OthersId = 1000;
    public const string OthersName = "Others";
}
