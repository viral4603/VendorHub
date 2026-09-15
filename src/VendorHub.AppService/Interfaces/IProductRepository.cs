using VendorHub.Domain.Entities;

namespace VendorHub.AppService.Interfaces;

public interface IProductRepository
{
    Task<List<Product>> GetActiveAsync(int? categoryId, string? search, decimal? minPrice, decimal? maxPrice);
    Task<Product?> GetByIdAsync(int id);
    Task<List<Product>> GetByIdsAsync(IEnumerable<int> ids);
    Task<List<Product>> GetByVendorIdAsync(int vendorId);
    Task AddAsync(Product product);
    Task SaveChangesAsync();
}
