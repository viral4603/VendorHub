using Microsoft.EntityFrameworkCore;
using VendorHub.AppService.Interfaces;
using VendorHub.Domain.Entities;
using VendorHub.Infrastructure.Data;

namespace VendorHub.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email) =>
        await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == email);

    public async Task<User?> GetByIdAsync(int id) =>
        await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);

    public async Task AddAsync(User user) =>
        await _context.Users.AddAsync(user);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}