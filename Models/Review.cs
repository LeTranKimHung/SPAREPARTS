using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SparePartsManagement.Models;

public class Review
{
    [Key]
    public int ReviewId { get; set; }

    [Required]
    public int ProductId { get; set; }
    [ForeignKey("ProductId")]
    public virtual Product? Product { get; set; }

    [Required]
    [EmailAddress]
    public string UserEmail { get; set; } = string.Empty;

    [Required]
    [Range(1, 5)]
    [Display(Name = "Số sao")]
    public int StarRating { get; set; }

    [Required]
    [Display(Name = "Bình luận")]
    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
