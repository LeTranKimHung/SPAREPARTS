using System.ComponentModel.DataAnnotations;

namespace SparePartsManagement.Models;

public class Brand
{
    [Key]
    public int BrandId { get; set; }

    [Required]
    [Display(Name = "Tên hãng xe")]
    public string Name { get; set; } = string.Empty; // e.g. Toyota, Honda, Yamaha

    public string? Description { get; set; }

    // Navigation property
    public virtual ICollection<Product>? Products { get; set; }
}
