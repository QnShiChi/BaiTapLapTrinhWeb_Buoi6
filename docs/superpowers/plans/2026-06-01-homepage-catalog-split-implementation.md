# Homepage Catalog Split And Slider Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Tách `Home/Index` thành homepage landing page riêng, chuyển `Products/Index` thành catalog giống mockup, và giữ module admin `Slider` để cấp ảnh hero từ backend.

**Architecture:** Phần dữ liệu slider tiếp tục dùng entity `SliderImage`, repository riêng và admin CRUD riêng. Homepage dùng `HomeController` + `HomePageViewModel` để render landing page nhiều section, còn `ProductsController` trở về vai trò catalog/controller cho danh sách sản phẩm và lọc theo danh mục bằng một `ProductListViewModel` mở rộng. Layout storefront được giữ chung nhưng homepage và catalog có view/cấu trúc tách bạch.

**Tech Stack:** ASP.NET Core MVC, Razor Views, Entity Framework Core, SQL Server, xUnit, Moq

---

## File Structure

- Modify: `Controllers/HomeController.cs`
- Modify: `Controllers/ProductsController.cs`
- Modify: `ViewModels/ProductListViewModel.cs`
- Create: `ViewModels/HomePageViewModel.cs`
- Modify: `Views/Home/Index.cshtml`
- Modify: `Views/Products/Index.cshtml`
- Modify: `Views/Products/Category.cshtml`
- Modify: `Views/Shared/_Layout.cshtml`
- Modify: `Views/Shared/_LoginPartial.cshtml`
- Modify: `wwwroot/css/site.css`
- Modify: `Areas/Admin/Views/Shared/_AdminLayout.cshtml`
- Create/Modify: `Areas/Admin/Controllers/SliderController.cs`
- Create/Modify: `Areas/Admin/Views/Slider/Index.cshtml`
- Create/Modify: `Areas/Admin/Views/Slider/Create.cshtml`
- Create/Modify: `Areas/Admin/Views/Slider/Delete.cshtml`
- Modify: `Models/ApplicationDbContext.cs`
- Modify: `Program.cs`
- Create/Modify: `Models/SliderImage.cs`
- Create/Modify: `Repositories/ISliderRepository.cs`
- Create/Modify: `Repositories/SliderRepository.cs`
- Create/Modify: `tests/BaiTapWeb_Buoi5.Tests/SliderRepositoryTests.cs`
- Create: `tests/BaiTapWeb_Buoi5.Tests/HomeControllerTests.cs`
- Modify: `tests/BaiTapWeb_Buoi5.Tests/ProductsControllerTests.cs`

### Task 1: Lock homepage into `Home/Index`

**Files:**
- Create: `tests/BaiTapWeb_Buoi5.Tests/HomeControllerTests.cs`
- Modify: `Controllers/HomeController.cs`
- Create: `ViewModels/HomePageViewModel.cs`
- Modify: `Views/Home/Index.cshtml`

- [ ] **Step 1: Write the failing homepage controller tests**

Create `tests/BaiTapWeb_Buoi5.Tests/HomeControllerTests.cs`:

```csharp
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
```

- [ ] **Step 2: Run tests to verify they fail**

Run:

```bash
dotnet test tests/BaiTapWeb_Buoi5.Tests/BaiTapWeb_Buoi5.Tests.csproj --filter HomeControllerTests -v minimal
```

Expected:

```text
FAIL ... 'HomeController' does not contain a constructor that takes 3 arguments
```

- [ ] **Step 3: Implement homepage controller and model**

Create `ViewModels/HomePageViewModel.cs`:

```csharp
using BaiTapWeb_Buoi5.Models;

namespace BaiTapWeb_Buoi5.ViewModels;

public class HomePageViewModel
{
    public IEnumerable<SliderImage> SliderImages { get; set; } = [];
    public IEnumerable<Category> Categories { get; set; } = [];
    public IEnumerable<Product> LatestProducts { get; set; } = [];
    public string PageTitle { get; set; } = string.Empty;
}
```

Replace `Controllers/HomeController.cs` with:

```csharp
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
```

- [ ] **Step 4: Replace `Views/Home/Index.cshtml` with landing homepage**

Replace `Views/Home/Index.cshtml` with:

