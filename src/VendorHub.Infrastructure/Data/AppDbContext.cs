using Microsoft.EntityFrameworkCore;
using VendorHub.Domain.Entities;
using VendorHub.Domain.Enums;
using OrderEntity = VendorHub.Domain.Entities.Order;
using VendorEntity = VendorHub.Domain.Entities.Vendor;

namespace VendorHub.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<VendorEntity> Vendors => Set<VendorEntity>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<OrderEntity> Orders => Set<OrderEntity>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
            entity.Property(u => u.Name).IsRequired().HasMaxLength(200);

            entity.HasOne(u => u.Role)
                  .WithMany(r => r.Users)
                  .HasForeignKey(u => u.RoleId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<VendorEntity>(entity =>
        {
            entity.Property(v => v.ShopName).IsRequired().HasMaxLength(200);
            entity.Property(v => v.Description).HasMaxLength(1000);
            entity.Property(v => v.Status).HasConversion<int>();

            // One vendor profile per user.
            entity.HasIndex(v => v.UserId).IsUnique();

            entity.HasOne(v => v.User)
                  .WithOne()
                  .HasForeignKey<VendorEntity>(v => v.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(c => c.Name).IsRequired().HasMaxLength(200);

            // HasFilter(null) drops EF's default "IS NOT NULL" filter so the uniqueness
            // also covers root categories, where ParentCategoryId is null.
            entity.HasIndex(c => new { c.Name, c.ParentCategoryId }).IsUnique().HasFilter(null);

            // Self-referencing hierarchy: a category may have a parent and many children.
            entity.HasOne(c => c.ParentCategory)
                  .WithMany(c => c.SubCategories)
                  .HasForeignKey(c => c.ParentCategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Description).HasMaxLength(2000);
            entity.Property(p => p.ImageUrl).HasMaxLength(500);
            entity.Property(p => p.Price).HasPrecision(18, 2);

            entity.HasIndex(p => p.Name);

            entity.HasOne(p => p.Vendor)
                  .WithMany(v => v.Products)
                  .HasForeignKey(p => p.VendorId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderEntity>(entity =>
        {
            entity.Property(o => o.Status).HasConversion<int>();
            entity.Property(o => o.TotalAmount).HasPrecision(18, 2);

            entity.HasIndex(o => o.CustomerUserId);
            entity.HasIndex(o => o.VendorId);

            entity.HasOne(o => o.CustomerUser)
                  .WithMany()
                  .HasForeignKey(o => o.CustomerUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(o => o.Vendor)
                  .WithMany()
                  .HasForeignKey(o => o.VendorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(i => i.ProductName).IsRequired().HasMaxLength(200);
            entity.Property(i => i.UnitPrice).HasPrecision(18, 2);
            entity.Property(i => i.LineTotal).HasPrecision(18, 2);

            // Items have no life of their own — deleting an order deletes its lines.
            entity.HasOne(i => i.Order)
                  .WithMany(o => o.Items)
                  .HasForeignKey(i => i.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Restrict on Product: the name and price are already snapshotted on the
            // item, and products are soft-deleted rather than removed.
            entity.HasOne(i => i.Product)
                  .WithMany()
                  .HasForeignKey(i => i.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Role>().HasData(
            new Role { Id = (int)RoleType.Admin, Name = RoleType.Admin.ToString() },
            new Role { Id = (int)RoleType.Vendor, Name = RoleType.Vendor.ToString() },
            new Role { Id = (int)RoleType.Customer, Name = RoleType.Customer.ToString() }
        );
    }
}
