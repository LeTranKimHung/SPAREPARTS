using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SparePartsManagement.Models;

public class Product
{
    [Key]
    public int ProductId { get; set; }

    [Required]
    [Display(Name = "Tên phụ tùng")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Giá bán (VND)")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Display(Name = "Hình ảnh")]
    public string? ImageUrl { get; set; }

    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

    [Display(Name = "Hãng xe")]
    public int BrandId { get; set; }
    [ForeignKey("BrandId")]
    public virtual Brand? Brand { get; set; }

    [Display(Name = "Loại xe (vd: SH, Camry...)")]
    public string? CarModel { get; set; } // specific car model e.g. "Toyota Corolla Altis"

    [Display(Name = "Loại phụ tùng")]
    public int CategoryId { get; set; }
    [ForeignKey("CategoryId")]
    public virtual Category? Category { get; set; }

    [Display(Name = "Số lượng tồn kho")]
    public int StockQuantity { get; set; }

    [Display(Name = "Đánh giá sao (1-5)")]
    public double? StarRating { get; set; }

    public bool IsHot { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public virtual ICollection<Review>? Reviews { get; set; }
}
