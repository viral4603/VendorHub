using Microsoft.EntityFrameworkCore;
using VendorHub.AppService.Interfaces;
using VendorHub.Infrastructure.Data;
using PaymentEntity = VendorHub.Domain.Entities.Payment;

namespace VendorHub.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _context;

    public PaymentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentEntity?> GetByIdAsync(int id) =>
        await _context.Payments
            .Include(p => p.CustomerUser)
            .Include(p => p.Orders)
                .ThenInclude(o => o.Vendor)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<List<PaymentEntity>> GetByCustomerUserIdAsync(int customerUserId) =>
        await _context.Payments
            .Include(p => p.CustomerUser)
            .Include(p => p.Orders)
                .ThenInclude(o => o.Vendor)
            .Where(p => p.CustomerUserId == customerUserId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    public async Task AddAsync(PaymentEntity payment) =>
        await _context.Payments.AddAsync(payment);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
