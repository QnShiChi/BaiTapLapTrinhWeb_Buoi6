# ASM5 BookDB Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the current Grade/Student sample with a Book/Category management application that matches ASM5 requirements and follows the visual system in `DESIGN.md`.

**Architecture:** Keep ASP.NET Core MVC + EF Core + SQL Server, replace the current domain with `Book` and `Category`, rebuild the schema with seeded data, then implement Book CRUD, category filtering, image upload, and token-driven Razor UI. Use a dedicated index view model so the sidebar category counts and the book grid can be rendered in one request.

**Tech Stack:** ASP.NET Core MVC, EF Core 8, SQL Server, Razor, Bootstrap utilities, custom CSS tokens from `DESIGN.md`

---

## File Structure

- Delete: `Controllers/GradeController.cs`
- Delete: `Views/Grade/Index.cshtml`
- Delete: `Models/Grade.cs`
- Delete: `Models/Student.cs`
- Modify: `Models/ApplicationDbContext.cs`
- Create: `Models/Category.cs`
- Create: `Models/Book.cs`
- Create: `ViewModels/BookIndexViewModel.cs`
- Create: `ViewModels/BookFormViewModel.cs`
- Create: `Controllers/BookController.cs`
- Create: `Views/Book/Index.cshtml`
- Create: `Views/Book/Details.cshtml`
- Create: `Views/Book/Create.cshtml`
- Create: `Views/Book/Edit.cshtml`
- Create: `Views/Book/Delete.cshtml`
- Modify: `Views/Shared/_Layout.cshtml`
- Modify: `Views/Home/Index.cshtml`
- Modify: `wwwroot/css/site.css`
- Create: `wwwroot/Content/ImagesBook/*`
- Replace: `Migrations/*`

### Task 1: Replace the domain models

**Files:**
- Create: `Models/Category.cs`
- Create: `Models/Book.cs`
- Modify: `Models/ApplicationDbContext.cs`
- Delete: `Models/Grade.cs`
- Delete: `Models/Student.cs`

- [ ] **Step 1: Run the baseline build**

Run: `dotnet build`
Expected: PASS with the current Grade/Student domain still in place.

- [ ] **Step 2: Add the new category model**

`Models/Category.cs`

```csharp
namespace BaiTapWeb_Buoi5.Models;

public class Category
{
    public int CategoryId { get; set; }
    public required string CategoryName { get; set; }
    public ICollection<Book> Books { get; set; } = new List<Book>();
}
```

- [ ] **Step 3: Add the new book model**

`Models/Book.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaiTapWeb_Buoi5.Models;

public class Book
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Author { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,0)")]
    public decimal Price { get; set; }

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Image { get; set; } = string.Empty;

    public int CategoryId { get; set; }
    public Category? Category { get; set; }
}
```

- [ ] **Step 4: Replace the DbContext contents**

`Models/ApplicationDbContext.cs`

