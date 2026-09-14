using VendorHub.AppService.Interfaces;
using VendorHub.Contracts.Catalog;
using VendorHub.Domain.Entities;

namespace VendorHub.AppService.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return categories.Select(MapToDto).ToList();
    }

    public async Task<CategoryDto> CreateAsync(CategoryCreateDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new InvalidOperationException("Category name is required.");

        var name = request.Name.Trim();

        if (request.ParentCategoryId.HasValue)
        {
            var parentExists = await _categoryRepository.ExistsAsync(request.ParentCategoryId.Value);
            if (!parentExists)
                throw new KeyNotFoundException("Parent category not found.");
        }

        if (await _categoryRepository.NameExistsAsync(name, request.ParentCategoryId))
            throw new InvalidOperationException("A category with this name already exists under the same parent.");

        var category = new Category
        {
            Name = name,
            ParentCategoryId = request.ParentCategoryId
        };

        await _categoryRepository.AddAsync(category);
        await _categoryRepository.SaveChangesAsync();

        // Re-read so the parent navigation is populated for the response.
        var created = await _categoryRepository.GetByIdAsync(category.Id);
        return MapToDto(created ?? category);
    }

    private static CategoryDto MapToDto(Category category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        ParentCategoryId = category.ParentCategoryId,
        ParentCategoryName = category.ParentCategory?.Name
    };
}
