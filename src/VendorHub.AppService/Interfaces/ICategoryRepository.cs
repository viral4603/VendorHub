using VendorHub.Domain.Entities;

namespace VendorHub.AppService.Interfaces;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> NameExistsAsync(string name, int? parentCategoryId);
    Task AddAsync(Category category);
    Task SaveChangesAsync();
}