```csharp
using Microsoft.EntityFrameworkCore;

namespace BaiTapWeb_Buoi5.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Book> Books => Set<Book>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>()
            .HasMany(category => category.Books)
            .WithOne(book => book.Category)
            .HasForeignKey(book => book.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

- [ ] **Step 5: Remove the old domain files**

Delete:

```text
Models/Grade.cs
Models/Student.cs
```

- [ ] **Step 6: Run build again**

Run: `dotnet build`
Expected: PASS with no `Grade` or `Student` references left.

- [ ] **Step 7: Commit**

```bash
git add Models/ApplicationDbContext.cs Models/Category.cs Models/Book.cs Models/Grade.cs Models/Student.cs
git commit -m "refactor: replace grade domain with book entities"
```

### Task 2: Rebuild migrations and seed BookDB

**Files:**
- Modify: `Models/ApplicationDbContext.cs`
- Replace: `Migrations/*`
- Create: `wwwroot/Content/ImagesBook/*`

- [ ] **Step 1: Add sample cover images**

Create these files:

```text
wwwroot/Content/ImagesBook/cho-toi-xin-mot-ve-di-tuoi-tho.jpg
wwwroot/Content/ImagesBook/lap-trinh-csharp.jpg
wwwroot/Content/ImagesBook/java-fundamentals.jpg
wwwroot/Content/ImagesBook/cuoc-song-rat-giong-cuoc-doi.jpg
```

- [ ] **Step 2: Add deterministic seed data in `OnModelCreating`**

Append this block inside `Models/ApplicationDbContext.cs`:

```csharp
modelBuilder.Entity<Category>().HasData(
    new Category { CategoryId = 1, CategoryName = "Cuoc song" },
    new Category { CategoryId = 2, CategoryName = "Lap trinh" },
    new Category { CategoryId = 3, CategoryName = "Suc khoe" }
);

modelBuilder.Entity<Book>().HasData(
    new Book
    {
        Id = 1,
        Title = "Cho toi xin mot ve di tuoi tho",
        Author = "Nguyen Nhat Anh",
        Price = 61600,
        Description = "Tac pham noi bat ve ky uc tuoi tho va nhung rung dong rat nhe.",
        Image = "Content/ImagesBook/cho-toi-xin-mot-ve-di-tuoi-tho.jpg",
        CategoryId = 1
    },
    new Book
    {
        Id = 2,
        Title = "Cuoc song rat giong cuoc doi",
        Author = "Hai Do",
        Price = 100000,
        Description = "Sach truyen cam hung ve cach nhin doi song tu nhieu goc do.",
        Image = "Content/ImagesBook/cuoc-song-rat-giong-cuoc-doi.jpg",
        CategoryId = 1
    },
    new Book
    {
        Id = 3,
        Title = "Lap trinh C#",
        Author = "Tac gia giao trinh",
        Price = 120000,
        Description = "Sach nhap mon C# cho sinh vien va nguoi moi hoc lap trinh.",
        Image = "Content/ImagesBook/lap-trinh-csharp.jpg",
        CategoryId = 2
    },
    new Book
    {
        Id = 4,
        Title = "Java Fundamentals",
        Author = "James Gosling",
        Price = 135000,
        Description = "Sach tong hop kien thuc Java co ban voi cac vi du nen tang.",
        Image = "Content/ImagesBook/java-fundamentals.jpg",
        CategoryId = 2
    }
);
```

- [ ] **Step 3: Remove the old migration files**

Delete all existing files under `Migrations/` that reference `Grade` or `Student`.

- [ ] **Step 4: Build before migration generation**

Run: `dotnet build`
Expected: PASS.

- [ ] **Step 5: Generate a fresh migration**

Run: `DOTNET_ROOT=/snap/dotnet-sdk/256 /home/phan-duong-quoc-nhat/.dotnet/tools/dotnet-ef migrations add InitialBookDb`
Expected: PASS.

- [ ] **Step 6: Apply the schema**

Run: `DOTNET_ROOT=/snap/dotnet-sdk/256 /home/phan-duong-quoc-nhat/.dotnet/tools/dotnet-ef database update`
Expected: PASS with `Books` and `Categories` created plus seed rows inserted.

- [ ] **Step 7: Commit**

```bash
git add Models/ApplicationDbContext.cs Migrations wwwroot/Content/ImagesBook
git commit -m "feat: create BookDB schema and seed data"
```

### Task 3: Implement the Book index page with category counts

**Files:**
- Create: `ViewModels/BookIndexViewModel.cs`
- Create: `Controllers/BookController.cs`
- Create: `Views/Book/Index.cshtml`
- Modify: `Views/Shared/_Layout.cshtml`
- Modify: `Views/Home/Index.cshtml`

- [ ] **Step 1: Verify `/Book` is not implemented yet**

Run:

```bash
dotnet run --urls http://localhost:5123
curl -i http://127.0.0.1:5123/Book
```

Expected: `404 Not Found` before implementation.

- [ ] **Step 2: Add the index page view model**

`ViewModels/BookIndexViewModel.cs`

```csharp
using BaiTapWeb_Buoi5.Models;

namespace BaiTapWeb_Buoi5.ViewModels;

public class BookIndexViewModel
{
    public List<Category> Categories { get; set; } = new();
    public List<Book> Books { get; set; } = new();
    public int? SelectedCategoryId { get; set; }
}
```

- [ ] **Step 3: Add `BookController.Index`**

`Controllers/BookController.cs`

```csharp
using BaiTapWeb_Buoi5.Models;
using BaiTapWeb_Buoi5.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaiTapWeb_Buoi5.Controllers;

public class BookController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public BookController(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<IActionResult> Index(int? categoryId)
    {
        var categories = await _context.Categories
            .Include(category => category.Books)
            .OrderBy(category => category.CategoryId)
            .ToListAsync();

        var booksQuery = _context.Books
            .Include(book => book.Category)
            .AsQueryable();

        if (categoryId.HasValue)
        {
            booksQuery = booksQuery.Where(book => book.CategoryId == categoryId.Value);
        }

        var model = new BookIndexViewModel
        {
            Categories = categories,
            Books = await booksQuery.OrderByDescending(book => book.Id).ToListAsync(),
            SelectedCategoryId = categoryId
        };

        return View(model);
    }
}
```

- [ ] **Step 4: Add the initial `Views/Book/Index.cshtml`**

```cshtml
@model BaiTapWeb_Buoi5.ViewModels.BookIndexViewModel

@{
    ViewData["Title"] = "Trang chu";
}

<section class="book-page-shell">
    <aside class="category-sidebar">
        <h2>Chu de</h2>
        <ul class="category-list">
            @foreach (var category in Model.Categories)
            {
                var isActive = Model.SelectedCategoryId == category.CategoryId;
                <li>
                    <a class="@(isActive ? "is-active" : string.Empty)"
                       asp-controller="Book"
                       asp-action="Index"
                       asp-route-categoryId="@category.CategoryId">
                        @category.CategoryName (@category.Books.Count)
                    </a>
                </li>
            }
        </ul>
    </aside>

    <div class="book-content">
        <div class="book-toolbar">
            <h1>Thu vien sach</h1>
            <a asp-controller="Book" asp-action="Create" class="button-primary">Them moi</a>
        </div>

        <div class="book-grid">
            @foreach (var book in Model.Books)
            {
                <article class="book-card">
                    <a asp-controller="Book" asp-action="Edit" asp-route-id="@book.Id" class="book-card-edit">Edit</a>
                    <img src="~/@book.Image" alt="@book.Title" class="book-card-image" />
                    <h3>@book.Title</h3>
                    <p>Tac gia: @book.Author</p>
                    <p>The loai: @book.Category?.CategoryName</p>
                    <div class="book-card-actions">
                        <a asp-controller="Book" asp-action="Details" asp-route-id="@book.Id" class="button-ghost">Chi tiet</a>
                        <a asp-controller="Book" asp-action="Delete" asp-route-id="@book.Id" class="button-danger">Xoa</a>
                    </div>
                </article>
            }
        </div>
    </div>
</section>
```

- [ ] **Step 5: Point Home and layout to the Book flow**

`Views/Home/Index.cshtml`

```cshtml
@{
    Response.Redirect(Url.Action("Index", "Book")!);
}
```

`Views/Shared/_Layout.cshtml` navbar links:

```cshtml
<a asp-controller="Book" asp-action="Index">Trang chu</a>
<a asp-controller="Book" asp-action="Create">Them moi</a>
```

- [ ] **Step 6: Verify HTML output**

Run:

```bash
dotnet build
dotnet run --urls http://localhost:5123
curl -s http://127.0.0.1:5123/Book
```

Expected: HTML contains category names, counts, and seeded book titles.

- [ ] **Step 7: Commit**

```bash
git add Controllers/BookController.cs ViewModels/BookIndexViewModel.cs Views/Book/Index.cshtml Views/Home/Index.cshtml Views/Shared/_Layout.cshtml
git commit -m "feat: add book listing and category filter"
```

### Task 4: Add create, edit, and delete with image upload

**Files:**
- Create: `ViewModels/BookFormViewModel.cs`
- Modify: `Controllers/BookController.cs`
- Create: `Views/Book/Create.cshtml`
- Create: `Views/Book/Edit.cshtml`
- Create: `Views/Book/Delete.cshtml`

- [ ] **Step 1: Confirm the CRUD routes are missing**

Run:

```bash
curl -i http://127.0.0.1:5123/Book/Create
curl -i http://127.0.0.1:5123/Book/Edit/1
curl -i http://127.0.0.1:5123/Book/Delete/1
```

Expected: missing-action responses before implementation.

- [ ] **Step 2: Add the form view model**

`ViewModels/BookFormViewModel.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BaiTapWeb_Buoi5.ViewModels;

public class BookFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Author { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Required]
    public string Description { get; set; } = string.Empty;

    public IFormFile? ImageFile { get; set; }
    public string? ExistingImage { get; set; }
    public int CategoryId { get; set; }
    public List<SelectListItem> Categories { get; set; } = new();
}
```

- [ ] **Step 3: Add image helpers and GET/POST create-edit-delete actions**

Add these helpers to `BookController`:

```csharp
private async Task<string> SaveImageAsync(IFormFile imageFile)
{
    var uploadsFolder = Path.Combine(_environment.WebRootPath, "Content", "ImagesBook");
    Directory.CreateDirectory(uploadsFolder);

    var safeFileName = Path.GetFileName(imageFile.FileName);
    var fileName = $"{Guid.NewGuid()}_{safeFileName}";
    var filePath = Path.Combine(uploadsFolder, fileName);

    await using var stream = System.IO.File.Create(filePath);
    await imageFile.CopyToAsync(stream);

    return Path.Combine("Content", "ImagesBook", fileName).Replace("\\", "/");
}

private async Task<List<SelectListItem>> GetCategoryItemsAsync()
{
    return await _context.Categories
        .OrderBy(category => category.CategoryName)
        .Select(category => new SelectListItem
        {
            Value = category.CategoryId.ToString(),
            Text = category.CategoryName
        })
        .ToListAsync();
}
```

Implement `Create`, `Edit`, and `Delete` actions using `BookFormViewModel`, preserving `ExistingImage` when no new upload is provided.

- [ ] **Step 4: Add the Razor forms**

`Views/Book/Create.cshtml` and `Views/Book/Edit.cshtml` should use:

```cshtml
<form method="post" enctype="multipart/form-data" class="book-form-card">
    <div asp-validation-summary="ModelOnly" class="text-danger"></div>
    <input asp-for="Title" class="book-input" />
    <input asp-for="Author" class="book-input" />
    <input asp-for="Price" class="book-input" />
    <textarea asp-for="Description" class="book-input book-textarea"></textarea>
    <select asp-for="CategoryId" asp-items="Model.Categories" class="book-input"></select>
    <input asp-for="ImageFile" type="file" accept="image/*" class="book-input" />
    <button type="submit" class="button-primary">Luu</button>
</form>
```

Create `Views/Book/Delete.cshtml` as a confirmation page showing title, cover, and a submit button.

- [ ] **Step 5: Verify the CRUD flow**

Run: `dotnet build`
Expected: PASS.

Then manually verify in browser:
- create one book with an uploaded image
- edit it without changing the image
- edit it again with a new image
- delete it

- [ ] **Step 6: Commit**

```bash
git add Controllers/BookController.cs ViewModels/BookFormViewModel.cs Views/Book/Create.cshtml Views/Book/Edit.cshtml Views/Book/Delete.cshtml
git commit -m "feat: add book CRUD with image upload"
```

### Task 5: Add details page and apply `DESIGN.md` styling

**Files:**
- Modify: `Controllers/BookController.cs`
- Create: `Views/Book/Details.cshtml`
- Modify: `Views/Shared/_Layout.cshtml`
- Modify: `wwwroot/css/site.css`
- Modify: `Views/Book/Index.cshtml`
- Modify: `Views/Book/Create.cshtml`
- Modify: `Views/Book/Edit.cshtml`
- Modify: `Views/Book/Delete.cshtml`

- [ ] **Step 1: Confirm the details page is missing or incomplete**

Run: `curl -i http://127.0.0.1:5123/Book/Details/1`
Expected: missing or incomplete output before implementation.

- [ ] **Step 2: Add the details action**

```csharp
public async Task<IActionResult> Details(int id)
{
    var book = await _context.Books
        .Include(item => item.Category)
        .FirstOrDefaultAsync(item => item.Id == id);

    if (book is null)
    {
        return NotFound();
    }

    return View(book);
}
```

- [ ] **Step 3: Add the details view**

`Views/Book/Details.cshtml`

```cshtml
@model BaiTapWeb_Buoi5.Models.Book

@{
    ViewData["Title"] = "Chi tiet sach";
}

<section class="book-detail-shell">
    <div class="book-detail-image-card">
        <img src="~/@Model.Image" alt="@Model.Title" class="book-detail-image" />
    </div>
    <div class="book-detail-content">
        <p class="book-detail-kicker">THONG TIN CHI TIET SACH</p>
        <h1>@Model.Title</h1>
        <p><strong>Tac gia:</strong> @Model.Author</p>
        <p><strong>Chu de:</strong> @Model.Category?.CategoryName</p>
        <p class="book-detail-description">@Model.Description</p>
        <p class="book-detail-price">@Model.Price.ToString("N0") VND</p>
        <div class="book-detail-actions">
            <button type="button" class="button-ghost">Them vao gio hang</button>
            <button type="button" class="button-primary">Mua ngay</button>
        </div>
    </div>
</section>
```

- [ ] **Step 4: Replace the default site CSS with token-driven classes**

At minimum add token variables and these selectors in `wwwroot/css/site.css`:

```css
:root {
  --color-midnight-ink: #000000;
  --color-canvas-white: #ffffff;
  --color-charcoal-border: #171717;
  --color-shadow-base: #0a0a0d;
  --color-pale-ash: #f5f5f5;
  --color-accent-green: #a3e635;
  --font-satoshi: Satoshi, ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, Segoe
