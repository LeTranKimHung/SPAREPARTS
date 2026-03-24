using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SparePartsManagement.Models;

namespace SparePartsManagement.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // 🔹 2. Trang quản trị (Admin) - Quản lý sản phẩm (CRUD)
    public DbSet<Product> Products { get; set; } = default!;

    // 🔹 2. Trang quản trị (Admin) - Quản lý loại phụ tùng
    public DbSet<Category> Categories { get; set; } = default!;

    // 🔹 2. Trang quản trị (Admin) - Quản lý Hãng xe
    public DbSet<Brand> Brands { get; set; } = default!;

    // 🔹 2. Trang quản trị (Admin) - Quản lý đơn hàng
    public DbSet<Order> Orders { get; set; } = default!;
    public DbSet<OrderItem> OrderItems { get; set; } = default!;

    // ⭐ Chức năng nâng cao: Đánh giá sản phẩm
    public DbSet<Review> Reviews { get; set; } = default!;

    // 🔧 Đăng ký bảo dưỡng
    public DbSet<ServiceBooking> ServiceBookings { get; set; } = default!;

    // 🎁 Áp dụng khuyến mãi
    public DbSet<Coupon> Coupons { get; set; } = default!;

    // Note: User management is handled by IdentityDbContext (Base)

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Seed some data if needed for test
        // 🔹 1. Trang người dùng (User) - Tìm kiếm (Lốp xe, Dầu nhớt, Ắc quy, Phanh, Đèn xe...)
        builder.Entity<Category>().HasData(
            new Category { CategoryId = 1, Name = "Lốp xe", Description = "Lốp xe các hãng" },
            new Category { CategoryId = 2, Name = "Dầu nhớt", Description = "Dầu nhớt động cơ" },
            new Category { CategoryId = 3, Name = "Ắc quy", Description = "Ắc quy xe điện/xăng" },
            new Category { CategoryId = 4, Name = "Phanh", Description = "Má phanh, đĩa phanh" },
            new Category { CategoryId = 5, Name = "Đèn xe", Description = "Đèn pha, đèn hậu" }
        );

        builder.Entity<Brand>().HasData(
             new Brand { BrandId = 1, Name = "Toyota" },
             new Brand { BrandId = 2, Name = "Honda" },
             new Brand { BrandId = 3, Name = "Mazda" },
             new Brand { BrandId = 4, Name = "Yamaha" }
        );
    }
}
