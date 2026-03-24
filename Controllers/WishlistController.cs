using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SparePartsManagement.Helpers;
using SparePartsManagement.ViewModels;
using SparePartsManagement.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SparePartsManagement.Controllers;

public class WishlistController : Controller
{
    private readonly ApplicationDbContext _context;
    private const string WISHLIST_KEY = "WishlistItems";

    public WishlistController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var wishlist = HttpContext.Session.GetSession<List<CartItem>>(WISHLIST_KEY) ?? new List<CartItem>();
        return View(wishlist);
    }

    [HttpPost]
    public async Task<IActionResult> Toggle(int id)
    {
        var wishlist = HttpContext.Session.GetSession<List<CartItem>>(WISHLIST_KEY) ?? new List<CartItem>();
        var item = wishlist.FirstOrDefault(i => i.ProductId == id);

        if (item != null)
        {
            wishlist.Remove(item);
        }
        else
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                wishlist.Add(new CartItem
                {
                    ProductId = product.ProductId,
                    ProductName = product.Name,
                    Price = product.Price,
                    ImageUrl = product.ImageUrl
                });
            }
        }

        HttpContext.Session.SetSession(WISHLIST_KEY, wishlist);
        return Redirect(Request.Headers["Referer"].ToString() ?? "/PublicProduct");
    }

    [HttpPost]
    public IActionResult Remove(int id)
    {
        var wishlist = HttpContext.Session.GetSession<List<CartItem>>(WISHLIST_KEY) ?? new List<CartItem>();
        var item = wishlist.FirstOrDefault(i => i.ProductId == id);
        if (item != null)
        {
            wishlist.Remove(item);
            HttpContext.Session.SetSession(WISHLIST_KEY, wishlist);
        }
        return RedirectToAction(nameof(Index));
    }
}
