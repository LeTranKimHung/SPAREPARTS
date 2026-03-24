using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SparePartsManagement.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SparePartsManagement.Areas.Admin.Controllers;

public class RevenueData {
    public DateTime Date {get;set;}
    public decimal Revenue {get;set;}
}
public class TopSellingData {
    public string ProductName {get;set;}
    public int TotalSold {get;set;}
}

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var totalProducts = await _context.Products.CountAsync();
        var totalOrders = await _context.Orders.CountAsync();
        var totalUsers = await _context.Users.CountAsync();

        var thisMonthRevenue = await _context.Orders
            .Where(o => o.Status == 2 && o.OrderDate.Month == DateTime.Now.Month && o.OrderDate.Year == DateTime.Now.Year)
            .SumAsync(o => o.TotalAmount);
            
        var ordersByStatus = await _context.Orders
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(k => k.Status, v => v.Count);

        var last7Days = DateTime.Now.AddDays(-7).Date;
        var revenue7Days = await _context.Orders
            .Where(o => o.Status == 2 && o.OrderDate >= last7Days)
            .GroupBy(o => o.OrderDate.Date)
            .Select(g => new RevenueData { Date = g.Key, Revenue = g.Sum(x => x.TotalAmount) })
            .OrderBy(x => x.Date)
            .ToListAsync();
            
        var topSelling = await _context.OrderItems
            .Include(oi => oi.Product)
            .GroupBy(oi => oi.ProductId)
            .Select(g => new TopSellingData {
                ProductName = g.FirstOrDefault().Product.Name ?? "No Name",
                TotalSold = g.Sum(x => x.Quantity)
            })
            .OrderByDescending(x => x.TotalSold)
            .Take(5)
            .ToListAsync();
            
        var lowStock = await _context.Products
            .Where(p => p.StockQuantity < 5)
            .Take(10)
            .ToListAsync();

        ViewBag.TotalProducts = totalProducts;
        ViewBag.TotalOrders = totalOrders;
        ViewBag.TotalUsers = totalUsers;
        ViewBag.ThisMonthRevenue = thisMonthRevenue;
        ViewBag.OrdersByStatus = ordersByStatus;
        ViewBag.Revenue7Days = revenue7Days;
        ViewBag.TopSelling = topSelling;
        ViewBag.LowStock = lowStock;

        return View();
    }
}
