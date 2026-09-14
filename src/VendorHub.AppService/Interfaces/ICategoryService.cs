using VendorHub.Contracts.Catalog;

namespace VendorHub.AppService.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync();
    Task<CategoryDto> CreateAsync(CategoryCreateDto request);
}
