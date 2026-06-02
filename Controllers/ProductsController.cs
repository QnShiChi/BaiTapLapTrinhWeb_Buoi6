using BaiTapWeb_Buoi5.Repositories;
using BaiTapWeb_Buoi5.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BaiTapWeb_Buoi5.Controllers;

public class ProductsController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public ProductsController(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        ISliderRepository sliderRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<IActionResult> Index()
    {
        var model = new ProductListViewModel
        {
            Products = await _productRepository.GetAllAsync(),
            Categories = await _categoryRepository.GetAllAsync(),
            PageTitle = "Sản phẩm",
            IntroTitle = "Experience Premium Shopping",
            IntroText = "Khám phá bộ sưu tập sản phẩm tinh tế và chất lượng nhất, được tuyển chọn kỹ lưỡng dành riêng cho phong cách sống hiện đại của bạn."
        };

        return View(model);
    }

    public async Task<IActionResult> Category(int categoryId)
    {
        var categories = await _categoryRepository.GetAllAsync();
        var selectedCategory = categories.FirstOrDefault(category => category.Id == categoryId);
        if (selectedCategory is null)
        {
            return NotFound();
        }

        var model = new ProductListViewModel
        {
            Products = await _productRepository.GetByCategoryIdAsync(categoryId),
            Categories = categories,
            SelectedCategoryId = categoryId,
            PageTitle = $"Danh mục: {selectedCategory.Name}",
            IntroTitle = selectedCategory.Name,
            IntroText = selectedCategory.Description
        };

        return View("Index", model);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        var model = new ProductDetailsViewModel
        {
            Product = product,
            Categories = await _categoryRepository.GetAllAsync()
        };

        return View(model);
    }
}
