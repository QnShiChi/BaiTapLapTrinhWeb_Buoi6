# Homepage Slider And Redesign Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Redesign homepage theo `Lumina Retail` và thêm module admin `Slider` riêng để quản lý ảnh carousel thật từ backend.

**Architecture:** Tách bài toán thành hai phần rõ ràng: dữ liệu slider có model/repository/controller/admin views riêng, còn homepage dùng một view model tổng hợp để render landing page mới từ slider, categories và products. Ảnh slider được upload vào `wwwroot/uploads/sliders`, metadata lưu trong SQL Server, và hero carousel chỉ đọc từ bảng `SliderImages` theo `DisplayOrder`.

**Tech Stack:** ASP.NET Core MVC, Razor Views, Entity Framework Core, SQL Server, xUnit, Moq

---

## File Structure

- Create: `Models/SliderImage.cs`
- Modify: `Models/ApplicationDbContext.cs`
- Create: `Repositories/ISliderRepository.cs`
- Create: `Repositories/SliderRepository.cs`
- Modify: `Program.cs`
- Create: `ViewModels/HomePageViewModel.cs`
- Modify: `Controllers/ProductsController.cs`
- Modify: `Views/Products/Index.cshtml`
- Modify: `wwwroot/css/site.css`
- Create: `Areas/Admin/Controllers/SliderController.cs`
- Create: `Areas/Admin/Views/Slider/Index.cshtml`
- Create: `Areas/Admin/Views/Slider/Create.cshtml`
- Create: `Areas/Admin/Views/Slider/Delete.cshtml`
- Create: `ViewModels/SliderFormViewModel.cs`
- Modify: `Areas/Admin/Views/Shared/_AdminLayout.cshtml`
- Create: `Migrations/<timestamp>_AddSliderImages.cs`
- Create: `tests/BaiTapWeb_Buoi5.Tests/SliderRepositoryTests.cs`
- Create: `tests/BaiTapWeb_Buoi5.Tests/ProductsControllerTests.cs`

### Task 1: Add slider data model and repository support

**Files:**
- Create: `Models/SliderImage.cs`
- Modify: `Models/ApplicationDbContext.cs`
- Create: `Repositories/ISliderRepository.cs`
- Create: `Repositories/SliderRepository.cs`
- Modify: `Program.cs`
- Test: `tests/BaiTapWeb_Buoi5.Tests/SliderRepositoryTests.cs`

- [ ] **Step 1: Write the failing repository tests**

Create `tests/BaiTapWeb_Buoi5.Tests/SliderRepositoryTests.cs`:

```csharp
using BaiTapWeb_Buoi5.Models;
using BaiTapWeb_Buoi5.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BaiTapWeb_Buoi5.Tests;

public class SliderRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsSliderImagesOrderedByDisplayOrder()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);
        context.SliderImages.AddRange(
            new SliderImage { ImageUrl = "uploads/sliders/b.jpg", DisplayOrder = 2 },
            new SliderImage { ImageUrl = "uploads/sliders/a.jpg", DisplayOrder = 1 });
        await context.SaveChangesAsync();

        var repository = new SliderRepository(context);

        var result = (await repository.GetAllAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("uploads/sliders/a.jpg", result[0].ImageUrl);
        Assert.Equal("uploads/sliders/b.jpg", result[1].ImageUrl);
    }

    [Fact]
    public async Task UpdateDisplayOrdersAsync_UpdatesOnlyMatchedSliderImages()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);
        var first = new SliderImage { ImageUrl = "uploads/sliders/1.jpg", DisplayOrder = 1 };
        var second = new SliderImage { ImageUrl = "uploads/sliders/2.jpg", DisplayOrder = 2 };
        context.SliderImages.AddRange(first, second);
        await context.SaveChangesAsync();

        var repository = new SliderRepository(context);

        await repository.UpdateDisplayOrdersAsync(new Dictionary<int, int>
        {
            [first.Id] = 5,
            [second.Id] = 3
        });

        var items = await context.SliderImages.OrderBy(item => item.Id).ToListAsync();

        Assert.Equal(5, items[0].DisplayOrder);
        Assert.Equal(3, items[1].DisplayOrder);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run:

```bash
dotnet test tests/BaiTapWeb_Buoi5.Tests/BaiTapWeb_Buoi5.Tests.csproj --filter SliderRepositoryTests -v minimal
```

Expected:

```text
FAIL ... The type or namespace name 'SliderImage' could not be found
```

- [ ] **Step 3: Write the slider model and repository**

Create `Models/SliderImage.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace BaiTapWeb_Buoi5.Models;

