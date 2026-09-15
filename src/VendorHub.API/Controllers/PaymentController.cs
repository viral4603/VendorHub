using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorHub.AppService.Exceptions;
using VendorHub.AppService.Interfaces;
using VendorHub.Contracts.Common;
using VendorHub.Contracts.Payment;

namespace VendorHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("pay")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Pay(PayRequestDto request)
    {
        try
        {
            var payment = await _paymentService.PayAsync(GetCurrentUserId(), request);
            return Ok(ApiResponse<PaymentResponseDto>.SuccessResponse(payment, "Payment processed successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PaymentResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<PaymentResponseDto>.FailureResponse(ex.Message));
        }
        catch (BusinessRuleException ex)
        {
            return Conflict(ApiResponse<PaymentResponseDto>.FailureResponse(ex.Message, ex.Errors));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<PaymentResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("my-payments")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMyPayments()
    {
        try
        {
            var payments = await _paymentService.GetMyPaymentsAsync(GetCurrentUserId());
            return Ok(ApiResponse<List<PaymentResponseDto>>.SuccessResponse(payments, "Payments retrieved successfully"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<List<PaymentResponseDto>>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var payment = await _paymentService.GetByIdAsync(GetCurrentUserId(), GetCurrentUserRole(), id);
            return Ok(ApiResponse<PaymentResponseDto>.SuccessResponse(payment, "Payment retrieved successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PaymentResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<PaymentResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("order/{orderId:int}/status")]
    [Authorize(Roles = "Customer,Vendor,Admin")]
    public async Task<IActionResult> GetOrderPaymentStatus(int orderId)
    {
        try
        {
            var status = await _paymentService.GetOrderPaymentStatusAsync(GetCurrentUserId(), GetCurrentUserRole(), orderId);
            return Ok(ApiResponse<PaymentStatusResponseDto>.SuccessResponse(status, "Payment status retrieved successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PaymentStatusResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<PaymentStatusResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{id:int}/refund")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Refund(int id)
    {
        try
        {
            var payment = await _paymentService.RefundAsync(id);
            return Ok(ApiResponse<PaymentResponseDto>.SuccessResponse(payment, "Payment refunded successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PaymentResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<PaymentResponseDto>.FailureResponse(ex.Message));
        }
    }

    private int GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userId, out var parsedUserId))
            throw new UnauthorizedAccessException("The current user could not be identified from the token.");

        return parsedUserId;
    }

    private string GetCurrentUserRole() =>
        User.FindFirstValue(ClaimTypes.Role)
            ?? throw new UnauthorizedAccessException("The current user's role could not be read from the token.");
}