```cshtml
@model BaiTapWeb_Buoi5.ViewModels.HomePageViewModel
@{
    ViewData["Title"] = "Trang chủ";
    var sliderImages = Model.SliderImages.ToList();
}

<section class="home-page">
    <section class="hero-carousel">
        <div class="hero-copy-panel">
            <p class="eyebrow">Lumina Retail</p>
            <h1>@Model.PageTitle</h1>
            <p class="hero-text">Khám phá những lựa chọn được tuyển chọn kỹ lưỡng cho công nghệ, phong cách và trải nghiệm sống tinh gọn.</p>
            @if (Model.Categories.Any())
            {
                <a asp-controller="Products" asp-action="Index" class="button-primary hero-cta">Mua sắm ngay</a>
            }
        </div>

        <div class="hero-slider-stage" data-hero-slider>
            @if (sliderImages.Count == 0)
            {
                <div class="hero-slide hero-slide-empty">
                    <div class="hero-empty-state">
                        <p class="eyebrow">Slider</p>
                        <h2>Chưa có ảnh slider</h2>
                        <p>Admin có thể thêm ảnh tại khu vực quản lý slider để hoàn thiện carousel.</p>
                    </div>
                </div>
            }
            else
            {
                <div class="hero-slider-track">
                    @for (var i = 0; i < sliderImages.Count; i++)
                    {
                        var slide = sliderImages[i];
                        <div class="hero-slide @(i == 0 ? "is-active" : string.Empty)">
                            <img src="@Url.Content($"~/{slide.ImageUrl}")" alt="Slider @(i + 1)" class="hero-slide-image" />
                        </div>
                    }
                </div>
                <div class="hero-slider-dots" aria-hidden="true">
                    @for (var i = 0; i < sliderImages.Count; i++)
                    {
                        <span class="hero-dot @(i == 0 ? "is-active" : string.Empty)"></span>
                    }
                </div>
            }
        </div>
    </section>

    <section class="benefits-grid">
        <article class="benefit-card"><div class="benefit-icon">01</div><h3>Giao hàng nhanh</h3><p>Trải nghiệm đóng gói chỉn chu và giao nhận đúng hẹn cho từng đơn hàng.</p></article>
        <article class="benefit-card"><div class="benefit-icon">02</div><h3>Bảo hành chính hãng</h3><p>Cam kết minh bạch về nguồn gốc và hỗ trợ sau mua rõ ràng.</p></article>
        <article class="benefit-card"><div class="benefit-icon">03</div><h3>Hỗ trợ 24/7</h3><p>Đội ngũ luôn sẵn sàng tư vấn khi bạn cần chọn mua hoặc xử lý sự cố.</p></article>
    </section>

    <section class="section-stack">
        <div class="section-header">
            <div>
                <p class="eyebrow">Danh mục nổi bật</p>
                <h1>Khám phá theo nhu cầu</h1>
            </div>
        </div>
        <div class="category-feature-grid">
            @foreach (var category in Model.Categories.Take(4))
            {
                var categoryProduct = Model.LatestProducts.FirstOrDefault(product => product.CategoryId == category.Id);
                var categoryImage = categoryProduct?.ProductImages.FirstOrDefault(image => image.IsPrimary)?.ImageUrl
                    ?? categoryProduct?.ProductImages.FirstOrDefault()?.ImageUrl
                    ?? "https://placehold.co/900x900/f0edef/1b1b1d?text=QLBanHang";
                <a asp-controller="Products" asp-action="Category" asp-route-categoryId="@category.Id" class="category-feature-card">
                    <img src="@((categoryImage.StartsWith("http", StringComparison.OrdinalIgnoreCase)) ? categoryImage : Url.Content($"~/{categoryImage}"))" alt="@category.Name" class="category-feature-image" />
                    <div class="category-feature-overlay">
                        <p>@category.Name</p>
                        <span>Xem ngay</span>
                    </div>
                </a>
            }
        </div>
    </section>

    <section class="section-stack">
        <div class="section-header">
            <div>
                <p class="eyebrow">Sản phẩm mới</p>
                <h1>Lựa chọn mới nhất</h1>
            </div>
        </div>
        <div class="product-grid product-grid-home">
            @foreach (var product in Model.LatestProducts)
            {
                var primaryImage = product.ProductImages.FirstOrDefault(image => image.IsPrimary)?.ImageUrl
                    ?? product.ProductImages.FirstOrDefault()?.ImageUrl
                    ?? "https://placehold.co/600x600/f0edef/1b1b1d?text=QLBanHang";
                <article class="product-card product-card-home">
                    <span class="product-badge">New</span>
                    <img src="@((primaryImage.StartsWith("http", StringComparison.OrdinalIgnoreCase)) ? primaryImage : Url.Content($"~/{primaryImage}"))" alt="@product.Name" class="product-card-image" />
                    <div class="product-card-body">
                        <p class="product-category">@product.Category?.Name</p>
                        <h3>@product.Name</h3>
                        <p class="product-description-preview">@product.Description</p>
                        <div class="product-card-footer">
                            <p class="product-price">@product.Price.ToString("N0") VND</p>
                            <a asp-controller="Products" asp-action="Details" asp-route-id="@product.Id" class="button-secondary">Chi tiết</a>
                        </div>
                    </div>
                </article>
            }
        </div>
    </section>

    <section class="promo-banner">
        <div class="promo-copy">
            <p class="eyebrow">Ưu đãi đặc biệt</p>
            <h1>Giảm giá cho những lựa chọn được yêu thích</h1>
            <p class="hero-text">Không gian mua sắm mới được tổ chức để nổi bật hình ảnh sản phẩm, nhịp điệu nội dung và các điểm chạm thương mại quan trọng.</p>
        </div>
    </section>

    <section class="newsletter-card">
        <div>
            <p class="eyebrow">Newsletter</p>
            <h1>Đăng ký nhận tin</h1>
            <p class="hero-text">Nhận thông tin về bộ sưu tập mới, ưu đãi theo mùa và những lựa chọn nổi bật được tuyển chọn mỗi tuần.</p>
        </div>
        <form class="newsletter-form">
            <input type="email" class="form-control" placeholder="Địa chỉ email của bạn" />
            <button type="button" class="button-primary">Đăng ký</button>
        </form>
    </section>
</section>

@section Scripts {
    <script>
        (() => {
            const slider = document.querySelector('[data-hero-slider]');
            if (!slider) return;
            const slides = Array.from(slider.querySelectorAll('.hero-slide'));
            const dots = Array.from(slider.querySelectorAll('.hero-dot'));
            if (slides.length <= 1) return;

            let currentIndex = 0;
            const activate = (index) => {
                slides.forEach((slide, i) => slide.classList.toggle('is-active', i === index));
                dots.forEach((dot, i) => dot.classList.toggle('is-active', i === index));
            };

            window.setInterval(() => {
                currentIndex = (currentIndex + 1) % slides.length;
                activate(currentIndex);
            }, 4500);
        })();
    </script>
}
```

