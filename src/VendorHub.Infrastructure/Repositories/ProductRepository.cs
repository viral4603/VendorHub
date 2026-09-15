using Microsoft.EntityFrameworkCore;
using VendorHub.AppService.Interfaces;
using VendorHub.Domain.Entities;
using VendorHub.Infrastructure.Data;

namespace VendorHub.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetActiveAsync(int? categoryId, string? search, decimal? minPrice, decimal? maxPrice)
    {
        var query = _context.Products
            .Include(p => p.Vendor)
            .Include(p => p.Category)
            .Where(p => p.IsActive);

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => EF.Functions.Like(p.Name, $"%{search}%"));

        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);

        return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id) =>
        await _context.Products
            .Include(p => p.Vendor)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<List<Product>> GetByIdsAsync(IEnumerable<int> ids)
    {
        var idList = ids.Distinct().ToList();

        return await _context.Products
            .Include(p => p.Vendor)
            .Include(p => p.Category)
            .Where(p => idList.Contains(p.Id))
            .ToListAsync();
    }

    public async Task<List<Product>> GetByVendorIdAsync(int vendorId) =>
        await _context.Products
            .Include(p => p.Vendor)
            .Include(p => p.Category)
            .Where(p => p.VendorId == vendorId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    public async Task AddAsync(Product product) =>
        await _context.Products.AddAsync(product);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
