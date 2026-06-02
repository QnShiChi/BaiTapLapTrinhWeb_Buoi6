using BaiTapWeb_Buoi5.Models;

namespace BaiTapWeb_Buoi5.ViewModels;

public class ProductListViewModel
{
    public IEnumerable<Product> Products { get; set; } = [];
    public IEnumerable<Category> Categories { get; set; } = [];
    public int? SelectedCategoryId { get; set; }
    public string PageTitle { get; set; } = string.Empty;
    public string? IntroTitle { get; set; }
    public string? IntroText { get; set; }
}
