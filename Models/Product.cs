using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaiTapWeb_Buoi5.Models;

public class Product
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, 999999999)]
    public decimal Price { get; set; }

    [Required, StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    public Category? Category { get; set; }

    public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
}
