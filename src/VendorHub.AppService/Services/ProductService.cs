using VendorHub.AppService.Interfaces;
using VendorHub.Contracts.Catalog;
using VendorHub.Domain.Entities;
using VendorHub.Domain.Enums;
using VendorEntity = VendorHub.Domain.Entities.Vendor;

namespace VendorHub.AppService.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IVendorRepository _vendorRepository;

    public ProductService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IVendorRepository vendorRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _vendorRepository = vendorRepository;
    }

    public async Task<List<ProductResponseDto>> GetAllAsync(ProductFilterDto filter)
    {
        if (filter.MinPrice.HasValue && filter.MaxPrice.HasValue && filter.MinPrice > filter.MaxPrice)
            throw new InvalidOperationException("minPrice cannot be greater than maxPrice.");

        var products = await _productRepository.GetActiveAsync(
            filter.CategoryId,
            string.IsNullOrWhiteSpace(filter.Search) ? null : filter.Search.Trim(),
            filter.MinPrice,
            filter.MaxPrice);

        return products.Select(MapToDto).ToList();
    }

    public async Task<ProductResponseDto> GetByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null || !product.IsActive)
            throw new KeyNotFoundException("Product not found.");

        return MapToDto(product);
    }

    public async Task<List<ProductResponseDto>> GetMyProductsAsync(int userId)
    {
        var vendor = await GetVendorForUserAsync(userId);

        var products = await _productRepository.GetByVendorIdAsync(vendor.Id);
        return products.Select(MapToDto).ToList();
    }

    public async Task<ProductResponseDto> CreateAsync(int userId, ProductCreateDto request)
    {
        var vendor = await GetApprovedVendorForUserAsync(userId);

        ValidateProductInput(request.Name, request.Price, request.Stock);

        if (!await _categoryRepository.ExistsAsync(request.CategoryId))
            throw new KeyNotFoundException("Category not found.");

        var product = new Product
        {
            VendorId = vendor.Id,
            CategoryId = request.CategoryId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Price = request.Price,
            Stock = request.Stock,
            ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim(),
            IsActive = true
        };

        await _productRepository.AddAsync(product);
        await _productRepository.SaveChangesAsync();

        var created = await _productRepository.GetByIdAsync(product.Id);
        return MapToDto(created ?? product);
    }

    public async Task<ProductResponseDto> UpdateAsync(int userId, int productId, ProductUpdateDto request)
    {
        var vendor = await GetApprovedVendorForUserAsync(userId);
        var product = await GetOwnedProductAsync(vendor.Id, productId);

        ValidateProductInput(request.Name, request.Price, request.Stock);

        if (!await _categoryRepository.ExistsAsync(request.CategoryId))
            throw new KeyNotFoundException("Category not found.");

        product.CategoryId = request.CategoryId;
        product.Name = request.Name.Trim();
        product.Description = request.Description?.Trim() ?? string.Empty;
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim();
        product.IsActive = request.IsActive;

        await _productRepository.SaveChangesAsync();

        var updated = await _productRepository.GetByIdAsync(product.Id);
        return MapToDto(updated ?? product);
    }

    public async Task DeleteAsync(int userId, int productId)
    {
        var vendor = await GetApprovedVendorForUserAsync(userId);
        var product = await GetOwnedProductAsync(vendor.Id, productId);

        if (!product.IsActive)
            throw new InvalidOperationException("This product has already been deleted.");

        // Soft delete — the row is kept so existing orders keep referencing it.
        product.IsActive = false;

        await _productRepository.SaveChangesAsync();
    }

    private async Task<VendorEntity> GetVendorForUserAsync(int userId) =>
        await _vendorRepository.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("No vendor profile found for the current user.");

    private async Task<VendorEntity> GetApprovedVendorForUserAsync(int userId)
    {
        var vendor = await GetVendorForUserAsync(userId);

        if (vendor.Status != VendorStatus.Approved)
            throw new InvalidOperationException($"Your vendor account is {vendor.Status.ToString().ToLower()}. Only approved vendors can manage products.");

        return vendor;
    }

    private async Task<Product> GetOwnedProductAsync(int vendorId, int productId)
    {
        var product = await _productRepository.GetByIdAsync(productId)
            ?? throw new KeyNotFoundException("Product not found.");

        if (product.VendorId != vendorId)
            throw new UnauthorizedAccessException("You can only manage your own products.");

        return product;
    }

    private static void ValidateProductInput(string name, decimal price, int stock)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Product name is required.");

        if (price <= 0)
            throw new InvalidOperationException("Price must be greater than zero.");

        if (stock < 0)
            throw new InvalidOperationException("Stock cannot be negative.");
    }

    private static ProductResponseDto MapToDto(Product product) => new()
    {
        Id = product.Id,
        VendorId = product.VendorId,
        ShopName = product.Vendor?.ShopName ?? string.Empty,
        CategoryId = product.CategoryId,
        CategoryName = product.Category?.Name ?? string.Empty,
        Name = product.Name,
        Description = product.Description,
        Price = product.Price,
        Stock = product.Stock,
        ImageUrl = product.ImageUrl,
        IsActive = product.IsActive,
        CreatedAt = product.CreatedAt
    };
}
