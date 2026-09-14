namespace VendorHub.Contracts.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string message = "Request successful") =>
        new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> FailureResponse(string message, List<string>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors };
}

public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string>? Errors { get; set; }

    public static ApiResponse SuccessResponse(string message = "Request successful") =>
        new() { Success = true, Message = message };

    public static ApiResponse FailureResponse(string message, List<string>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors };
}