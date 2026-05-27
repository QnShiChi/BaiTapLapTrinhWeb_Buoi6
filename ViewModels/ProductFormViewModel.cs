using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace BaiTapWeb_Buoi5.ViewModels;

public class ProductFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 999999999)]
    public decimal Price { get; set; }

    [Required, StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Danh mục")]
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn danh mục.")]
    public int CategoryId { get; set; }

    [Display(Name = "Ảnh sản phẩm")]
    public List<IFormFile> ImageFiles { get; set; } = [];

    public List<string> ExistingImages { get; set; } = [];

    public List<SelectListItem> Categories { get; set; } = [];
}
