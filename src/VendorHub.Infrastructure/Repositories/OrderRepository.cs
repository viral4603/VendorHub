using Microsoft.EntityFrameworkCore;
using VendorHub.AppService.Interfaces;
using VendorHub.Domain.Entities;
using VendorHub.Infrastructure.Data;
using OrderEntity = VendorHub.Domain.Entities.Order;

namespace VendorHub.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<OrderEntity?> GetByIdAsync(int id) =>
        await _context.Orders
            .Include(o => o.Vendor)
            .Include(o => o.CustomerUser)
            .Include(o => o.Items)
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<List<OrderEntity>> GetByIdsAsync(IEnumerable<int> ids)
    {
        var idList = ids.Distinct().ToList();

        return await _context.Orders
            .Include(o => o.Vendor)
            .Include(o => o.CustomerUser)
            .Include(o => o.Payment)
            .Where(o => idList.Contains(o.Id))
            .ToListAsync();
    }

    public async Task<List<OrderEntity>> GetByCustomerUserIdAsync(int customerUserId) =>
        await _context.Orders
            .Include(o => o.Vendor)
            .Include(o => o.CustomerUser)
            .Include(o => o.Items)
            .Where(o => o.CustomerUserId == customerUserId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

    public async Task<List<OrderEntity>> GetByVendorIdAsync(int vendorId) =>
        await _context.Orders
            .Include(o => o.Vendor)
            .Include(o => o.CustomerUser)
            .Include(o => o.Items)
            .Where(o => o.VendorId == vendorId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

    public async Task AddRangeAsync(IEnumerable<OrderEntity> orders) =>
        await _context.Orders.AddRangeAsync(orders);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