- [ ] **Step 5: Run tests to verify green**

Run:

```bash
dotnet test tests/BaiTapWeb_Buoi5.Tests/BaiTapWeb_Buoi5.Tests.csproj --filter HomeControllerTests -v minimal
```

Expected:

```text
Passed! 1 test passed.
```

- [ ] **Step 6: Commit**

```bash
git add tests/BaiTapWeb_Buoi5.Tests/HomeControllerTests.cs Controllers/HomeController.cs ViewModels/HomePageViewModel.cs Views/Home/Index.cshtml
git commit -m "feat: move landing page to home"
```

### Task 2: Return `Products/Index` to a true catalog page

**Files:**
- Modify: `Controllers/ProductsController.cs`
- Modify: `ViewModels/ProductListViewModel.cs`
- Modify: `Views/Products/Index.cshtml`
- Modify: `Views/Products/Category.cshtml`
- Modify: `tests/BaiTapWeb_Buoi5.Tests/ProductsControllerTests.cs`

- [ ] **Step 1: Rewrite the failing product catalog tests**

Replace `tests/BaiTapWeb_Buoi5.Tests/ProductsControllerTests.cs` with:

```csharp
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
        sliderRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<SliderImage>());

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
```

- [ ] **Step 2: Run tests to verify they fail**

Run:

```bash
dotnet test tests/BaiTapWeb_Buoi5.Tests/BaiTapWeb_Buoi5.Tests.csproj --filter ProductsControllerTests -v minimal
```

Expected:

```text
FAIL ... Assert.IsType() Failure because HomePageViewModel was returned
```

- [ ] **Step 3: Update catalog controller and view model**

