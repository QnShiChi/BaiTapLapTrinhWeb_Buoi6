using BaiTapWeb_Buoi5.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BaiTapWeb_Buoi5.Controllers;

public class CategoryController : Controller
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryController(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _categoryRepository.GetAllAsync());
    }
}
