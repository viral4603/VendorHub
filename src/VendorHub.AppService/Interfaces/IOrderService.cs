using VendorHub.Contracts.Order;

namespace VendorHub.AppService.Interfaces;

public interface IOrderService
{
    Task<List<OrderResponseDto>> CheckoutAsync(int userId, CheckoutRequestDto request);
    Task<List<OrderResponseDto>> GetMyOrdersAsync(int userId);
    Task<List<OrderResponseDto>> GetVendorOrdersAsync(int userId);
    Task<OrderResponseDto> GetByIdAsync(int userId, string role, int orderId);
    Task<OrderResponseDto> UpdateStatusAsync(int userId, int orderId, UpdateOrderStatusRequestDto request);
}
