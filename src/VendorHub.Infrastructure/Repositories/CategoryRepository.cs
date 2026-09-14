using Microsoft.EntityFrameworkCore;
using VendorHub.AppService.Interfaces;
using VendorHub.Domain.Entities;
using VendorHub.Infrastructure.Data;

namespace VendorHub.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetAllAsync() =>
        await _context.Categories
            .Include(c => c.ParentCategory)
            .OrderBy(c => c.Name)
            .ToListAsync();

    public async Task<Category?> GetByIdAsync(int id) =>
        await _context.Categories
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<bool> ExistsAsync(int id) =>
        await _context.Categories.AnyAsync(c => c.Id == id);

    public async Task<bool> NameExistsAsync(string name, int? parentCategoryId) =>
        await _context.Categories.AnyAsync(c =>
            c.Name == name && c.ParentCategoryId == parentCategoryId);

    public async Task AddAsync(Category category) =>
        await _context.Categories.AddAsync(category);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
