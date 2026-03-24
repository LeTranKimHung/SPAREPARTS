using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SparePartsManagement.Models;

public class Coupon
{
    [Key]
    public int CouponId { get; set; }

    [Required]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty; // e.g. "OFF10", "HELLOSPRING"

    [Required]
    [Range(0, 100)]
    [Display(Name = "Giảm giá (%)")]
    public double DiscountPercentage { get; set; }

    [Display(Name = "Giá trị đơn tối thiểu")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MinimumAmount { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Ngày hết hạn")]
    public DateTime? ExpiryDate { get; set; }

    public bool IsActive { get; set; } = true;
}
