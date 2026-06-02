using BaiTapWeb_Buoi5.Models;
using BaiTapWeb_Buoi5.Repositories;
using BaiTapWeb_Buoi5.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BaiTapWeb_Buoi5.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class SliderController : Controller
{
    private readonly ISliderRepository _sliderRepository;
    private readonly IWebHostEnvironment _environment;

    public SliderController(ISliderRepository sliderRepository, IWebHostEnvironment environment)
    {
        _sliderRepository = sliderRepository;
        _environment = environment;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _sliderRepository.GetAllAsync());
    }

    public IActionResult Create()
    {
        return View(new SliderFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SliderFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var imagePath = await SaveImageAsync(model.ImageFile!);
        await _sliderRepository.AddAsync(new SliderImage
        {
            ImageUrl = imagePath,
            DisplayOrder = model.DisplayOrder
        });

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var sliderImage = await _sliderRepository.GetByIdAsync(id);
        return sliderImage is null ? NotFound() : View(sliderImage);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var sliderImage = await _sliderRepository.GetByIdAsync(id);
        if (sliderImage is not null)
        {
            DeleteImageFile(sliderImage.ImageUrl);
        }

        await _sliderRepository.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateOrder(Dictionary<int, int> displayOrders)
    {
        await _sliderRepository.UpdateDisplayOrdersAsync(displayOrders);
        return RedirectToAction(nameof(Index));
    }

    private async Task<string> SaveImageAsync(IFormFile imageFile)
    {
        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "sliders");
        Directory.CreateDirectory(uploadsFolder);

        var safeFileName = Path.GetFileName(imageFile.FileName);
        var fileName = $"{Guid.NewGuid()}_{safeFileName}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        await using var stream = System.IO.File.Create(filePath);
        await imageFile.CopyToAsync(stream);

        return Path.Combine("uploads", "sliders", fileName).Replace("\\", "/");
    }

    private void DeleteImageFile(string imageUrl)
    {
        var normalizedPath = imageUrl.Replace("/", Path.DirectorySeparatorChar.ToString());
        var filePath = Path.Combine(_environment.WebRootPath, normalizedPath);

        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }
    }
}
