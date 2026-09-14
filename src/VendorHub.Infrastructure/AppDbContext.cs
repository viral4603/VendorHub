using Microsoft.EntityFrameworkCore;

namespace VendorHub.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // DbSets will go here once entities are created, e.g.:
    // public DbSet<Product> Products => Set<Product>();
    // public DbSet<Vendor> Vendors => Set<Vendor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Fluent API configurations will go here
    }
}