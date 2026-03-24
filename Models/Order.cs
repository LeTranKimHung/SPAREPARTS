using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SparePartsManagement.Models;

public class Order
{
    [Key]
    public int OrderId { get; set; }

    [Required]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string Phone { get; set; } = string.Empty;

    [Required]
    public string ShippingAddress { get; set; } = string.Empty;

    [Display(Name = "Ngày đặt hàng")]
    public DateTime OrderDate { get; set; } = DateTime.Now;

    [Display(Name = "Tổng cộng")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    // Shipping status logic (Theo dõi đơn hàng)
    // 0: Chờ xác nhận, 1: Đang chuẩn bị, 2: Đang giao, 3: Đã giao, 4: Đã hủy
    public int Status { get; set; } = 0;

    public string? StatusTracking { get; set; } // Tracking number if any

    public virtual ICollection<OrderItem>? OrderItems { get; set; }
}

public class OrderItem
{
    [Key]
    public int OrderItemId { get; set; }

    public int OrderId { get; set; }
    [ForeignKey("OrderId")]
    public virtual Order? Order { get; set; }

    public int ProductId { get; set; }
    [ForeignKey("ProductId")]
    public virtual Product? Product { get; set; }

    public int Quantity { get; set; }

    public decimal PriceAtOrder { get; set; } // Price might change later, frozen here
}
