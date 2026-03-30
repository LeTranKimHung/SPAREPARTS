using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SparePartsManagement.Data;
using SparePartsManagement.Models;
using SparePartsManagement.ViewModels;
using SparePartsManagement.Helpers;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace SparePartsManagement.Controllers;

public class CartController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly Microsoft.Extensions.Configuration.IConfiguration _config;
    private const string CART_KEY = "CartItems";

    public CartController(ApplicationDbContext context, Microsoft.Extensions.Configuration.IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    // List all items in session-based cart
    public IActionResult Index()
    {
        var cart = HttpContext.Session.GetSession<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
        return View(cart);
    }

    [HttpPost]
    public async Task<IActionResult> Add(int id, int quantity = 1)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        var cart = HttpContext.Session.GetSession<List<CartItem>>(CART_KEY) ?? new List<CartItem>();

        var existingItem = cart.FirstOrDefault(item => item.ProductId == id);
        int requestedQuantity = (existingItem?.Quantity ?? 0) + quantity;
        
        if (requestedQuantity > product.StockQuantity)
        {
            TempData["CartError"] = "Sản phẩm trong kho không đủ";
            return RedirectToAction("Details", "PublicProduct", new { id = id });
        }

        if (existingItem != null)
        {
            existingItem.Quantity = requestedQuantity;
        }
        else
        {
            cart.Add(new CartItem
            {
                ProductId = product.ProductId,
                ProductName = product.Name,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                Quantity = quantity
            });
        }

        HttpContext.Session.SetSession(CART_KEY, cart);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Remove(int id)
    {
        var cart = HttpContext.Session.GetSession<List<CartItem>>(CART_KEY);
        if (cart != null)
        {
            var itemToRemove = cart.FirstOrDefault(i => i.ProductId == id);
            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
                HttpContext.Session.SetSession(CART_KEY, cart);
            }
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Update(int id, int quantity)
    {
        if (quantity < 1) return Remove(id);

        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        if (quantity > product.StockQuantity)
        {
            TempData["CartError"] = "Sản phẩm trong kho không đủ";
            return RedirectToAction(nameof(Index));
        }

        var cart = HttpContext.Session.GetSession<List<CartItem>>(CART_KEY);
        if (cart != null)
        {
            var item = cart.FirstOrDefault(i => i.ProductId == id);
            if (item != null)
            {
                item.Quantity = quantity;
                HttpContext.Session.SetSession(CART_KEY, cart);
            }
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize]
    public IActionResult Checkout()
    {
        var cart = HttpContext.Session.GetSession<List<CartItem>>(CART_KEY);
        if (cart == null || !cart.Any()) return RedirectToAction(nameof(Index));

        var subtotal = cart.Sum(i => i.Total);
        var discount = (decimal)(HttpContext.Session.GetSession<double?>("DiscountPercentage") ?? 0);
        var discountAmount = subtotal * (discount / 100);

        var order = new Order
        {
            TotalAmount = subtotal - discountAmount,
            Email = User.Identity?.Name ?? ""
        };
        ViewBag.Discount = discount;
        ViewBag.DiscountAmount = discountAmount;
        ViewBag.Subtotal = subtotal;
        ViewBag.ShippingFee = 0;
        return View(order);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(Order order, int shippingFee, string paymentMethod = "COD")
    {
        var cart = HttpContext.Session.GetSession<List<CartItem>>(CART_KEY);
        if (cart == null || !cart.Any()) return RedirectToAction(nameof(Index));

        if (ModelState.IsValid)
        {
            var discount = (decimal)(HttpContext.Session.GetSession<double?>("DiscountPercentage") ?? 0);
            var subtotal = cart.Sum(i => i.Total);
            var discountAmount = subtotal * (discount / 100);

            order.OrderDate = DateTime.Now;
            order.Status = 0; // Chờ xác nhận
            
            order.TotalAmount = (subtotal - discountAmount) + shippingFee;
            
            if(shippingFee > 0) {
                order.ShippingAddress = $"[Phí Ship: {shippingFee.ToString("N0")}đ] " + order.ShippingAddress;
            } else {
                order.ShippingAddress = "[Freeship HCM] " + order.ShippingAddress;
            }

            if (paymentMethod == "VNPAY")
            {
                order.ShippingAddress = "[Chờ Thanh toán VNPAY] " + order.ShippingAddress;
            }
            else
            {
                order.ShippingAddress = "[Thanh toán tiền mặt COD] " + order.ShippingAddress;
            }
            
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Lưu OrderItems & Trừ kho
            foreach (var item in cart)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    PriceAtOrder = item.Price
                };
                _context.OrderItems.Add(orderItem);

                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity -= item.Quantity;
                    if (product.StockQuantity < 0) product.StockQuantity = 0;
                }
            }

            await _context.SaveChangesAsync();

            if (paymentMethod != "VNPAY")
            {
                HttpContext.Session.Remove(CART_KEY);
                HttpContext.Session.Remove("DiscountPercentage");

                try {
                    string mailBody = $"<h3>Cảm ơn bạn đã đặt hàng!</h3><p>Mã đơn hàng: <b>#{order.OrderId}</b></p><p>Tổng tiền: <b style=\"color:red\">{order.TotalAmount:N0}đ</b></p><p>Chúng tôi sẽ sớm liên hệ và giao hàng đến bạn.</p>";
                    await SendOrderEmailAsync(order.Email, $"XÁC NHẬN ĐƠN HÀNG #{order.OrderId} - SPARE PARTS", mailBody);
                } catch { /* Bỏ qua lỗi gửi mail */ }
            }

            if (paymentMethod == "VNPAY")
            {
                var vnpay = new VnPayLibrary();
                var vnPayConfig = _config.GetSection("VnPay");
                
                vnpay.AddRequestData("vnp_Version", "2.1.0");
                vnpay.AddRequestData("vnp_Command", "pay");
                vnpay.AddRequestData("vnp_TmnCode", vnPayConfig["TmnCode"]!);
                vnpay.AddRequestData("vnp_Amount", ((long)(order.TotalAmount * 100)).ToString());
                vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
                vnpay.AddRequestData("vnp_CurrCode", "VND");
                vnpay.AddRequestData("vnp_IpAddr", "127.0.0.1"); // Vượt lỗi phân tích IPv6 của VNPAY
                vnpay.AddRequestData("vnp_Locale", "vn");
                vnpay.AddRequestData("vnp_OrderInfo", "ThanhToanDonHang" + order.OrderId); // Xoá khoảng trắng để khử sai khác Percent-Encoding
                vnpay.AddRequestData("vnp_OrderType", "other");
                
                var returnUrl = $"{Request.Scheme}://{Request.Host}/Cart/PaymentCallback";
                vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
                vnpay.AddRequestData("vnp_TxnRef", order.OrderId.ToString());

                string paymentUrl = vnpay.CreateRequestUrl(vnPayConfig["BaseUrl"]!, vnPayConfig["HashSecret"]!);
                return Redirect(paymentUrl);
            }

            return RedirectToAction("TrackOrder", new { id = order.OrderId });
        }
        
        // Repopulate ViewBag properties when validation fails
        var fallbackDiscount = (decimal)(HttpContext.Session.GetSession<double?>("DiscountPercentage") ?? 0);
        var fallbackSubtotal = cart.Sum(i => i.Total);
        var fallbackDiscountAmount = fallbackSubtotal * (fallbackDiscount / 100);
        
        ViewBag.Discount = fallbackDiscount;
        ViewBag.DiscountAmount = fallbackDiscountAmount;
        ViewBag.Subtotal = fallbackSubtotal;
        ViewBag.ShippingFee = shippingFee;

        return View(order);
    }

    [HttpGet]
    public async Task<IActionResult> PaymentCallback()
    {
        var queryObject = Request.Query;
        var vnpay = new VnPayLibrary();
        foreach (var (key, value) in queryObject)
        {
            if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
            {
                vnpay.AddResponseData(key, value.ToString());
            }
        }

        var vnPayConfig = _config.GetSection("VnPay");
        string vnp_SecureHash = Request.Query["vnp_SecureHash"].ToString();
        bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnPayConfig["HashSecret"]!);

        if (!checkSignature)
        {
            TempData["CartError"] = "Lỗi xác thực chữ ký VNPay.";
            return RedirectToAction("TrackOrder");
        }

        long orderId = Convert.ToInt64(vnpay.GetResponseData("vnp_TxnRef"));
        string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");

        var order = await _context.Orders.FindAsync((int)orderId);
        if (order != null)
        {
            if (vnp_ResponseCode == "00")
            {
                // Thanh toán thành công (Vẫn giữ trạng thái 0 để Admin duyệt, nhưng đổi log chú thích)
                order.ShippingAddress = order.ShippingAddress.Replace("[Chờ Thanh toán VNPAY]", "[ĐÃ THANH TOÁN VNPAY ONLINE]");
                TempData["CouponSuccess"] = "Thanh toán giao dịch trực tuyến VNPay thành công!";

                // Gửi mail xác nhận ngay sau khi VNPay trả về thành công
                try {
                    string mailBody = $"<h3>Cảm ơn bạn đã thanh toán thành công qua VNPAY!</h3><p>Mã đơn hàng: <b>#{order.OrderId}</b></p><p>Tổng tiền thanh toán: <b style=\"color:red\">{order.TotalAmount:N0}đ</b></p><p>Chúng tôi sẽ sớm liên hệ và giao hàng đến bạn.</p>";
                    await SendOrderEmailAsync(order.Email, $"XÁC NHẬN ĐƠN HÀNG VNPAY #{order.OrderId} - SPARE PARTS", mailBody);
                } catch { /* Bỏ qua lỗi gửi mail */ }
            }
            else
            {
                // Hủy do thanh toán thất bại
                order.Status = 3; // Hủy đơn
                order.ShippingAddress = order.ShippingAddress.Replace("[Chờ Thanh toán VNPAY]", "[GIAO DỊCH VNPAY THẤT BẠI - HỦY]");
                TempData["CartError"] = $"Giao dịch VNPay bị lỗi hoặc đã hủy (Mã lỗi: {vnp_ResponseCode}). Đơn hàng đã bị hủy.";
            }
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("TrackOrder", new { id = orderId });
    }

    [Authorize]
    public async Task<IActionResult> TrackOrder(int? id)
    {
        if (id == null)
        {
            var orders = await _context.Orders.Where(o => o.Email == User.Identity.Name).ToListAsync();
            return View(orders);
        }
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.OrderId == id);
            
        if (order == null || order.Email != User.Identity.Name) 
            return NotFound();
            
        return View("OrderDetails", order);
    }

    // Khách hủy đơn của chính mình theo luồng (chỉ cho phép khi chưa hoàn tất/hủy trước đó)
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelOrder(int id)
    {
        var userEmail = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(userEmail)) return Unauthorized();

        var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == id && o.Email == userEmail);
        if (order == null) return NotFound();

        // Status: 0=Chờ xác nhận, 1=Đang giao, 2=Đã hoàn thành, 3=Đã hủy
        if (order.Status == 0 || order.Status == 1)
        {
            order.Status = 3;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(TrackOrder), new { id });
    }

    // Khách xác nhận đã nhận hàng (chỉ khi đơn đang "đang giao")
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmReceived(int id)
    {
        var userEmail = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(userEmail)) return Unauthorized();

        var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == id && o.Email == userEmail);
        if (order == null) return NotFound();

        if (order.Status == 1)
        {
            order.Status = 2;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(TrackOrder), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> ApplyCoupon(string couponCode)
    {
        if (string.IsNullOrEmpty(couponCode))
        {
            TempData["CouponError"] = "Vui lòng nhập mã giảm giá!";
            return RedirectToAction(nameof(Index));
        }

        var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == couponCode && c.IsActive);
        
        if (coupon == null || (coupon.ExpiryDate.HasValue && coupon.ExpiryDate < DateTime.Now))
        {
            TempData["CouponError"] = "Mã giảm giá không hợp lệ hoặc đã hết hạn!";
            HttpContext.Session.Remove("DiscountPercentage");
        }
        else
        {
            HttpContext.Session.SetSession("DiscountPercentage", coupon.DiscountPercentage);
            TempData["CouponSuccess"] = $"Đã áp dụng mã {coupon.Code}. Giảm {coupon.DiscountPercentage}%!";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task SendOrderEmailAsync(string email, string subject, string body)
    {
        var emailSettings = _config.GetSection("EmailSettings");
        var client = new System.Net.Mail.SmtpClient(emailSettings["SmtpServer"], int.Parse(emailSettings["SmtpPort"]!))
        {
            EnableSsl = true,
            Credentials = new System.Net.NetworkCredential(emailSettings["SenderEmail"], emailSettings["AppPassword"])
        };
        var mailMessage = new System.Net.Mail.MailMessage
        {
            From = new System.Net.Mail.MailAddress(emailSettings["SenderEmail"]!, emailSettings["SenderName"]),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        mailMessage.To.Add(email);
        await client.SendMailAsync(mailMessage);
    }
}
