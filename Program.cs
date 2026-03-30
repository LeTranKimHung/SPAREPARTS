using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SparePartsManagement.Data;
using SparePartsManagement.Models;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// 🔹 Cấu hình Session & Cache (Để làm Giỏ hàng)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 🔹 Cấu hình MySQL Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 🔹 Cấu hình Identity (Đăng nhập & Phân quyền)
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => {
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Cấu hình đường dẫn Cookie (Trang Login mặc định)
builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();

// Seed Admin User & Roles (Quan trọng để test phân quyền)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        
        // Tạo Role Admin & User
        string[] roleNames = { "Admin", "User" };
        foreach (var roleName in roleNames) {
            if (!await roleManager.RoleExistsAsync(roleName)) {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Tạo tài khoản Admin mặc định để test
        var adminEmail = "admin@spareparts.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null) {
            var user = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            var result = await userManager.CreateAsync(user, "Admin123");
            if (result.Succeeded) {
                await userManager.AddToRoleAsync(user, "Admin");
                Console.WriteLine("✅ Admin seeded: admin@spareparts.com / Admin123");
            } else {
                Console.WriteLine($"❌ Error seeding admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // Seed Category
        if (!context.Categories.Any()) {
            context.Categories.Add(new Category { Name = "Lọc Gió", Description = "Lọc không khí cao cấp" });
            context.Categories.Add(new Category { Name = "Nhông Sên Dĩa", Description = "Truyền động bền bỉ" });
            await context.SaveChangesAsync();
            Console.WriteLine("✅ Categories seeded.");
        }

        // Seed Brand
        if (!context.Brands.Any()) {
            context.Brands.Add(new Brand { Name = "Honda Vietnam", Description = "Chính hãng Honda" });
            context.Brands.Add(new Brand { Name = "Yamaha Lube", Description = "Công nghệ Nhật Bản" });
            await context.SaveChangesAsync();
            Console.WriteLine("✅ Brands seeded.");
        }

        // Seed Coupon
        if (!context.Coupons.Any())
        {
            context.Coupons.Add(new Coupon { 
                Code = "GIAM10", 
                DiscountPercentage = 10, 
                IsActive = true, 
                ExpiryDate = DateTime.Now.AddDays(30) 
            });
            await context.SaveChangesAsync();
            Console.WriteLine("✅ GIAM10 coupon seeded.");
        }

        // 🧹 Tự động dọn dẹp các sản phẩm rác từ việc chạy UI Test
        var testProducts = context.Products.Where(p => p.Name.StartsWith("UI-TEST-")).ToList();
        if (testProducts.Any())
        {
            context.Products.RemoveRange(testProducts);
            await context.SaveChangesAsync();
            Console.WriteLine($"🧹 Cleaned {testProducts.Count} UI-TEST clutter products.");
        }
    } catch (Exception ex) {
        Console.WriteLine($"💥 Seeding failed: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

// 🔹 Authentication & Authorization Middleware
app.UseAuthentication();
app.UseAuthorization();

// Map các Areas (Admin)
app.MapAreaControllerRoute(
    name: "AdminArea",
    areaName: "Admin",
    pattern: "Admin/{controller=Home}/{action=Index}/{id?}");

// Map Default User route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