Update `ViewModels/ProductListViewModel.cs`:

```csharp
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
```

Replace `Controllers/ProductsController.cs` with:

```csharp
using BaiTapWeb_Buoi5.Repositories;
using BaiTapWeb_Buoi5.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BaiTapWeb_Buoi5.Controllers;

public class ProductsController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ISliderRepository _sliderRepository;

    public ProductsController(
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
```

- [ ] **Step 4: Replace `Views/Products/Index.cshtml` with catalog layout**

Replace `Views/Products/Index.cshtml` with:

```cshtml
@model ProductListViewModel
@{
    ViewData["Title"] = Model.PageTitle;
}

<section class="catalog-hero">
    <div class="catalog-hero-copy">
        <p class="eyebrow">Lumina Retail</p>
        <h1>@(Model.IntroTitle ?? Model.PageTitle)</h1>
        <p class="hero-text">@(string.IsNullOrWhiteSpace(Model.IntroText) ? "Không gian tuyển chọn dành cho những sản phẩm công nghệ, phong cách và nội thất có tính thẩm mỹ cao." : Model.IntroText)</p>
        <div class="catalog-hero-actions">
            <a asp-controller="Products" asp-action="Index" class="button-primary">Mua sắm ngay</a>
            <a asp-controller="Category" asp-action="Index" class="button-secondary">Xem bộ sưu tập</a>
        </div>
    </div>
</section>

<section class="catalog-layout catalog-layout-premium">
    <aside class="catalog-sidebar premium-sidebar">
        <h2>Categories</h2>
        <a class="category-pill @(Model.SelectedCategoryId is null ? "active" : string.Empty)" asp-controller="Products" asp-action="Index">Tất cả</a>
        @foreach (var category in Model.Categories)
        {
            <a class="category-pill @(Model.SelectedCategoryId == category.Id ? "active" : string.Empty)"
               asp-controller="Products"
               asp-action="Category"
               asp-route-categoryId="@category.Id">
                @category.Name
            </a>
        }

        <div class="price-filter-card">
            <p class="eyebrow">Price range</p>
            <input type="range" min="0" max="20000000" value="10000000" class="catalog-range" />
            <div class="price-range-meta">
                <span>0đ</span>
                <span>20.000.000đ+</span>
            </div>
        </div>
    </aside>

    <section class="catalog-content">
        <div class="catalog-toolbar">
            <p class="section-subtitle">Hiển thị @Model.Products.Count() sản phẩm</p>
            <div class="catalog-view-actions">
                <button type="button" class="catalog-view-button is-active">▦</button>
                <button type="button" class="catalog-view-button">☰</button>
            </div>
        </div>

        <div class="product-grid catalog-product-grid">
            @foreach (var product in Model.Products)
            {
                var primaryImage = product.ProductImages.FirstOrDefault(image => image.IsPrimary)?.ImageUrl
                    ?? product.ProductImages.FirstOrDefault()?.ImageUrl
                    ?? "https://placehold.co/600x600/f0edef/1b1b1d?text=QLBanHang";

                <article class="product-card product-card-home">
                    <span class="product-badge">New</span>
                    <img src="@((primaryImage.StartsWith("http", StringComparison.OrdinalIgnoreCase)) ? primaryImage : Url.Content($"~/{primaryImage}"))" alt="@product.Name" class="product-card-image" />
                    <div class="product-card-body">
                        <p class="product-category">@product.Category?.Name</p>
                        <h3>@product.Name</h3>
                        <p class="product-description-preview">@product.Description</p>
                        <div class="product-card-footer">
                            <p class="product-price">@product.Price.ToString("N0") VND</p>
                            <a asp-controller="Products" asp-action="Details" asp-route-id="@product.Id" class="button-primary">Chi tiết</a>
                        </div>
                    </div>
                </article>
            }
        </div>

        <div class="catalog-pagination" aria-label="Pagination">
            <span class="catalog-page-button">1</span>
            <span class="catalog-page-button is-active">2</span>
            <span class="catalog-page-button">3</span>
            <span class="catalog-page-button">›</span>
        </div>
    </section>
</section>
```

- [ ] **Step 5: Run tests to verify green**

Run:

```bash
dotnet test tests/BaiTapWeb_Buoi5.Tests/BaiTapWeb_Buoi5.Tests.csproj --filter ProductsControllerTests -v minimal
```

