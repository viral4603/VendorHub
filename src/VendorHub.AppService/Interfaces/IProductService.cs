using VendorHub.Contracts.Catalog;

namespace VendorHub.AppService.Interfaces;

public interface IProductService
{
    Task<List<ProductResponseDto>> GetAllAsync(ProductFilterDto filter);
    Task<ProductResponseDto> GetByIdAsync(int id);
    Task<List<ProductResponseDto>> GetMyProductsAsync(int userId);
    Task<ProductResponseDto> CreateAsync(int userId, ProductCreateDto request);
    Task<ProductResponseDto> UpdateAsync(int userId, int productId, ProductUpdateDto request);
    Task DeleteAsync(int userId, int productId);
}
