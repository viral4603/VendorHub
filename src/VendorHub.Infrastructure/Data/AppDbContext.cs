using Microsoft.EntityFrameworkCore;
using VendorHub.Domain.Entities;
using VendorHub.Domain.Enums;
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

        modelBuilder.Entity<Role>().HasData(
            new Role { Id = (int)RoleType.Admin, Name = RoleType.Admin.ToString() },
            new Role { Id = (int)RoleType.Vendor, Name = RoleType.Vendor.ToString() },
            new Role { Id = (int)RoleType.Customer, Name = RoleType.Customer.ToString() }
        );
    }
}
