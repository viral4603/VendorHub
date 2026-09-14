using Microsoft.EntityFrameworkCore;
using VendorHub.AppService.Interfaces;
using VendorHub.Infrastructure.Data;
using VendorEntity = VendorHub.Domain.Entities.Vendor;

namespace VendorHub.Infrastructure.Repositories;

public class VendorRepository : IVendorRepository
{
    private readonly AppDbContext _context;

    public VendorRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<VendorEntity?> GetByIdAsync(int id) =>
        await _context.Vendors.Include(v => v.User).FirstOrDefaultAsync(v => v.Id == id);

    public async Task<VendorEntity?> GetByUserIdAsync(int userId) =>
        await _context.Vendors.Include(v => v.User).FirstOrDefaultAsync(v => v.UserId == userId);

    public async Task<List<VendorEntity>> GetAllAsync() =>
        await _context.Vendors.Include(v => v.User).OrderByDescending(v => v.CreatedAt).ToListAsync();

    public async Task AddAsync(VendorEntity vendor) =>
        await _context.Vendors.AddAsync(vendor);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
