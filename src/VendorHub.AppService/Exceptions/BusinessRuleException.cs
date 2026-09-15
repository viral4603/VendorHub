namespace VendorHub.AppService.Exceptions;

/// <summary>
/// A business-rule failure that carries several per-item messages (for example, every
/// product that failed the stock check). It derives from <see cref="InvalidOperationException"/>
/// so existing controller catch blocks keep mapping it to Conflict; controllers that want
/// the detail list catch this type first and pass <see cref="Errors"/> into ApiResponse.
/// </summary>
public class BusinessRuleException : InvalidOperationException
{
    public List<string> Errors { get; }

    public BusinessRuleException(string message, List<string> errors) : base(message)
    {
        Errors = errors;
    }
}
