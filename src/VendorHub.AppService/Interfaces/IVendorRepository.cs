using VendorHub.Domain.Entities;

namespace VendorHub.AppService.Interfaces;

public interface IVendorRepository
{
    Task<Vendor?> GetByIdAsync(int id);
    Task<Vendor?> GetByUserIdAsync(int userId);
    Task<List<Vendor>> GetAllAsync();
    Task AddAsync(Vendor vendor);
    Task SaveChangesAsync();
}
