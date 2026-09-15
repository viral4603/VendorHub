using VendorHub.AppService.Exceptions;
using VendorHub.AppService.Interfaces;
using VendorHub.Contracts.Payment;
using VendorHub.Domain.Enums;
using OrderEntity = VendorHub.Domain.Entities.Order;
using PaymentEntity = VendorHub.Domain.Entities.Payment;
using VendorEntity = VendorHub.Domain.Entities.Vendor;

namespace VendorHub.AppService.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IVendorRepository _vendorRepository;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IOrderRepository orderRepository,
        IVendorRepository vendorRepository)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _vendorRepository = vendorRepository;
    }

    public async Task<PaymentResponseDto> PayAsync(int userId, PayRequestDto request)
    {
        var method = ParseMethod(request.Method);
        var orders = await ResolvePayableOrdersAsync(userId, request);

        // The charge is summed from the orders themselves — the request carries no
        // amount, so there is no client-supplied figure to be talked into trusting.
        var amount = orders.Sum(o => o.TotalAmount);

        var payment = new PaymentEntity
        {
            CustomerUserId = userId,
            Amount = amount,
            Method = method,
            // Simulated authorisation: cards and UPI settle immediately, while cash on
            // delivery stays Pending until the courier collects.
            Status = method == PaymentMethod.CashOnDelivery
                ? PaymentStatus.Pending
                : PaymentStatus.Completed,
            TransactionReference = GenerateTransactionReference()
        };

        await _paymentRepository.AddAsync(payment);

        // Assigning the navigation lets EF fill in Order.PaymentId once the payment is
        // inserted, so both sides land in the single SaveChanges below.
        foreach (var order in orders)
            order.Payment = payment;

        await _paymentRepository.SaveChangesAsync();

        // Re-read so the customer and vendor navigations are populated for the response.
        var created = await _paymentRepository.GetByIdAsync(payment.Id);
        return created != null ? MapToDto(created, created.Orders) : MapToDto(payment, orders);
    }

    public async Task<List<PaymentResponseDto>> GetMyPaymentsAsync(int userId)
    {
        var payments = await _paymentRepository.GetByCustomerUserIdAsync(userId);
        return payments.Select(p => MapToDto(p, p.Orders)).ToList();
    }

    public async Task<PaymentResponseDto> GetByIdAsync(int userId, string role, int paymentId)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId)
            ?? throw new KeyNotFoundException("Payment not found.");

        if (!IsAdmin(role) && payment.CustomerUserId != userId)
            throw new UnauthorizedAccessException("You can only view your own payments.");

        return MapToDto(payment, payment.Orders);
    }

    public async Task<PaymentStatusResponseDto> GetOrderPaymentStatusAsync(int userId, string role, int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId)
            ?? throw new KeyNotFoundException("Order not found.");

        await EnsureCanViewOrderAsync(userId, role, order);

        return new PaymentStatusResponseDto
        {
            OrderId = order.Id,
            OrderTotal = order.TotalAmount,
            IsPaid = order.Payment?.Status == PaymentStatus.Completed,
            PaymentId = order.Payment?.Id,
            PaymentStatus = order.Payment?.Status.ToString(),
            Method = order.Payment?.Method.ToString(),
            TransactionReference = order.Payment?.TransactionReference,
            PaidAt = order.Payment?.CreatedAt
        };
    }

    public async Task<PaymentResponseDto> RefundAsync(int paymentId)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId)
            ?? throw new KeyNotFoundException("Payment not found.");

        if (payment.Status != PaymentStatus.Completed)
            throw new InvalidOperationException($"Only a completed payment can be refunded. This payment is {payment.Status}.");

        // Status change only — there is no gateway to return funds through.
        payment.Status = PaymentStatus.Refunded;
        await _paymentRepository.SaveChangesAsync();

        return MapToDto(payment, payment.Orders);
    }

    /// <summary>
    /// Loads the orders being paid for and enforces every rule that must hold before a
    /// payment row is created: the orders exist, belong to this customer, are still
    /// payable, and are not already covered by a live payment.
    /// </summary>
    private async Task<List<OrderEntity>> ResolvePayableOrdersAsync(int userId, PayRequestDto request)
    {
        if (request.OrderIds == null || request.OrderIds.Count == 0)
            throw new InvalidOperationException("At least one order id is required.");

        var orderIds = request.OrderIds.Distinct().ToList();
        var orders = await _orderRepository.GetByIdsAsync(orderIds);

        var missing = orderIds.Where(id => orders.All(o => o.Id != id)).ToList();
        if (missing.Count > 0)
            throw new KeyNotFoundException($"Order(s) not found: {string.Join(", ", missing)}.");

        // Ownership is checked before anything else is reported, so a customer probing
        // someone else's order ids learns nothing about their state.
        var foreign = orders.Where(o => o.CustomerUserId != userId).Select(o => o.Id).ToList();
        if (foreign.Count > 0)
            throw new UnauthorizedAccessException($"You can only pay for your own orders. Not yours: {string.Join(", ", foreign)}.");

        var failures = new List<string>();

        foreach (var order in orders)
        {
            if (order.Status == OrderStatus.Cancelled)
                failures.Add($"Order {order.Id} is cancelled and cannot be paid for.");

            // A Failed payment leaves the order payable again; Pending (cash on
            // delivery) and Completed both count as already covered.
            if (order.Payment != null && order.Payment.Status != PaymentStatus.Failed)
                failures.Add($"Order {order.Id} is already covered by payment {order.Payment.Id} ({order.Payment.Status}).");
        }

        if (failures.Count > 0)
            throw new BusinessRuleException("Payment failed — some orders cannot be paid for.", failures);

        return orders;
    }

    private async Task EnsureCanViewOrderAsync(int userId, string role, OrderEntity order)
    {
        if (IsAdmin(role))
            return;

        if (string.Equals(role, RoleType.Vendor.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            var vendor = await GetVendorForUserAsync(userId);

            if (order.VendorId != vendor.Id)
                throw new UnauthorizedAccessException("You can only view payments for orders belonging to your own shop.");

            return;
        }

        if (order.CustomerUserId != userId)
            throw new UnauthorizedAccessException("You can only view payments for your own orders.");
    }

    private static PaymentMethod ParseMethod(string method)
    {
        if (!Enum.TryParse<PaymentMethod>(method, ignoreCase: true, out var parsed) || !Enum.IsDefined(parsed))
        {
            throw new InvalidOperationException(
                $"'{method}' is not a valid payment method. Allowed values: {string.Join(", ", Enum.GetNames<PaymentMethod>())}.");
        }

        return parsed;
    }

    private static string GenerateTransactionReference() =>
        $"VH-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

    private static bool IsAdmin(string role) =>
        string.Equals(role, RoleType.Admin.ToString(), StringComparison.OrdinalIgnoreCase);

    private async Task<VendorEntity> GetVendorForUserAsync(int userId) =>
        await _vendorRepository.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("No vendor profile found for the current user.");

    private static PaymentResponseDto MapToDto(PaymentEntity payment, IEnumerable<OrderEntity> orders) => new()
    {
        Id = payment.Id,
        CustomerUserId = payment.CustomerUserId,
        CustomerName = payment.CustomerUser?.Name ?? string.Empty,
        Amount = payment.Amount,
        Method = payment.Method.ToString(),
        Status = payment.Status.ToString(),
        TransactionReference = payment.TransactionReference,
        CreatedAt = payment.CreatedAt,
        Orders = orders.Select(order => new PaymentOrderSummaryDto
        {
            OrderId = order.Id,
            VendorId = order.VendorId,
            ShopName = order.Vendor?.ShopName ?? string.Empty,
            OrderStatus = order.Status.ToString(),
            TotalAmount = order.TotalAmount
        }).ToList()
    };
}