Expected:

```text
Passed! 2 tests passed.
```

- [ ] **Step 6: Commit**

```bash
git add tests/BaiTapWeb_Buoi5.Tests/ProductsControllerTests.cs Controllers/ProductsController.cs ViewModels/ProductListViewModel.cs Views/Products/Index.cshtml
git commit -m "feat: redesign products as catalog"
```

### Task 3: Add shared storefront styling for homepage and product catalog

**Files:**
- Modify: `wwwroot/css/site.css`
- Modify: `Views/Shared/_Layout.cshtml`
- Modify: `Views/Shared/_LoginPartial.cshtml`

- [ ] **Step 1: Update shared layout for premium storefront shell**

Replace the main nav/search/footer block in `Views/Shared/_Layout.cshtml` with:

```cshtml
<header class="top-nav">
    <div class="container-shell nav-shell">
        <a class="brand-link" asp-area="" asp-controller="Home" asp-action="Index">QLBanHang</a>
        <nav class="primary-nav">
            <a asp-area="" asp-controller="Home" asp-action="Index">Trang chủ</a>
            <a asp-area="" asp-controller="Products" asp-action="Index">Sản phẩm</a>
            <a asp-area="" asp-controller="Category" asp-action="Index">Danh mục</a>
            <a asp-area="" asp-controller="Home" asp-action="Privacy">Privacy</a>
        </nav>
        <div class="nav-utilities">
            <form class="nav-search" asp-area="" asp-controller="Products" asp-action="Index" method="get">
                <input type="search" name="q" placeholder="Tìm kiếm sản phẩm..." />
            </form>
            <partial name="_LoginPartial" />
        </div>
    </div>
</header>

<main role="main" class="container-shell page-shell">
    @RenderBody()
</main>

<footer class="site-footer">
    <div class="container-shell footer-shell footer-shell-rich">
        <div class="footer-brand-block">
            <a class="brand-link" asp-area="" asp-controller="Home" asp-action="Index">QLBanHang</a>
            <p>Không gian mua sắm chọn lọc cho công nghệ, phong cách và trải nghiệm sống hiện đại.</p>
        </div>
        <div class="footer-links">
            <a asp-area="" asp-controller="Home" asp-action="Index">Trang chủ</a>
            <a asp-area="" asp-controller="Products" asp-action="Index">Sản phẩm</a>
            <a asp-area="" asp-controller="Category" asp-action="Index">Danh mục</a>
            <a asp-area="" asp-controller="Home" asp-action="Privacy">Privacy</a>
        </div>
        <div class="footer-meta">
            <span>&copy; 2026 - QLBanHang</span>
        </div>
    </div>
</footer>
```

- [ ] **Step 2: Keep auth links lightweight**

Use this `Views/Shared/_LoginPartial.cshtml` structure:

```cshtml
<div class="nav-auth-links">
@if (SignInManager.IsSignedIn(User))
{
    <span class="user-chip">@User.Identity?.Name</span>
    @if (User.IsInRole("Admin"))
    {
        <a asp-area="Admin" asp-controller="Product" asp-action="Index">Admin</a>
    }
    <form asp-area="Identity" asp-page="/Account/Logout" asp-route-returnUrl="@Url.Action("Index", "Home", new { area = "" })" method="post">
        <button type="submit" class="button-link">Đăng xuất</button>
    </form>
}
else
{
    <a asp-area="Identity" asp-page="/Account/Login">Đăng nhập</a>
    <a asp-area="Identity" asp-page="/Account/Register">Đăng ký</a>
}
</div>
```

- [ ] **Step 3: Append catalog-specific CSS**

Append to `wwwroot/css/site.css`:

