using VendorHub.AppService.Exceptions;
using VendorHub.AppService.Interfaces;
using VendorHub.Contracts.Order;
using VendorHub.Domain.Entities;
using VendorHub.Domain.Enums;
using OrderEntity = VendorHub.Domain.Entities.Order;
using VendorEntity = VendorHub.Domain.Entities.Vendor;

namespace VendorHub.AppService.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IVendorRepository _vendorRepository;

    public OrderService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IVendorRepository vendorRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _vendorRepository = vendorRepository;
    }

    public async Task<List<OrderResponseDto>> CheckoutAsync(int userId, CheckoutRequestDto request)
    {
        // Step 1 — where the purchased lines come from. This is the only method that
        // knows about the request body; swapping it for a Cart lookup later leaves
        // the validation / splitting / stock logic below untouched.
        var lines = await ResolveCheckoutLinesAsync(userId, request);

        // Step 2 — validate everything before a single write happens, so a bad line
        // never leaves partially created orders behind.
        ValidateStock(lines);

        // Step 3 — one Order per vendor, each carrying only that vendor's lines.
        var orders = BuildPerVendorOrders(userId, lines);

        // Step 4 — reduce stock. The product entities are tracked by the same scoped
        // DbContext as the new orders, so the single SaveChanges below persists both
        // the orders and the stock reductions together.
        foreach (var line in lines)
            line.Product.Stock -= line.Quantity;

        await _orderRepository.AddRangeAsync(orders);
        await _orderRepository.SaveChangesAsync();

        return orders.Select(MapToDto).ToList();
    }

    public async Task<List<OrderResponseDto>> GetMyOrdersAsync(int userId)
    {
        var orders = await _orderRepository.GetByCustomerUserIdAsync(userId);
        return orders.Select(MapToDto).ToList();
    }

    public async Task<List<OrderResponseDto>> GetVendorOrdersAsync(int userId)
    {
        var vendor = await GetVendorForUserAsync(userId);

        var orders = await _orderRepository.GetByVendorIdAsync(vendor.Id);
        return orders.Select(MapToDto).ToList();
    }

    public async Task<OrderResponseDto> GetByIdAsync(int userId, string role, int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId)
            ?? throw new KeyNotFoundException("Order not found.");

        await EnsureCanViewAsync(userId, role, order);

        return MapToDto(order);
    }

    public async Task<OrderResponseDto> UpdateStatusAsync(int userId, int orderId, UpdateOrderStatusRequestDto request)
    {
        var vendor = await GetVendorForUserAsync(userId);

        var order = await _orderRepository.GetByIdAsync(orderId)
            ?? throw new KeyNotFoundException("Order not found.");

        if (order.VendorId != vendor.Id)
            throw new UnauthorizedAccessException("You can only update orders that belong to your own shop.");

        if (!Enum.TryParse<OrderStatus>(request.Status, ignoreCase: true, out var newStatus)
            || !Enum.IsDefined(newStatus))
        {
            throw new InvalidOperationException(
                $"'{request.Status}' is not a valid order status. Allowed values: {string.Join(", ", Enum.GetNames<OrderStatus>())}.");
        }

        EnsureStatusTransitionAllowed(order.Status, newStatus);

        order.Status = newStatus;
        await _orderRepository.SaveChangesAsync();

        return MapToDto(order);
    }

    /// <summary>
    /// Resolves the lines being purchased. Today they come straight from the request
    /// body; when the Cart module is added this reads the customer's cart instead and
    /// nothing else in <see cref="CheckoutAsync"/> has to change.
    /// </summary>
    private async Task<List<CheckoutLine>> ResolveCheckoutLinesAsync(int userId, CheckoutRequestDto request)
    {
        if (request.Items == null || request.Items.Count == 0)
            throw new InvalidOperationException("Checkout requires at least one item.");

        if (request.Items.Any(i => i.Quantity <= 0))
            throw new InvalidOperationException("Every item quantity must be greater than zero.");

        // The same product listed twice is treated as one line with the summed
        // quantity, so the stock check sees the real total being requested.
        var requested = request.Items
            .GroupBy(i => i.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

        var products = await _productRepository.GetByIdsAsync(requested.Keys);
        var productsById = products.ToDictionary(p => p.Id);

        var missing = requested.Keys.Where(id => !productsById.ContainsKey(id)).ToList();
        if (missing.Count > 0)
            throw new KeyNotFoundException($"Product(s) not found: {string.Join(", ", missing)}.");

        var lines = new List<CheckoutLine>();
        var unavailable = new List<string>();

        foreach (var (productId, quantity) in requested)
        {
            var product = productsById[productId];

            if (!product.IsActive)
                unavailable.Add($"'{product.Name}' is no longer available for purchase.");
            else if (product.Vendor.Status != VendorStatus.Approved)
                unavailable.Add($"'{product.Name}' belongs to a vendor that is not approved.");
            else
                lines.Add(new CheckoutLine(product, quantity));
        }

        if (unavailable.Count > 0)
            throw new BusinessRuleException("Checkout failed — some items cannot be purchased.", unavailable);

        return lines;
    }

    private static void ValidateStock(List<CheckoutLine> lines)
    {
        var failures = lines
            .Where(l => l.Product.Stock < l.Quantity)
            .Select(l => $"'{l.Product.Name}' (product {l.Product.Id}) — requested {l.Quantity}, only {l.Product.Stock} in stock.")
            .ToList();

        if (failures.Count > 0)
            throw new BusinessRuleException("Checkout failed — insufficient stock. No orders were created.", failures);
    }

    private static List<OrderEntity> BuildPerVendorOrders(int userId, List<CheckoutLine> lines) =>
        lines
            .GroupBy(l => l.Product.VendorId)
            .Select(group =>
            {
                var order = new OrderEntity
                {
                    CustomerUserId = userId,
                    VendorId = group.Key,
                    Status = OrderStatus.Pending,
                    Items = group.Select(line => new OrderItem
                    {
                        ProductId = line.Product.Id,
                        // Name and price are copied now — the product row may change later.
                        ProductName = line.Product.Name,
                        UnitPrice = line.Product.Price,
                        Quantity = line.Quantity,
                        LineTotal = line.Product.Price * line.Quantity
                    }).ToList()
                };

                order.TotalAmount = order.Items.Sum(i => i.LineTotal);
                return order;
            })
            .ToList();

    private async Task EnsureCanViewAsync(int userId, string role, OrderEntity order)
    {
        if (string.Equals(role, RoleType.Admin.ToString(), StringComparison.OrdinalIgnoreCase))
            return;

        if (string.Equals(role, RoleType.Vendor.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            var vendor = await GetVendorForUserAsync(userId);

            if (order.VendorId != vendor.Id)
                throw new UnauthorizedAccessException("You can only view orders that belong to your own shop.");

            return;
        }

        if (order.CustomerUserId != userId)
            throw new UnauthorizedAccessException("You can only view your own orders.");
    }

    private static void EnsureStatusTransitionAllowed(OrderStatus current, OrderStatus next)
    {
        if (current == next)
            throw new InvalidOperationException($"This order is already {current}.");

        var allowed = current switch
        {
            OrderStatus.Pending => new[] { OrderStatus.Processing, OrderStatus.Cancelled },
            OrderStatus.Processing => new[] { OrderStatus.Shipped, OrderStatus.Cancelled },
            OrderStatus.Shipped => new[] { OrderStatus.Delivered },
            _ => Array.Empty<OrderStatus>()
        };

        if (!allowed.Contains(next))
        {
            var options = allowed.Length == 0
                ? $"{current} is a final status and cannot be changed."
                : $"Allowed next statuses: {string.Join(", ", allowed)}.";

            throw new InvalidOperationException($"Cannot move an order from {current} to {next}. {options}");
        }
    }

    private async Task<VendorEntity> GetVendorForUserAsync(int userId) =>
        await _vendorRepository.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("No vendor profile found for the current user.");

    private static OrderResponseDto MapToDto(OrderEntity order) => new()
    {
        Id = order.Id,
        CustomerUserId = order.CustomerUserId,
        CustomerName = order.CustomerUser?.Name ?? string.Empty,
        VendorId = order.VendorId,
        ShopName = order.Vendor?.ShopName ?? string.Empty,
        Status = order.Status.ToString(),
        TotalAmount = order.TotalAmount,
        CreatedAt = order.CreatedAt,
        Items = order.Items.Select(item => new OrderItemResponseDto
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            UnitPrice = item.UnitPrice,
            Quantity = item.Quantity,
            LineTotal = item.LineTotal
        }).ToList()
    };

    private sealed record CheckoutLine(Product Product, int Quantity);
}