public class SliderImage
{
    public int Id { get; set; }

    [Required]
    [StringLength(300)]
    public string ImageUrl { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

Update `Models/ApplicationDbContext.cs`:

```csharp
public DbSet<SliderImage> SliderImages => Set<SliderImage>();

protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);

    builder.Entity<SliderImage>()
        .Property(item => item.CreatedAt)
        .HasDefaultValueSql("GETUTCDATE()");

    builder.Entity<SliderImage>()
        .HasIndex(item => item.DisplayOrder);

    builder.Entity<Category>()
        .HasMany(category => category.Products)
        .WithOne(product => product.Category)
        .HasForeignKey(product => product.CategoryId)
        .OnDelete(DeleteBehavior.Restrict);

    builder.Entity<Product>()
        .HasMany(product => product.ProductImages)
        .WithOne(image => image.Product)
        .HasForeignKey(image => image.ProductId)
        .OnDelete(DeleteBehavior.Cascade);
}
```

Create `Repositories/ISliderRepository.cs`:

```csharp
using BaiTapWeb_Buoi5.Models;

namespace BaiTapWeb_Buoi5.Repositories;

public interface ISliderRepository
{
    Task<IEnumerable<SliderImage>> GetAllAsync();
    Task<SliderImage?> GetByIdAsync(int id);
    Task AddAsync(SliderImage sliderImage);
    Task DeleteAsync(int id);
    Task UpdateDisplayOrdersAsync(IDictionary<int, int> displayOrders);
}
```

Create `Repositories/SliderRepository.cs`:

```csharp
using BaiTapWeb_Buoi5.Models;
using Microsoft.EntityFrameworkCore;

namespace BaiTapWeb_Buoi5.Repositories;

public class SliderRepository : ISliderRepository
{
    private readonly ApplicationDbContext _context;

    public SliderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SliderImage>> GetAllAsync()
    {
        return await _context.SliderImages
            .OrderBy(item => item.DisplayOrder)
            .ThenBy(item => item.Id)
            .ToListAsync();
    }

    public async Task<SliderImage?> GetByIdAsync(int id)
    {
        return await _context.SliderImages.FirstOrDefaultAsync(item => item.Id == id);
    }

    public async Task AddAsync(SliderImage sliderImage)
    {
        await _context.SliderImages.AddAsync(sliderImage);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var sliderImage = await _context.SliderImages.FindAsync(id);
        if (sliderImage is null)
        {
            return;
        }

        _context.SliderImages.Remove(sliderImage);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateDisplayOrdersAsync(IDictionary<int, int> displayOrders)
    {
        var items = await _context.SliderImages
            .Where(item => displayOrders.Keys.Contains(item.Id))
            .ToListAsync();

        foreach (var item in items)
        {
            item.DisplayOrder = displayOrders[item.Id];
        }

        await _context.SaveChangesAsync();
    }
}
```

Update `Program.cs` service registration:

```csharp
builder.Services.AddScoped<ISliderRepository, SliderRepository>();
```

- [ ] **Step 4: Run tests to verify they pass**

Run:

```bash
dotnet test tests/BaiTapWeb_Buoi5.Tests/BaiTapWeb_Buoi5.Tests.csproj --filter SliderRepositoryTests -v minimal
```

Expected:

```text
Passed! 2 tests passed.
```

- [ ] **Step 5: Commit**

```bash
git add Models/SliderImage.cs Models/ApplicationDbContext.cs Repositories/ISliderRepository.cs Repositories/SliderRepository.cs Program.cs tests/BaiTapWeb_Buoi5.Tests/SliderRepositoryTests.cs
git commit -m "feat: add slider image data model"
```

### Task 2: Build admin slider management

**Files:**
- Create: `ViewModels/SliderFormViewModel.cs`
- Create: `Areas/Admin/Controllers/SliderController.cs`
- Create: `Areas/Admin/Views/Slider/Index.cshtml`
- Create: `Areas/Admin/Views/Slider/Create.cshtml`
- Create: `Areas/Admin/Views/Slider/Delete.cshtml`
- Modify: `Areas/Admin/Views/Shared/_AdminLayout.cshtml`

- [ ] **Step 1: Add admin slider form model**

Create `ViewModels/SliderFormViewModel.cs`:

```csharp
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BaiTapWeb_Buoi5.ViewModels;

public class SliderFormViewModel
{
    [Required(ErrorMessage = "Vui lòng chọn ảnh slider.")]
    [Display(Name = "Ảnh slider")]
    public IFormFile? ImageFile { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Thứ tự hiển thị phải từ 0 trở lên.")]
    [Display(Name = "Thứ tự hiển thị")]
    public int DisplayOrder { get; set; }
}
```

- [ ] **Step 2: Add admin slider controller**

Create `Areas/Admin/Controllers/SliderController.cs`:

```csharp
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
```

- [ ] **Step 3: Add admin slider views**

Create `Areas/Admin/Views/Slider/Index.cshtml`:

```cshtml
@model IEnumerable<BaiTapWeb_Buoi5.Models.SliderImage>
@{
    ViewData["Title"] = "Slider";
}

<section class="admin-page">
    <div class="section-header">
        <div>
            <p class="eyebrow">Admin</p>
            <h1>Quản lý slider</h1>
        </div>
        <a asp-action="Create" class="button-primary">Thêm ảnh</a>
    </div>

    <form asp-action="UpdateOrder" method="post">
        @Html.AntiForgeryToken()
        <table class="admin-table">
            <thead>
                <tr>
                    <th>Ảnh</th>
                    <th>Đường dẫn</th>
                    <th>Thứ tự</th>
                    <th>Hành động</th>
                </tr>
            </thead>
            <tbody>
            @foreach (var item in Model)
            {
                <tr>
                    <td><img src="@Url.Content($"~/{item.ImageUrl}")" alt="Slider @item.Id" style="width:180px;border-radius:12px;" /></td>
                    <td>@item.ImageUrl</td>
                    <td><input type="number" name="displayOrders[@item.Id]" value="@item.DisplayOrder" class="form-control" /></td>
                    <td class="admin-actions">
                        <a asp-action="Delete" asp-route-id="@item.Id" class="button-danger">Xóa</a>
                    </td>
                </tr>
            }
            </tbody>
        </table>

        <button type="submit" class="button-secondary">Cập nhật thứ tự</button>
    </form>
</section>
```

Create `Areas/Admin/Views/Slider/Create.cshtml`:

```cshtml
@model BaiTapWeb_Buoi5.ViewModels.SliderFormViewModel
@{
    ViewData["Title"] = "Thêm ảnh slider";
}

<section class="form-page">
    <div class="section-header">
        <div>
            <p class="eyebrow">Admin</p>
            <h1>Thêm ảnh slider</h1>
        </div>
    </div>

    <div class="form-card">
        <form asp-action="Create" method="post" enctype="multipart/form-data">
            @Html.AntiForgeryToken()
            <div asp-validation-summary="ModelOnly" class="text-danger"></div>
            <div class="form-group">
                <label asp-for="ImageFile"></label>
                <input asp-for="ImageFile" class="form-control" type="file" accept="image/*" />
                <span asp-validation-for="ImageFile" class="text-danger"></span>
            </div>
            <div class="form-group">
                <label asp-for="DisplayOrder"></label>
                <input asp-for="DisplayOrder" class="form-control" />
                <span asp-validation-for="DisplayOrder" class="text-danger"></span>
            </div>
            <button type="submit" class="button-primary">Lưu ảnh</button>
        </form>
    </div>
</section>
```

Create `Areas/Admin/Views/Slider/Delete.cshtml`:

```cshtml
@model BaiTapWeb_Buoi5.Models.SliderImage
@{
    ViewData["Title"] = "Xóa ảnh slider";
}

<section class="detail-page">
    <div class="detail-card danger-card">
        <p class="eyebrow">Admin</p>
        <h1>Xóa ảnh slider</h1>
        <p>Bạn có chắc muốn xóa ảnh slider này không?</p>
        <img src="@Url.Content($"~/{Model.ImageUrl}")" alt="Slider @Model.Id" style="width:min(100%,420px);border-radius:16px;" />
        <form asp-action="Delete" method="post">
            @Html.AntiForgeryToken()
            <input type="hidden" asp-for="Id" />
            <button type="submit" class="button-danger">Xóa</button>
        </form>
    </div>
</section>
```

- [ ] **Step 4: Add slider link to admin navigation**

Update `Areas/Admin/Views/Shared/_AdminLayout.cshtml` nav:

```cshtml
<nav class="primary-nav">
    <a asp-area="Admin" asp-controller="Product" asp-action="Index">Sản phẩm</a>
    <a asp-area="Admin" asp-controller="Category" asp-action="Index">Danh mục</a>
    <a asp-area="Admin" asp-controller="Slider" asp-action="Index">Slider</a>
    <a asp-area="" asp-controller="Products" asp-action="Index">Trang người dùng</a>
</nav>
```

- [ ] **Step 5: Commit**

```bash
git add ViewModels/SliderFormViewModel.cs Areas/Admin/Controllers/SliderController.cs Areas/Admin/Views/Slider Areas/Admin/Views/Shared/_AdminLayout.cshtml
git commit -m "feat: add admin slider management"
```

### Task 3: Load slider data into homepage and replace storefront layout

**Files:**
- Create: `ViewModels/HomePageViewModel.cs`
- Modify: `Controllers/ProductsController.cs`
- Modify: `Views/Products/Index.cshtml`
- Modify: `wwwroot/css/site.css`
- Test: `tests/BaiTapWeb_Buoi5.Tests/ProductsControllerTests.cs`

- [ ] **Step 1: Write the failing homepage controller tests**

Create `tests/BaiTapWeb_Buoi5.Tests/ProductsControllerTests.cs`:

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
    public async Task Index_ReturnsHomePageViewModelWithSliderImages()
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

        var controller = new ProductsController(productRepository.Object, categoryRepository.Object, sliderRepository.Object);

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
dotnet test tests/BaiTapWeb_Buoi5.Tests/BaiTapWeb_Buoi5.Tests.csproj --filter ProductsControllerTests -v minimal
```

Expected:

```text
FAIL ... The type or namespace name 'HomePageViewModel' could not be found
```

- [ ] **Step 3: Add homepage view model and controller integration**

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

Update constructor and `Index()` in `Controllers/ProductsController.cs`:

```csharp
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
    var products = (await _productRepository.GetAllAsync()).ToList();

    var model = new HomePageViewModel
    {
        SliderImages = await _sliderRepository.GetAllAsync(),
        Categories = await _categoryRepository.GetAllAsync(),
        LatestProducts = products.Take(8).ToList(),
        PageTitle = "Khám phá phong cách sống hiện đại"
    };

    return View(model);
}
```

- [ ] **Step 4: Replace homepage Razor view with landing page composition**

Replace `Views/Products/Index.cshtml` with:

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
            <a asp-controller="Products" asp-action="Category" asp-route-categoryId="@(Model.Categories.FirstOrDefault()?.Id)" class="button-primary">Mua sắm ngay</a>
        </div>

        <div class="hero-slider-stage">
            @if (sliderImages.Count == 0)
            {
                <div class="hero-slide hero-slide-empty">
                    <div class="hero-empty-state">
                        <p class="eyebrow">Slider</p>
                        <h2>Chưa có ảnh slider</h2>
                        <p>Admin có thể thêm ảnh tại khu vực quản lý slider.</p>
                    </div>
                </div>
            }
            else
            {
                @for (var i = 0; i < sliderImages.Count; i++)
                {
                    var slide = sliderImages[i];
                    <div class="hero-slide @(i == 0 ? "is-active" : string.Empty)">
                        <img src="@Url.Content($"~/{slide.ImageUrl}")" alt="Slider @(i + 1)" class="hero-slide-image" />
                    </div>
                }
            }
        </div>
    </section>

    <section class="benefits-grid">
        <article class="benefit-card"><h3>Giao hàng nhanh</h3><p>Nhận hàng đúng hẹn với trải nghiệm đóng gói chỉn chu.</p></article>
        <article class="benefit-card"><h3>Bảo hành chính hãng</h3><p>Cam kết sản phẩm chính hãng và hỗ trợ sau mua rõ ràng.</p></article>
        <article class="benefit-card"><h3>Hỗ trợ 24/7</h3><p>Đội ngũ sẵn sàng hỗ trợ khi bạn cần tư vấn hoặc xử lý sự cố.</p></article>
    </section>

    <section class="section-stack">
        <div class="section-header">
            <div>
                <p class="eyebrow">Danh mục nổi bật</p>
                <h2>Khám phá theo nhu cầu</h2>
            </div>
        </div>

        <div class="category-feature-grid">
            @foreach (var category in Model.Categories.Take(4))
            {
                <a asp-controller="Products" asp-action="Category" asp-route-categoryId="@category.Id" class="category-feature-card">
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
                <h2>Lựa chọn mới nhất</h2>
            </div>
        </div>

        <div class="product-grid product-grid-home">
            @foreach (var product in Model.LatestProducts)
            {
                var primaryImage = product.ProductImages.FirstOrDefault(image => image.IsPrimary)?.ImageUrl
                    ?? product.ProductImages.FirstOrDefault()?.ImageUrl
                    ?? "https://placehold.co/600x600?text=No+Image";

                <article class="product-card product-card-home">
                    <img src="@((primaryImage.StartsWith("http", StringComparison.OrdinalIgnoreCase)) ? primaryImage : Url.Content($"~/{primaryImage}"))" alt="@product.Name" class="product-card-image" />
                    <div class="product-card-body">
                        <p class="product-category">@product.Category?.Name</p>
                        <h3>@product.Name</h3>
                        <p class="product-price">@product.Price.ToString("N0") VND</p>
                        <a asp-controller="Products" asp-action="Details" asp-route-id="@product.Id" class="button-secondary">Chi tiết</a>
                    </div>
                </article>
            }
        </div>
    </section>

    <section class="promo-banner">
        <div>
            <p class="eyebrow">Ưu đãi đặc biệt</p>
            <h2>Giảm giá cho những lựa chọn được yêu thích</h2>
            <p>Thiết kế storefront mới giúp nội dung thương mại nổi bật hơn, dễ mở rộng cho chiến dịch sau này.</p>
        </div>
    </section>

    <section class="newsletter-card">
        <div>
            <p class="eyebrow">Newsletter</p>
            <h2>Đăng ký nhận tin</h2>
            <p>Nhận thông tin sản phẩm mới và ưu đãi được tuyển chọn.</p>
        </div>
        <form class="newsletter-form">
            <input type="email" class="form-control" placeholder="Địa chỉ email của bạn" />
            <button type="button" class="button-primary">Đăng ký</button>
        </form>
    </section>
</section>
```

- [ ] **Step 5: Add homepage styles aligned with Lumina Retail**

Append to `wwwroot/css/site.css`:

```css
.home-page {
  display: grid;
  gap: 72px;
}

.hero-carousel {
  display: grid;
  grid-template-columns: 420px minmax(0, 1fr);
  gap: 24px;
  align-items: stretch;
}

.hero-copy-panel,
.newsletter-card,
.benefit-card,
.promo-banner {
  background: var(--color-canvas);
  border: 1px solid #E8E8ED;
  border-radius: 24px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.04);
}

.hero-copy-panel {
  padding: 40px;
  display: grid;
  align-content: center;
  gap: 18px;
}

.hero-slider-stage {
  position: relative;
  min-height: 520px;
  overflow: hidden;
  border-radius: 24px;
  background: linear-gradient(180deg, #f6f3f5 0%, #ffffff 100%);
  border: 1px solid #E8E8ED;
}

.hero-slide,
.hero-slide-image {
  width: 100%;
  height: 100%;
}

.hero-slide-image {
  object-fit: cover;
}

.hero-slide-empty {
  display: grid;
  place-items: center;
}

.hero-empty-state {
  padding: 32px;
  text-align: center;
}

.benefits-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 20px;
}

.benefit-card {
  padding: 28px;
}

.section-stack {
  display: grid;
  gap: 24px;
}

.category-feature-grid {
  display: grid;
  grid-template-columns: 1.3fr 1fr 1fr 1fr;
  gap: 20px;
}

.category-feature-card {
  min-height: 260px;
  background: linear-gradient(160deg, #f0edef, #e4e2e4);
  border-radius: 24px;
  overflow: hidden;
  position: relative;
}

.category-feature-overlay {
  position: absolute;
  inset: auto 0 0 0;
  padding: 24px;
  color: #fff;
  background: linear-gradient(180deg, transparent, rgba(27, 27, 29, 0.76));
}

.product-grid-home {
  grid-template-columns: repeat(4, minmax(0, 1fr));
}

.product-card-home {
  padding: 12px;
  background: #fff;
  border: 1px solid #E8E8ED;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.04);
}

.promo-banner {
  padding: 40px;
  background: linear-gradient(135deg, rgba(186, 0, 54, 0.10), rgba(255, 255, 255, 0.95));
}

.newsletter-card {
  padding: 32px;
  display: grid;
  gap: 20px;
}

.newsletter-form {
  display: grid;
  grid-template-columns: minmax(0, 1fr) 180px;
  gap: 16px;
}

@media (max-width: 991.98px) {
  .hero-carousel,
  .benefits-grid,
  .newsletter-form,
  .product-grid-home,
  .category-feature-grid {
    grid-template-columns: 1fr;
  }
}
```

- [ ] **Step 6: Run tests to verify green**

Run:

```bash
dotnet test tests/BaiTapWeb_Buoi5.Tests/BaiTapWeb_Buoi5.Tests.csproj --filter ProductsControllerTests -v minimal
dotnet build
```

Expected:

```text
Passed! 1 test passed.
Build succeeded.
```

- [ ] **Step 7: Commit**

```bash
git add ViewModels/HomePageViewModel.cs Controllers/ProductsController.cs Views/Products/Index.cshtml wwwroot/css/site.css tests/BaiTapWeb_Buoi5.Tests/ProductsControllerTests.cs
git commit -m "feat: redesign homepage with slider hero"
```

### Task 4: Add database migration and verify full integration

**Files:**
- Create: `Migrations/<timestamp>_AddSliderImages.cs`
- Modify: `Migrations/ApplicationDbContextModelSnapshot.cs`
- Test: `tests/BaiTapWeb_Buoi5.Tests/SliderRepositoryTests.cs`

- [ ] **Step 1: Generate migration**

Run:

```bash
export PATH="$PATH:/home/phan-duong-quoc-nhat/.dotnet/tools"
export DOTNET_ROOT=/snap/dotnet-sdk/256
dotnet ef migrations add AddSliderImages
```

Expected:

```text
Done. To undo this action, use 'ef migrations remove'
```

- [ ] **Step 2: Apply database update**

Run:

```bash
export PATH="$PATH:/home/phan-duong-quoc-nhat/.dotnet/tools"
export DOTNET_ROOT=/snap/dotnet-sdk/256
dotnet ef database update
```

Expected:

```text
Done.
```

- [ ] **Step 3: Manual verification**

Run:

```bash
dotnet run
```

Verify in browser:

```text
1. Homepage loads the redesigned storefront.
2. If slider table is empty, hero shows fallback state without crashing.
3. Admin nav shows the 'Slider' entry.
4. Admin can create a slider image with upload + display order.
5. Uploaded image appears in the homepage hero area.
6. Admin can update display order and see homepage order change.
7. Admin can delete a slider image and the hero updates correctly.
```

- [ ] **Step 4: Commit**

```bash
git add Migrations
git commit -m "feat: persist slider images"
```

## Self-Review

- Spec coverage:
  - Homepage redesign is covered by Task 3.
  - Backend slider storage and admin module are covered by Task 1 and Task 2.
  - Homepage consuming slider images from backend is covered by Task 3 and Task 4.
  - Admin menu integration is covered by Task 2.
  - No-SVG hero requirement is satisfied by Task 3 using uploaded `SliderImage` data only.
- Placeholder scan:
  - No `TODO`, `TBD`, or vague “handle later” steps remain.
- Type consistency:
  - `SliderImage`, `ISliderRepository`, `HomePageViewModel`, and `SliderFormViewModel` are used consistently across tasks.
