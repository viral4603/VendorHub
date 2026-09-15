using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorHub.AppService.Exceptions;
using VendorHub.AppService.Interfaces;
using VendorHub.Contracts.Common;
using VendorHub.Contracts.Order;

namespace VendorHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost("checkout")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Checkout(CheckoutRequestDto request)
    {
        try
        {
            var orders = await _orderService.CheckoutAsync(GetCurrentUserId(), request);
            return Ok(ApiResponse<List<OrderResponseDto>>.SuccessResponse(orders, "Checkout completed successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<List<OrderResponseDto>>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<List<OrderResponseDto>>.FailureResponse(ex.Message));
        }
        catch (BusinessRuleException ex)
        {
            return Conflict(ApiResponse<List<OrderResponseDto>>.FailureResponse(ex.Message, ex.Errors));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<List<OrderResponseDto>>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("my-orders")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMyOrders()
    {
        try
        {
            var orders = await _orderService.GetMyOrdersAsync(GetCurrentUserId());
            return Ok(ApiResponse<List<OrderResponseDto>>.SuccessResponse(orders, "Orders retrieved successfully"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<List<OrderResponseDto>>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("vendor-orders")]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> GetVendorOrders()
    {
        try
        {
            var orders = await _orderService.GetVendorOrdersAsync(GetCurrentUserId());
            return Ok(ApiResponse<List<OrderResponseDto>>.SuccessResponse(orders, "Orders retrieved successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<List<OrderResponseDto>>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<List<OrderResponseDto>>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Customer,Vendor,Admin")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            // Role is passed through to the service, which decides what this caller may
            // see — the attribute above only gates who may reach the endpoint at all.
            var order = await _orderService.GetByIdAsync(GetCurrentUserId(), GetCurrentUserRole(), id);
            return Ok(ApiResponse<OrderResponseDto>.SuccessResponse(order, "Order retrieved successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<OrderResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<OrderResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateOrderStatusRequestDto request)
    {
        try
        {
            var order = await _orderService.UpdateStatusAsync(GetCurrentUserId(), id, request);
            return Ok(ApiResponse<OrderResponseDto>.SuccessResponse(order, "Order status updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<OrderResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<OrderResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<OrderResponseDto>.FailureResponse(ex.Message));
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
