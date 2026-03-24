using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SparePartsManagement.Models;
using SparePartsManagement.Data;
using System.Threading.Tasks;

namespace SparePartsManagement.Controllers;

public class PublicProductController : Controller
{
    private readonly ApplicationDbContext _context;

    public PublicProductController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? keyword, int? category, int? brand, decimal? minPrice, decimal? maxPrice, string? sort, int page = 1)
    {
        var products = _context.Products.Include(p => p.Category).Include(p => p.Brand).AsQueryable();

        // Filter
        if (!string.IsNullOrEmpty(keyword))
        {
            products = products.Where(p => p.Name.Contains(keyword) 
                     || (p.CarModel != null && p.CarModel.Contains(keyword)) 
                     || (p.Brand != null && p.Brand.Name.Contains(keyword))
                     || (p.Category != null && p.Category.Name.Contains(keyword)));
        }
        if (category.HasValue) products = products.Where(p => p.CategoryId == category.Value);
        if (brand.HasValue) products = products.Where(p => p.BrandId == brand.Value);
        if (minPrice.HasValue) products = products.Where(p => p.Price >= minPrice.Value);
        if (maxPrice.HasValue) products = products.Where(p => p.Price <= maxPrice.Value);

        // Sort
        products = sort switch
        {
            "price_asc" => products.OrderBy(p => p.Price),
            "price_desc" => products.OrderByDescending(p => p.Price),
            "oldest" => products.OrderBy(p => p.ProductId),
            _ => products.OrderByDescending(p => p.ProductId) // Mới nhất
        };

        ViewBag.CurrentSort = sort;
        ViewBag.Keyword = keyword;
        ViewBag.Category = category;
        ViewBag.Brand = brand;
        ViewBag.MinPrice = minPrice;
        ViewBag.MaxPrice = maxPrice;

        int pageSize = 9;
        var totalItems = await products.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        
        var pagedProducts = await products.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;

        return View(pagedProducts);
    }

    // 🔹 1. Trang người dùng (User) - Tìm kiếm (theo tên, hãng xe, loại xe)
    public async Task<IActionResult> Search(string? keyword)
    {
        if (string.IsNullOrEmpty(keyword)) return RedirectToAction(nameof(Index));

        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Where(p => p.Name.Contains(keyword) 
                     || p.CarModel.Contains(keyword) 
                     || p.Brand.Name.Contains(keyword)
                     || p.Category.Name.Contains(keyword))
            .ToListAsync();

        return View("Index", products);
    }

    // 🔹 1. Trang người dùng (User) - Xem chi tiết sản phẩm
    public async Task<IActionResult> Details(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Reviews)
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (product == null) return NotFound();

        return View(product);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> PostReview([FromBody] ReviewRequest request)
    {
        if (request == null || request.ProductId <= 0 || request.StarRating < 1 || request.StarRating > 5 || string.IsNullOrWhiteSpace(request.Comment))
        {
            return BadRequest(new { success = false, message = "Dữ liệu đánh giá không hợp lệ hoặc thiếu thông tin." });
        }

        var review = new Review
        {
            ProductId = request.ProductId,
            StarRating = request.StarRating,
            Comment = request.Comment,
            UserEmail = User.Identity?.Name ?? "Anonym",
            CreatedAt = DateTime.Now
        };
        
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        return Ok(new { 
            success = true, 
            message = "Đánh giá của bạn đã được lưu thành công!",
            data = new {
                userEmail = review.UserEmail,
                starRating = review.StarRating,
                comment = review.Comment,
                createdAt = review.CreatedAt.ToString("dd/MM/yyyy HH:mm")
            }
        });
    }

    public class ReviewRequest
    {
        public int ProductId { get; set; }
        public int StarRating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}
