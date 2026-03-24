using System.ComponentModel.DataAnnotations;

namespace SparePartsManagement.Models;

public class Category
{
    [Key]
    public int CategoryId { get; set; }

    [Required]
    [Display(Name = "Tên loại phụ tùng")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    // Navigation property
    public virtual ICollection<Product>? Products { get; set; }
}
