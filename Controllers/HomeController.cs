using System.Diagnostics;
using BaiTapWeb_Buoi5.Models;
using BaiTapWeb_Buoi5.Repositories;
using BaiTapWeb_Buoi5.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BaiTapWeb_Buoi5.Controllers;

public class HomeController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ISliderRepository _sliderRepository;

    public HomeController(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        ISliderRepository sliderRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _sliderRepository = sliderRepository;
    }

    public async Task<IActionResult> Index()
    {
        var products = (await _productRepository.GetAllAsync()).ToList();

        var model = new HomePageViewModel
        {
            SliderImages = await _sliderRepository.GetAllAsync(),
            Categories = await _categoryRepository.GetAllAsync(),
            LatestProducts = products.Take(8).ToList(),
            PageTitle = "Nâng tầm phong cách sống hiện đại"
        };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult AccessDenied()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
