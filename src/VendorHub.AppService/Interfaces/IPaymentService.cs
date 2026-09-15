using VendorHub.Contracts.Payment;

namespace VendorHub.AppService.Interfaces;

public interface IPaymentService
{
    Task<PaymentResponseDto> PayAsync(int userId, PayRequestDto request);
    Task<List<PaymentResponseDto>> GetMyPaymentsAsync(int userId);
    Task<PaymentResponseDto> GetByIdAsync(int userId, string role, int paymentId);
    Task<PaymentStatusResponseDto> GetOrderPaymentStatusAsync(int userId, string role, int orderId);
    Task<PaymentResponseDto> RefundAsync(int paymentId);
}
