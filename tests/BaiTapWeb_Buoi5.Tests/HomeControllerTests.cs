using BaiTapWeb_Buoi5.Controllers;
using BaiTapWeb_Buoi5.Models;
using BaiTapWeb_Buoi5.Repositories;
using BaiTapWeb_Buoi5.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace BaiTapWeb_Buoi5.Tests;

public class HomeControllerTests
{
    [Fact]
    public async Task Index_ReturnsLandingPageModelWithSliderImages()
    {
        var productRepository = new Mock<IProductRepository>();
        productRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Product>());

        var categoryRepository = new Mock<ICategoryRepository>();
        categoryRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Category>());

        var sliderRepository = new Mock<ISliderRepository>();
        sliderRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<SliderImage>
        {
            new() { ImageUrl = "uploads/sliders/hero.jpg", DisplayOrder = 1 }
        });

        var controller = new HomeController(productRepository.Object, categoryRepository.Object, sliderRepository.Object);

        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<HomePageViewModel>(viewResult.Model);
        Assert.Single(model.SliderImages);
        Assert.Equal("uploads/sliders/hero.jpg", model.SliderImages.First().ImageUrl);
    }
}