```css
.catalog-hero {
  padding: 56px 48px;
  border-radius: 24px;
  background: linear-gradient(135deg, rgba(255, 255, 255, 0.92), rgba(255, 178, 182, 0.18));
  border: 1px solid #E8E8ED;
}

.catalog-hero-copy {
  width: min(100%, 720px);
  display: grid;
  gap: 18px;
  margin: 0 auto;
  text-align: center;
}

.catalog-hero-actions {
  display: flex;
  justify-content: center;
  gap: 16px;
  flex-wrap: wrap;
}

.catalog-layout-premium {
  grid-template-columns: 260px minmax(0, 1fr);
  align-items: start;
}

.premium-sidebar {
  display: grid;
  gap: 24px;
}

.price-filter-card {
  display: grid;
  gap: 12px;
  padding-top: 16px;
  border-top: 1px solid #E8E8ED;
}

.catalog-range {
  width: 100%;
  accent-color: #ba0036;
}

.price-range-meta {
  display: flex;
  justify-content: space-between;
  color: var(--color-muted);
  font-size: 0.9rem;
}

.catalog-toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
}

.catalog-view-actions {
  display: flex;
  gap: 10px;
}

.catalog-view-button,
.catalog-page-button {
  width: 40px;
  height: 40px;
  display: grid;
  place-items: center;
  border: 1px solid #E8E8ED;
  border-radius: 12px;
  background: #fff;
  color: var(--color-ink);
}

.catalog-view-button.is-active,
.catalog-page-button.is-active {
  background: #ba0036;
  color: #fff;
  border-color: #ba0036;
}

.catalog-product-grid {
  grid-template-columns: repeat(3, minmax(0, 1fr));
}

.catalog-pagination {
  display: flex;
  justify-content: center;
  gap: 10px;
  margin-top: 24px;
}

@media (max-width: 991.98px) {
  .catalog-layout-premium,
  .catalog-product-grid {
    grid-template-columns: 1fr;
  }
}
```

- [ ] **Step 4: Run full tests and build**

Run:

```bash
dotnet test tests/BaiTapWeb_Buoi5.Tests/BaiTapWeb_Buoi5.Tests.csproj -v minimal
dotnet build
```

Expected:

```text
Passed! All tests passed.
Build succeeded.
```

- [ ] **Step 5: Commit**

```bash
git add Views/Shared/_Layout.cshtml Views/Shared/_LoginPartial.cshtml wwwroot/css/site.css Views/Products/Category.cshtml
git commit -m "style: split homepage and product catalog"
```

### Task 4: Keep slider admin integration and verify the complete flow

**Files:**
- Modify as needed: `Areas/Admin/Controllers/SliderController.cs`
- Modify as needed: `Areas/Admin/Views/Slider/*`
- Modify as needed: `Areas/Admin/Views/Shared/_AdminLayout.cshtml`

- [ ] **Step 1: Ensure admin slider module still exists after route split**

Verify these files exist and compile:

```text
Areas/Admin/Controllers/SliderController.cs
Areas/Admin/Views/Slider/Index.cshtml
Areas/Admin/Views/Slider/Create.cshtml
Areas/Admin/Views/Slider/Delete.cshtml
Areas/Admin/Views/Shared/_AdminLayout.cshtml
```

- [ ] **Step 2: Run migration and database update if not already applied**

Run:

```bash
export PATH="$PATH:/home/phan-duong-quoc-nhat/.dotnet/tools"
export DOTNET_ROOT=/snap/dotnet-sdk/256
dotnet ef migrations add AddSliderImages
dotnet ef database update
```

Expected:

```text
Done.
```

If migration already exists, skip creation and run only `dotnet ef database update`.

- [ ] **Step 3: Manual verification**

Run:

```bash
dotnet run
```

Verify in browser:

```text
1. `/` shows the landing homepage.
2. `/Products` shows the catalog page and no longer looks like homepage.
3. `/Products/Category/{id}` keeps the same catalog visual style.
4. `/Admin/Slider` is reachable by admin.
5. Uploading a slider image makes it appear in homepage hero.
6. If there is no slider image, homepage fallback state appears cleanly.
```

- [ ] **Step 4: Commit**

```bash
git add Areas/Admin/Controllers/SliderController.cs Areas/Admin/Views/Slider Areas/Admin/Views/Shared/_AdminLayout.cshtml Migrations
git commit -m "feat: integrate homepage slider admin"
```

## Self-Review

- Spec coverage:
  - Homepage landing page at `Home/Index` is covered by Task 1.
  - Product catalog split at `Products/Index` is covered by Task 2.
  - Shared storefront shell updates are covered by Task 3.
  - Slider admin integration is covered by Task 4.
- Placeholder scan:
  - No `TODO`, `TBD`, or vague follow-up wording remains.
- Type consistency:
  - `HomePageViewModel`, `ProductListViewModel`, and `SliderImage` are used consistently across tests, controllers, and views.
