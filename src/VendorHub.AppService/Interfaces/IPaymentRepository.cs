using VendorHub.Domain.Entities;

namespace VendorHub.AppService.Interfaces;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(int id);
    Task<List<Payment>> GetByCustomerUserIdAsync(int customerUserId);
    Task AddAsync(Payment payment);
    Task SaveChangesAsync();
}
