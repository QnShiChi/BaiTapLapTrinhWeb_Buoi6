using BaiTapWeb_Buoi5.Models;

namespace BaiTapWeb_Buoi5.ViewModels;

public class ProductDetailsViewModel
{
    public Product Product { get; set; } = default!;
    public IEnumerable<Category> Categories { get; set; } = [];
}
