using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SparePartsManagement.Data;
using SparePartsManagement.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace SparePartsManagement.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _context.Products.Include(p => p.Category).Take(8).ToListAsync();
        ViewBag.Reviews = await _context.Reviews
            .Include(r => r.Product)
            .OrderByDescending(r => r.CreatedAt)
            .Take(3)
            .ToListAsync();
        return View(products);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }

    public IActionResult Service()
    {
        return View(new ServiceBooking());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BookService(ServiceBooking model)
    {
        if (ModelState.IsValid)
        {
            model.CreatedAt = System.DateTime.Now;
            model.Status = BookingStatus.Pending;
            if (User.Identity?.IsAuthenticated == true)
            {
                model.UserEmail = User.Identity.Name;
            }
            _context.ServiceBookings.Add(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Bạn đã đặt lịch bảo dưỡng thành công! Bộ phận CSKH sẽ sớm liên hệ lại để xác nhận.";
            return RedirectToAction(nameof(Service));
        }
        
        TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ thông tin bắt buộc.";
        return View("Service", model);
    }

    [Authorize]
    public async Task<IActionResult> MyBookings()
    {
        var email = User.Identity?.Name;
        var bookings = await _context.ServiceBookings
            .Where(b => b.UserEmail == email)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
        return View(bookings);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
