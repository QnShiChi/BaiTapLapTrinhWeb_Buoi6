using System.ComponentModel.DataAnnotations;

namespace BaiTapWeb_Buoi5.Models;

public class ProductImage
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    [Required, StringLength(255)]
    public string ImageUrl { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public Product? Product { get; set; }
}
