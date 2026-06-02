using BaiTapWeb_Buoi5.Controllers;
using BaiTapWeb_Buoi5.Models;
using BaiTapWeb_Buoi5.Repositories;
using BaiTapWeb_Buoi5.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace BaiTapWeb_Buoi5.Tests;

public class ProductsControllerTests
{
    [Fact]
    public async Task Index_ReturnsProductListViewModelForCatalogPage()
    {
        var productRepository = new Mock<IProductRepository>();
        productRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Product>
        {
            new() { Id = 1, Name = "A", Price = 100, Description = "D", ProductImages = new List<ProductImage>() }
        });

        var categoryRepository = new Mock<ICategoryRepository>();
        categoryRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Category>
        {
            new() { Id = 5, Name = "Điện tử" }
        });

        var sliderRepository = new Mock<ISliderRepository>();

        var controller = new ProductsController(productRepository.Object, categoryRepository.Object, sliderRepository.Object);

        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ProductListViewModel>(viewResult.Model);
        Assert.Equal("Sản phẩm", model.PageTitle);
        Assert.Single(model.Products);
        Assert.Single(model.Categories);
    }

    [Fact]
    public async Task Category_ReturnsCatalogViewWithSelectedCategory()
    {
        var productRepository = new Mock<IProductRepository>();
        productRepository.Setup(repo => repo.GetByCategoryIdAsync(5)).ReturnsAsync(new List<Product>());

        var categoryRepository = new Mock<ICategoryRepository>();
        categoryRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Category>
        {
            new() { Id = 5, Name = "Điện tử" }
        });

        var sliderRepository = new Mock<ISliderRepository>();
        var controller = new ProductsController(productRepository.Object, categoryRepository.Object, sliderRepository.Object);

        var result = await controller.Category(5);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        var model = Assert.IsType<ProductListViewModel>(viewResult.Model);
        Assert.Equal(5, model.SelectedCategoryId);
    }
}
