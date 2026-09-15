using VendorHub.Domain.Entities;

namespace VendorHub.AppService.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id);
    Task<List<Order>> GetByCustomerUserIdAsync(int customerUserId);
    Task<List<Order>> GetByVendorIdAsync(int vendorId);
    Task AddRangeAsync(IEnumerable<Order> orders);
    Task SaveChangesAsync();
}
