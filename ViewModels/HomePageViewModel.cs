using BaiTapWeb_Buoi5.Models;

namespace BaiTapWeb_Buoi5.ViewModels;

public class HomePageViewModel
{
    public IEnumerable<SliderImage> SliderImages { get; set; } = [];
    public IEnumerable<Category> Categories { get; set; } = [];
    public IEnumerable<Product> LatestProducts { get; set; } = [];
    public string PageTitle { get; set; } = string.Empty;
}
