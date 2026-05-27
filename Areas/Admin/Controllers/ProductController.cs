using BaiTapWeb_Buoi5.Models;
using BaiTapWeb_Buoi5.Repositories;
using BaiTapWeb_Buoi5.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BaiTapWeb_Buoi5.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ProductController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IWebHostEnvironment _environment;

    public ProductController(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IWebHostEnvironment environment)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _environment = environment;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _productRepository.GetAllAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        return product is null ? NotFound() : View(product);
    }

    public async Task<IActionResult> Create()
    {
        return View(new ProductFormViewModel
        {
            Categories = await GetCategoryItemsAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel model)
    {
        if (model.ImageFiles.Count == 0)
        {
            ModelState.AddModelError(nameof(model.ImageFiles), "Vui lòng chọn ít nhất một ảnh.");
        }

        if (!ModelState.IsValid)
        {
            model.Categories = await GetCategoryItemsAsync();
            return View(model);
        }

        var product = new Product
        {
            Name = model.Name,
            Price = model.Price,
            Description = model.Description,
            CategoryId = model.CategoryId
        };

        foreach (var item in model.ImageFiles.Select((file, index) => new { file, index }))
        {
            var imagePath = await SaveImageAsync(item.file);
            product.ProductImages.Add(new ProductImage
            {
                ImageUrl = imagePath,
                IsPrimary = item.index == 0
            });
        }

        await _productRepository.AddAsync(product);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        return View(new ProductFormViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Description = product.Description,
            CategoryId = product.CategoryId,
            ExistingImages = product.ProductImages.Select(image => image.ImageUrl).ToList(),
            Categories = await GetCategoryItemsAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductFormViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        var product = await _productRepository.GetByIdAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            model.Categories = await GetCategoryItemsAsync();
            model.ExistingImages = product.ProductImages.Select(image => image.ImageUrl).ToList();
            return View(model);
        }

        product.Name = model.Name;
        product.Price = model.Price;
        product.Description = model.Description;
        product.CategoryId = model.CategoryId;

        if (model.ImageFiles.Count > 0)
        {
            product.ProductImages.Clear();

            foreach (var item in model.ImageFiles.Select((file, index) => new { file, index }))
            {
                var imagePath = await SaveImageAsync(item.file);
                product.ProductImages.Add(new ProductImage
                {
                    ImageUrl = imagePath,
                    IsPrimary = item.index == 0
                });
            }
        }

        await _productRepository.UpdateAsync(product);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        return product is null ? NotFound() : View(product);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _productRepository.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItem>> GetCategoryItemsAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return categories
            .Select(category => new SelectListItem
            {
                Value = category.Id.ToString(),
                Text = category.Name
            })
            .ToList();
    }

    private async Task<string> SaveImageAsync(IFormFile imageFile)
    {
        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "products");
        Directory.CreateDirectory(uploadsFolder);

        var safeFileName = Path.GetFileName(imageFile.FileName);
        var fileName = $"{Guid.NewGuid()}_{safeFileName}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        await using var stream = System.IO.File.Create(filePath);
        await imageFile.CopyToAsync(stream);

        return Path.Combine("uploads", "products", fileName).Replace("\\", "/");
    }
}
