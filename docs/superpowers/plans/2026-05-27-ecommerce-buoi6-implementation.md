# Ecommerce Buoi 6 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Rebuild the current ASP.NET Core MVC demo into a working ecommerce application with SQL Server, EF Core, Identity, repository pattern, public product browsing, and an admin area for product/category management.

**Architecture:** Keep the existing ASP.NET Core MVC host project, remove old `Book` demo code, and rebuild the business layer around `Category`, `Product`, `ProductImage`, and `ApplicationUser`. Use `ApplicationDbContext : IdentityDbContext<ApplicationUser>`, repository abstractions, public MVC controllers for browsing, and `Area/Admin` controllers protected by role-based authorization.

**Tech Stack:** ASP.NET Core MVC (.NET 8), Entity Framework Core, SQL Server 2022 in Docker, ASP.NET Core Identity, Razor Views, Bootstrap base assets with custom CSS.

---

## File Structure

### Files to Delete

- `Controllers/BookController.cs`
- `Models/Book.cs`
- `ViewModels/BookIndexViewModel.cs`
- `ViewModels/BookFormViewModel.cs`
- `Views/Book/Index.cshtml`
- `Views/Book/Create.cshtml`
- `Views/Book/Edit.cshtml`
- `Views/Book/Details.cshtml`
- `Views/Book/Delete.cshtml`
- `Migrations/20260520085624_InitialBookDb.cs`
- `Migrations/20260520085624_InitialBookDb.Designer.cs`
- `Migrations/ApplicationDbContextModelSnapshot.cs`

### Files to Create

- `Models/ApplicationUser.cs`
- `Models/Product.cs`
- `Models/ProductImage.cs`
- `Repositories/IProductRepository.cs`
- `Repositories/ProductRepository.cs`
- `Repositories/ICategoryRepository.cs`
- `Repositories/CategoryRepository.cs`
- `Data/DbInitializer.cs`
- `Areas/Admin/Controllers/ProductController.cs`
- `Areas/Admin/Controllers/CategoryController.cs`
- `Areas/Admin/Views/_ViewImports.cshtml`
- `Areas/Admin/Views/_ViewStart.cshtml`
- `Areas/Admin/Views/Shared/_AdminLayout.cshtml`
- `Areas/Admin/Views/Product/Index.cshtml`
- `Areas/Admin/Views/Product/Create.cshtml`
- `Areas/Admin/Views/Product/Edit.cshtml`
- `Areas/Admin/Views/Product/Details.cshtml`
- `Areas/Admin/Views/Product/Delete.cshtml`
- `Areas/Admin/Views/Category/Index.cshtml`
- `Areas/Admin/Views/Category/Create.cshtml`
- `Areas/Admin/Views/Category/Edit.cshtml`
- `Areas/Admin/Views/Category/Delete.cshtml`
- `Controllers/ProductsController.cs`
- `Controllers/CategoryController.cs`
- `Views/Products/Index.cshtml`
- `Views/Products/Details.cshtml`
- `Views/Category/Index.cshtml`
- `Views/Shared/_LoginPartial.cshtml`
- `Views/Shared/AccessDenied.cshtml`
- `Views/Shared/_StatusMessage.cshtml`
- `ViewModels/ProductFormViewModel.cs`
- `ViewModels/CategoryFormViewModel.cs`
- `ViewModels/ProductListViewModel.cs`
- `ViewModels/ProductDetailsViewModel.cs`
- `ViewModels/AdminProductListItemViewModel.cs`
- `ViewModels/RegisterViewModel.cs`
- `Areas/Identity/Pages/Account/Register.cshtml`
- `Areas/Identity/Pages/Account/Register.cshtml.cs`

### Files to Modify

- `Program.cs`
- `appsettings.json`
- `docker-compose.yml`
- `Models/ApplicationDbContext.cs`
- `Models/Category.cs`
- `Controllers/HomeController.cs`
- `Views/Home/Index.cshtml`
- `Views/Shared/_Layout.cshtml`
- `Views/Shared/_ValidationScriptsPartial.cshtml`
- `Views/_ViewImports.cshtml`
- `wwwroot/css/site.css`
- `BaiTapWeb_Buoi5.csproj`

### Files Generated During Execution

- `Migrations/<timestamp>_InitialECommerceIdentity.cs`
- `Migrations/<timestamp>_InitialECommerceIdentity.Designer.cs`
- `Migrations/ApplicationDbContextModelSnapshot.cs`

## Task 1: Update Project Dependencies and Host Configuration

**Files:**
- Modify: `BaiTapWeb_Buoi5.csproj`
- Modify: `Program.cs`
- Modify: `appsettings.json`
- Modify: `docker-compose.yml`

- [ ] **Step 1: Write the failing configuration expectation checklist**

```text
Expected after this task:
- Project references ASP.NET Core Identity EF package
- Program.cs registers DbContext, Identity, repositories, and admin/public routes
- appsettings.json contains SQL Server Docker connection string for QLBanHang
- docker-compose.yml can start SQL Server with sa/123456 without extra env files
```

- [ ] **Step 2: Verify current package set is incomplete for Identity**

Run: `rg -n "Microsoft.AspNetCore.Identity|AddDefaultIdentity|AddIdentity" BaiTapWeb_Buoi5.csproj Program.cs`
Expected: no Identity package or registration found

- [ ] **Step 3: Update `BaiTapWeb_Buoi5.csproj` to include Identity EF and scaffolding support**

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore" Version="8.0.27" />
    <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.27" />
    <PackageReference Include="Microsoft.AspNetCore.Identity.UI" Version="8.0.27" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.27">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.27" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.27">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>
</Project>
```

- [ ] **Step 4: Update `appsettings.json` with the SQL Server Docker connection**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=QLBanHang;User Id=sa;Password=123456;TrustServerCertificate=True;MultipleActiveResultSets=True"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

- [ ] **Step 5: Replace `docker-compose.yml` with a self-contained SQL Server service**

```yaml
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: qlbanhang_sqlserver
    environment:
      ACCEPT_EULA: "Y"
      MSSQL_SA_PASSWORD: "123456"
      MSSQL_PID: "Developer"
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql
    restart: unless-stopped

volumes:
  sqlserver_data:
```

- [ ] **Step 6: Replace `Program.cs` with Identity and area-aware routing**

```csharp
using BaiTapWeb_Buoi5.Data;
using BaiTapWeb_Buoi5.Models;
using BaiTapWeb_Buoi5.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseMigrationsEndPoint();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    await DbInitializer.SeedAsync(scope.ServiceProvider);
}

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
```

- [ ] **Step 7: Build to confirm the host setup compiles after package restore**

Run: `dotnet build`
Expected: build may still fail until later tasks add missing types, but package restore should complete and the remaining errors should reference not-yet-created models/repositories

- [ ] **Step 8: Commit**

```bash
git add BaiTapWeb_Buoi5.csproj Program.cs appsettings.json docker-compose.yml
git commit -m "chore: configure host for identity and sql server"
```

## Task 2: Replace Demo Models With Ecommerce + Identity Models

**Files:**
- Create: `Models/ApplicationUser.cs`
- Create: `Models/Product.cs`
- Create: `Models/ProductImage.cs`
- Modify: `Models/Category.cs`
- Modify: `Models/ApplicationDbContext.cs`
- Delete: `Models/Book.cs`

- [ ] **Step 1: Write the failing domain checklist**

```text
Expected after this task:
- Book model is removed
- Category/Product/ProductImage/ApplicationUser are the only business entities
- ApplicationDbContext inherits IdentityDbContext<ApplicationUser>
- DbSets match ecommerce + Identity requirements
```

- [ ] **Step 2: Verify old `Book` references still exist**

Run: `rg -n "\bBook\b|\bBooks\b" Models Controllers Views ViewModels`
Expected: multiple matches across current demo files

- [ ] **Step 3: Create `Models/ApplicationUser.cs`**

```csharp
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BaiTapWeb_Buoi5.Models;

public class ApplicationUser : IdentityUser
{
    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, StringLength(250)]
    public string Address { get; set; } = string.Empty;
}
```

- [ ] **Step 4: Create `Models/Product.cs`**

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaiTapWeb_Buoi5.Models;

public class Product
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, 999999999)]
    public decimal Price { get; set; }

    [Required, StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    public Category? Category { get; set; }

    public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
}
```

- [ ] **Step 5: Create `Models/ProductImage.cs`**

```csharp
using System.ComponentModel.DataAnnotations;

namespace BaiTapWeb_Buoi5.Models;

public class ProductImage
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    [Required, StringLength(255)]
    public string ImageUrl { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public Product? Product { get; set; }
}
```

- [ ] **Step 6: Replace `Models/Category.cs` with the ecommerce version**

```csharp
using System.ComponentModel.DataAnnotations;

namespace BaiTapWeb_Buoi5.Models;

public class Category
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
```

- [ ] **Step 7: Replace `Models/ApplicationDbContext.cs`**

```csharp
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BaiTapWeb_Buoi5.Models;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

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
}
```

- [ ] **Step 8: Delete the old `Models/Book.cs` file**

Run: `rm Models/Book.cs`
Expected: file removed from working tree

- [ ] **Step 9: Build to confirm entity and DbContext shape**

Run: `dotnet build`
Expected: remaining errors should now come from controllers/views that still reference `Book`

- [ ] **Step 10: Commit**

```bash
git add Models/ApplicationUser.cs Models/Product.cs Models/ProductImage.cs Models/Category.cs Models/ApplicationDbContext.cs
git rm Models/Book.cs
git commit -m "feat: add ecommerce identity domain models"
```

## Task 3: Add Repository Interfaces and EF Core Implementations

**Files:**
- Create: `Repositories/IProductRepository.cs`
- Create: `Repositories/ProductRepository.cs`
- Create: `Repositories/ICategoryRepository.cs`
- Create: `Repositories/CategoryRepository.cs`

- [ ] **Step 1: Write the failing repository checklist**

```text
Expected after this task:
- Controllers can depend on repositories instead of DbContext
- Product queries include Category and ProductImages where needed
- Category deletion safely removes child products and images
```

- [ ] **Step 2: Create `Repositories/IProductRepository.cs`**

```csharp
using BaiTapWeb_Buoi5.Models;

namespace BaiTapWeb_Buoi5.Repositories;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId);
    Task<Product?> GetByIdAsync(int id);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
}
```

- [ ] **Step 3: Create `Repositories/ICategoryRepository.cs`**

```csharp
using BaiTapWeb_Buoi5.Models;

namespace BaiTapWeb_Buoi5.Repositories;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    Task AddAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(int id);
}
```

- [ ] **Step 4: Create `Repositories/ProductRepository.cs`**

```csharp
using BaiTapWeb_Buoi5.Models;
using Microsoft.EntityFrameworkCore;

namespace BaiTapWeb_Buoi5.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products
            .Include(product => product.Category)
            .Include(product => product.ProductImages)
            .OrderByDescending(product => product.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId)
    {
        return await _context.Products
            .Include(product => product.Category)
            .Include(product => product.ProductImages)
            .Where(product => product.CategoryId == categoryId)
            .OrderByDescending(product => product.Id)
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .Include(product => product.Category)
            .Include(product => product.ProductImages.OrderBy(image => image.Id))
            .FirstOrDefaultAsync(product => product.Id == id);
    }

    public async Task AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _context.Products
            .Include(item => item.ProductImages)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (product is null)
        {
            return;
        }

        _context.ProductImages.RemoveRange(product.ProductImages);
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
    }
}
```

- [ ] **Step 5: Create `Repositories/CategoryRepository.cs` with transactional delete**

```csharp
using BaiTapWeb_Buoi5.Models;
using Microsoft.EntityFrameworkCore;

namespace BaiTapWeb_Buoi5.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;

    public CategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _context.Categories
            .Include(category => category.Products)
            .OrderBy(category => category.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories
            .Include(category => category.Products)
            .ThenInclude(product => product.ProductImages)
            .FirstOrDefaultAsync(category => category.Id == id);
    }

    public async Task AddAsync(Category category)
    {
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Category category)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _context.Categories
            .Include(item => item.Products)
            .ThenInclude(product => product.ProductImages)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (category is null)
        {
            return;
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        foreach (var product in category.Products)
        {
            _context.ProductImages.RemoveRange(product.ProductImages);
        }

        _context.Products.RemoveRange(category.Products);
        _context.Categories.Remove(category);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}
```

- [ ] **Step 6: Build to verify repository layer compiles**

Run: `dotnet build`
Expected: repository layer compiles; remaining errors still come from old MVC files

- [ ] **Step 7: Commit**

```bash
git add Repositories
git commit -m "feat: add product and category repositories"
```

## Task 4: Add Identity Seed Logic

**Files:**
- Create: `Data/DbInitializer.cs`

- [ ] **Step 1: Write the failing seed checklist**

```text
Expected after this task:
- Roles Admin and Member are created if missing
- One admin and one member account are created if missing
- New users can coexist with seed users without duplicate role errors
```

- [ ] **Step 2: Create `Data/DbInitializer.cs`**

```csharp
using BaiTapWeb_Buoi5.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BaiTapWeb_Buoi5.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await context.Database.MigrateAsync();

        string[] roles = ["Admin", "Member"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        await EnsureUserAsync(
            userManager,
            userName: "admin",
            email: "admin@qlbanhang.local",
            fullName: "System Admin",
            address: "Admin Address",
            password: "Admin@123",
            role: "Admin");

        await EnsureUserAsync(
            userManager,
            userName: "member",
            email: "member@qlbanhang.local",
            fullName: "Demo Member",
            address: "Member Address",
            password: "Member@123",
            role: "Member");
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string userName,
        string email,
        string fullName,
        string address,
        string password,
        string role)
    {
        var user = await userManager.FindByNameAsync(userName);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = userName,
                Email = email,
                FullName = fullName,
                Address = address,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Cannot seed user {userName}: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }
}
```

- [ ] **Step 3: Build to ensure startup seeding compiles**

Run: `dotnet build`
Expected: seed logic compiles, remaining errors are still in MVC files not yet replaced

- [ ] **Step 4: Commit**

```bash
git add Data/DbInitializer.cs Program.cs
git commit -m "feat: add identity seed initialization"
```

## Task 5: Remove Demo MVC Files and Add Shared ViewModels

**Files:**
- Delete: `Controllers/BookController.cs`
- Delete: `ViewModels/BookIndexViewModel.cs`
- Delete: `ViewModels/BookFormViewModel.cs`
- Delete: `Views/Book/*`
- Create: `ViewModels/ProductFormViewModel.cs`
- Create: `ViewModels/CategoryFormViewModel.cs`
- Create: `ViewModels/ProductListViewModel.cs`
- Create: `ViewModels/ProductDetailsViewModel.cs`
- Create: `ViewModels/AdminProductListItemViewModel.cs`
- Create: `ViewModels/RegisterViewModel.cs`

- [ ] **Step 1: Write the failing cleanup checklist**

```text
Expected after this task:
- No Book controller/viewmodel/view remains
- New view models exist for public and admin product/category flows
```

- [ ] **Step 2: Delete the old Book MVC files**

Run: `rm -rf Controllers/BookController.cs ViewModels/BookIndexViewModel.cs ViewModels/BookFormViewModel.cs Views/Book`
Expected: Book MVC files removed

- [ ] **Step 3: Create `ViewModels/CategoryFormViewModel.cs`**

```csharp
using System.ComponentModel.DataAnnotations;

namespace BaiTapWeb_Buoi5.ViewModels;

public class CategoryFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }
}
```

- [ ] **Step 4: Create `ViewModels/ProductFormViewModel.cs`**

```csharp
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace BaiTapWeb_Buoi5.ViewModels;

public class ProductFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 999999999)]
    public decimal Price { get; set; }

    [Required, StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Category")]
    [Range(1, int.MaxValue, ErrorMessage = "Please choose a category.")]
    public int CategoryId { get; set; }

    [Display(Name = "Product images")]
    public List<IFormFile> ImageFiles { get; set; } = [];

    public List<string> ExistingImages { get; set; } = [];

    public List<SelectListItem> Categories { get; set; } = [];
}
```

- [ ] **Step 5: Create `ViewModels/ProductListViewModel.cs`**

```csharp
using BaiTapWeb_Buoi5.Models;

namespace BaiTapWeb_Buoi5.ViewModels;

public class ProductListViewModel
{
    public IEnumerable<Product> Products { get; set; } = [];
    public IEnumerable<Category> Categories { get; set; } = [];
    public int? SelectedCategoryId { get; set; }
    public string PageTitle { get; set; } = string.Empty;
}
```

- [ ] **Step 6: Create `ViewModels/ProductDetailsViewModel.cs`**

```csharp
using BaiTapWeb_Buoi5.Models;

namespace BaiTapWeb_Buoi5.ViewModels;

public class ProductDetailsViewModel
{
    public Product Product { get; set; } = default!;
    public IEnumerable<Category> Categories { get; set; } = [];
}
```

- [ ] **Step 7: Create `ViewModels/AdminProductListItemViewModel.cs`**

```csharp
namespace BaiTapWeb_Buoi5.ViewModels;

public class AdminProductListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int ImageCount { get; set; }
}
```

- [ ] **Step 8: Create `ViewModels/RegisterViewModel.cs`**

```csharp
using System.ComponentModel.DataAnnotations;

namespace BaiTapWeb_Buoi5.ViewModels;

public class RegisterViewModel
{
    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, StringLength(250)]
    public string Address { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password), Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;
}
```

- [ ] **Step 9: Build to verify old Book MVC dependencies are gone**

Run: `dotnet build`
Expected: build errors now point to missing new controllers/views only

- [ ] **Step 10: Commit**

```bash
git add ViewModels
git rm Controllers/BookController.cs ViewModels/BookIndexViewModel.cs ViewModels/BookFormViewModel.cs
git add -A Views/Book
git commit -m "refactor: remove book demo mvc files"
```

## Task 6: Implement Public Browsing Controllers

**Files:**
- Modify: `Controllers/HomeController.cs`
- Create: `Controllers/ProductsController.cs`
- Create: `Controllers/CategoryController.cs`

- [ ] **Step 1: Write the failing public controller checklist**

```text
Expected after this task:
- Home redirects to product catalog
- ProductsController supports index, details, and category filter
- Public users have no create/edit/delete actions
```

- [ ] **Step 2: Replace `Controllers/HomeController.cs` with catalog-first behavior**

```csharp
using Microsoft.AspNetCore.Mvc;

namespace BaiTapWeb_Buoi5.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Products");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult AccessDenied()
    {
        return View();
    }
}
```

- [ ] **Step 3: Create `Controllers/ProductsController.cs`**

```csharp
using BaiTapWeb_Buoi5.Repositories;
using BaiTapWeb_Buoi5.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BaiTapWeb_Buoi5.Controllers;

public class ProductsController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public ProductsController(IProductRepository productRepository, ICategoryRepository categoryRepository)
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
            PageTitle = "All Products"
        };

        return View(model);
    }

    public async Task<IActionResult> Category(int categoryId)
    {
        var categories = await _categoryRepository.GetAllAsync();
        var selected = categories.FirstOrDefault(category => category.Id == categoryId);
        if (selected is null)
        {
            return NotFound();
        }

        var model = new ProductListViewModel
        {
            Products = await _productRepository.GetByCategoryIdAsync(categoryId),
            Categories = categories,
            SelectedCategoryId = categoryId,
            PageTitle = selected.Name
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

- [ ] **Step 4: Create `Controllers/CategoryController.cs`**

```csharp
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
        var categories = await _categoryRepository.GetAllAsync();
        return View(categories);
    }
}
```

- [ ] **Step 5: Build to verify public controller layer**

Run: `dotnet build`
Expected: controller layer compiles; view-related errors remain until Razor files are added

- [ ] **Step 6: Commit**

```bash
git add Controllers/HomeController.cs Controllers/ProductsController.cs Controllers/CategoryController.cs
git commit -m "feat: add public catalog controllers"
```

## Task 7: Implement Admin Area Controllers With Role Protection

**Files:**
- Create: `Areas/Admin/Controllers/ProductController.cs`
- Create: `Areas/Admin/Controllers/CategoryController.cs`

- [ ] **Step 1: Write the failing admin controller checklist**

```text
Expected after this task:
- Admin area exists
- All admin controllers require Admin role
- Product and category CRUD endpoints exist
- Category delete page can display child-product warning
```

- [ ] **Step 2: Create `Areas/Admin/Controllers/ProductController.cs`**

```csharp
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
            ModelState.AddModelError(nameof(model.ImageFiles), "Please choose at least one image.");
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
```

- [ ] **Step 3: Create `Areas/Admin/Controllers/CategoryController.cs`**

```csharp
using BaiTapWeb_Buoi5.Models;
using BaiTapWeb_Buoi5.Repositories;
using BaiTapWeb_Buoi5.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BaiTapWeb_Buoi5.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
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

    public IActionResult Create()
    {
        return View(new CategoryFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _categoryRepository.AddAsync(new Category
        {
            Name = model.Name,
            Description = model.Description
        });

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category is null)
        {
            return NotFound();
        }

        return View(new CategoryFormViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryFormViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _categoryRepository.UpdateAsync(new Category
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description
        });

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        return category is null ? NotFound() : View(category);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _categoryRepository.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
```

- [ ] **Step 4: Build to verify admin controllers**

Run: `dotnet build`
Expected: controller layer compiles; remaining issues should be view or Identity UI related

- [ ] **Step 5: Commit**

```bash
git add Areas/Admin/Controllers
git commit -m "feat: add admin area controllers"
```

## Task 8: Scaffold and Customize Identity Registration

**Files:**
- Create: `Areas/Identity/Pages/Account/Register.cshtml`
- Create: `Areas/Identity/Pages/Account/Register.cshtml.cs`
- Create: `Views/Shared/_LoginPartial.cshtml`
- Create: `Views/Shared/_StatusMessage.cshtml`

- [ ] **Step 1: Write the failing identity UI checklist**

```text
Expected after this task:
- Register page collects UserName, FullName, Address, Email, Password
- New users are assigned the Member role
- Shared layout can render login/register/logout links
```

- [ ] **Step 2: Create `Areas/Identity/Pages/Account/Register.cshtml.cs`**

```csharp
using System.ComponentModel.DataAnnotations;
using BaiTapWeb_Buoi5.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace BaiTapWeb_Buoi5.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class RegisterModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public RegisterModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, StringLength(250)]
        public string Address { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = new ApplicationUser
        {
            UserName = Input.UserName,
            Email = Input.Email,
            FullName = Input.FullName,
            Address = Input.Address
        };

        var result = await _userManager.CreateAsync(user, Input.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "Member");
            await _signInManager.SignInAsync(user, isPersistent: false);
            return LocalRedirect(returnUrl);
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return Page();
    }
}
```

- [ ] **Step 3: Create `Areas/Identity/Pages/Account/Register.cshtml`**

```cshtml
@page
@model BaiTapWeb_Buoi5.Areas.Identity.Pages.Account.RegisterModel
@{
    ViewData["Title"] = "Register";
}

<section class="auth-card">
    <h1>Create account</h1>
    <form method="post">
        <div asp-validation-summary="ModelOnly" class="text-danger"></div>
        <div class="form-group">
            <label asp-for="Input.UserName"></label>
            <input asp-for="Input.UserName" class="form-control" />
            <span asp-validation-for="Input.UserName" class="text-danger"></span>
        </div>
        <div class="form-group">
            <label asp-for="Input.FullName"></label>
            <input asp-for="Input.FullName" class="form-control" />
            <span asp-validation-for="Input.FullName" class="text-danger"></span>
        </div>
        <div class="form-group">
            <label asp-for="Input.Address"></label>
            <input asp-for="Input.Address" class="form-control" />
            <span asp-validation-for="Input.Address" class="text-danger"></span>
        </div>
        <div class="form-group">
            <label asp-for="Input.Email"></label>
            <input asp-for="Input.Email" class="form-control" />
            <span asp-validation-for="Input.Email" class="text-danger"></span>
        </div>
        <div class="form-group">
            <label asp-for="Input.Password"></label>
            <input asp-for="Input.Password" class="form-control" />
            <span asp-validation-for="Input.Password" class="text-danger"></span>
        </div>
        <div class="form-group">
            <label asp-for="Input.ConfirmPassword"></label>
            <input asp-for="Input.ConfirmPassword" class="form-control" />
            <span asp-validation-for="Input.ConfirmPassword" class="text-danger"></span>
        </div>
        <button type="submit" class="button-primary">Register</button>
    </form>
</section>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

- [ ] **Step 4: Create `Views/Shared/_LoginPartial.cshtml`**

```cshtml
@using BaiTapWeb_Buoi5.Models
@using Microsoft.AspNetCore.Identity
@inject SignInManager<ApplicationUser> SignInManager
@inject UserManager<ApplicationUser> UserManager

<div class="nav-auth-links">
@if (SignInManager.IsSignedIn(User))
{
    if (User.IsInRole("Admin"))
    {
        <a asp-area="Admin" asp-controller="Product" asp-action="Index">Admin</a>
    }
    <form asp-area="Identity" asp-page="/Account/Logout" asp-route-returnUrl="@Url.Action("Index", "Products", new { area = "" })" method="post">
        <button type="submit" class="button-link">Logout</button>
    </form>
}
else
{
    <a asp-area="Identity" asp-page="/Account/Login">Login</a>
    <a asp-area="Identity" asp-page="/Account/Register">Register</a>
}
</div>
```

- [ ] **Step 5: Create `Views/Shared/_StatusMessage.cshtml`**

```cshtml
@model string

@if (!string.IsNullOrWhiteSpace(Model))
{
    <div class="status-message">@Model</div>
}
```

- [ ] **Step 6: Build to verify Identity page model compiles**

Run: `dotnet build`
Expected: custom Identity registration compiles; remaining errors should now mostly be shared layout or pending views

- [ ] **Step 7: Commit**

```bash
git add Areas/Identity Views/Shared/_LoginPartial.cshtml Views/Shared/_StatusMessage.cshtml
git commit -m "feat: customize registration for application user fields"
```

## Task 9: Build Shared Layout and Public Views

**Files:**
- Modify: `Views/Shared/_Layout.cshtml`
- Modify: `Views/Home/Index.cshtml`
- Create: `Views/Products/Index.cshtml`
- Create: `Views/Products/Details.cshtml`
- Create: `Views/Category/Index.cshtml`
- Create: `Views/Shared/AccessDenied.cshtml`
- Modify: `Views/_ViewImports.cshtml`

- [ ] **Step 1: Write the failing public UI checklist**

```text
Expected after this task:
- Shared layout follows DESIGN.md colors and spacing direction
- Public product list and detail pages render correctly
- Access denied page exists
- Shared imports cover tag helpers for MVC and Identity pages
```

- [ ] **Step 2: Update `Views/_ViewImports.cshtml`**

```cshtml
@using BaiTapWeb_Buoi5
@using BaiTapWeb_Buoi5.Models
@using BaiTapWeb_Buoi5.ViewModels
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

- [ ] **Step 3: Replace `Views/Shared/_Layout.cshtml`**

```cshtml
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - QLBanHang</title>
    <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" />
    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
    <link rel="stylesheet" href="~/BaiTapWeb_Buoi5.styles.css" asp-append-version="true" />
</head>
<body>
    <header class="top-nav">
        <div class="container-shell nav-shell">
            <a class="brand-link" asp-controller="Products" asp-action="Index">QLBanHang</a>
            <nav class="primary-nav">
                <a asp-controller="Products" asp-action="Index">Products</a>
                <a asp-controller="Category" asp-action="Index">Categories</a>
            </nav>
            <partial name="_LoginPartial" />
        </div>
    </header>

    <main class="container-shell page-shell">
        @RenderBody()
    </main>

    <footer class="site-footer">
        <div class="container-shell footer-shell">
            <span>&copy; 2026 QLBanHang</span>
            <a asp-controller="Home" asp-action="Privacy">Privacy</a>
        </div>
    </footer>

    <script src="~/lib/jquery/dist/jquery.min.js"></script>
    <script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js"></script>
    <script src="~/js/site.js" asp-append-version="true"></script>
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

- [ ] **Step 4: Replace `Views/Home/Index.cshtml` with a redirect-compatible minimal entry page**

```cshtml
@{
    ViewData["Title"] = "Home";
}

<section class="hero-panel">
    <h1>QLBanHang</h1>
    <p>Browse products and manage the catalog with the admin area.</p>
    <a asp-controller="Products" asp-action="Index" class="button-primary">View Products</a>
</section>
```

- [ ] **Step 5: Create `Views/Products/Index.cshtml`**

```cshtml
@model ProductListViewModel
@{
    ViewData["Title"] = Model.PageTitle;
}

<section class="catalog-layout">
    <aside class="catalog-sidebar">
        <h2>Categories</h2>
        <a class="category-pill @(Model.SelectedCategoryId is null ? "active" : "")" asp-controller="Products" asp-action="Index">All</a>
        @foreach (var category in Model.Categories)
        {
            <a class="category-pill @(Model.SelectedCategoryId == category.Id ? "active" : "")"
               asp-controller="Products"
               asp-action="Category"
               asp-route-categoryId="@category.Id">
                @category.Name
            </a>
        }
    </aside>

    <section class="catalog-content">
        <div class="section-header">
            <h1>@Model.PageTitle</h1>
            <p>@Model.Products.Count() products available</p>
        </div>

        <div class="product-grid">
            @foreach (var product in Model.Products)
            {
                var primaryImage = product.ProductImages.FirstOrDefault(image => image.IsPrimary)?.ImageUrl
                    ?? product.ProductImages.FirstOrDefault()?.ImageUrl
                    ?? "images/placeholder-product.png";

                <article class="product-card">
                    <img src="~/@primaryImage" alt="@product.Name" class="product-card-image" />
                    <div class="product-card-body">
                        <p class="product-category">@product.Category?.Name</p>
                        <h3>@product.Name</h3>
                        <p class="product-price">@product.Price.ToString("N0") VND</p>
                        <a asp-controller="Products" asp-action="Details" asp-route-id="@product.Id" class="button-secondary">View details</a>
                    </div>
                </article>
            }
        </div>
    </section>
</section>
```

- [ ] **Step 6: Create `Views/Products/Details.cshtml`**

```cshtml
@model ProductDetailsViewModel
@{
    ViewData["Title"] = Model.Product.Name;
    var primaryImage = Model.Product.ProductImages.FirstOrDefault(image => image.IsPrimary)?.ImageUrl
        ?? Model.Product.ProductImages.FirstOrDefault()?.ImageUrl;
}

<section class="product-detail">
    <div class="product-gallery">
        @if (!string.IsNullOrWhiteSpace(primaryImage))
        {
            <img src="~/@primaryImage" alt="@Model.Product.Name" class="product-detail-primary-image" />
        }

        <div class="product-thumbnail-grid">
            @foreach (var image in Model.Product.ProductImages)
            {
                <img src="~/@image.ImageUrl" alt="@Model.Product.Name" class="product-thumbnail" />
            }
        </div>
    </div>

    <div class="product-info-card">
        <p class="product-category">@Model.Product.Category?.Name</p>
        <h1>@Model.Product.Name</h1>
        <p class="product-price">@Model.Product.Price.ToString("N0") VND</p>
        <p class="product-description">@Model.Product.Description</p>
    </div>
</section>
```

- [ ] **Step 7: Create `Views/Category/Index.cshtml`**

```cshtml
@model IEnumerable<Category>
@{
    ViewData["Title"] = "Categories";
}

<section class="simple-list-page">
    <div class="section-header">
        <h1>Categories</h1>
    </div>
    <div class="simple-card-grid">
        @foreach (var category in Model)
        {
            <article class="simple-card">
                <h3>@category.Name</h3>
                <p>@category.Description</p>
                <a asp-controller="Products" asp-action="Category" asp-route-categoryId="@category.Id" class="button-secondary">View products</a>
            </article>
        }
    </div>
</section>
```

- [ ] **Step 8: Create `Views/Shared/AccessDenied.cshtml`**

```cshtml
@{
    ViewData["Title"] = "Access Denied";
}

<section class="auth-card">
    <h1>Access denied</h1>
    <p>You do not have permission to access this area.</p>
    <a asp-controller="Products" asp-action="Index" class="button-primary">Back to products</a>
</section>
```

- [ ] **Step 9: Build to verify public Razor views**

Run: `dotnet build`
Expected: public site and shared layout compile; remaining issues should be admin views or CSS-only gaps

- [ ] **Step 10: Commit**

```bash
git add Views/Shared/_Layout.cshtml Views/Home/Index.cshtml Views/Products Views/Category Views/Shared/AccessDenied.cshtml Views/_ViewImports.cshtml
git commit -m "feat: add public ecommerce views"
```

## Task 10: Build Admin Layout and CRUD Views

**Files:**
- Create: `Areas/Admin/Views/_ViewImports.cshtml`
- Create: `Areas/Admin/Views/_ViewStart.cshtml`
- Create: `Areas/Admin/Views/Shared/_AdminLayout.cshtml`
- Create: `Areas/Admin/Views/Product/Index.cshtml`
- Create: `Areas/Admin/Views/Product/Create.cshtml`
- Create: `Areas/Admin/Views/Product/Edit.cshtml`
- Create: `Areas/Admin/Views/Product/Details.cshtml`
- Create: `Areas/Admin/Views/Product/Delete.cshtml`
- Create: `Areas/Admin/Views/Category/Index.cshtml`
- Create: `Areas/Admin/Views/Category/Create.cshtml`
- Create: `Areas/Admin/Views/Category/Edit.cshtml`
- Create: `Areas/Admin/Views/Category/Delete.cshtml`

- [ ] **Step 1: Write the failing admin view checklist**

```text
Expected after this task:
- Admin area uses its own layout
- Admin can manage products and categories via functional Razor views
- Category delete page includes warning about deleting child products
```

- [ ] **Step 2: Create admin view bootstrap files**

```cshtml
// Areas/Admin/Views/_ViewImports.cshtml
@using BaiTapWeb_Buoi5
@using BaiTapWeb_Buoi5.Models
@using BaiTapWeb_Buoi5.ViewModels
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

```cshtml
// Areas/Admin/Views/_ViewStart.cshtml
@{
    Layout = "_AdminLayout";
}
```

- [ ] **Step 3: Create `Areas/Admin/Views/Shared/_AdminLayout.cshtml`**

```cshtml
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - Admin QLBanHang</title>
    <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" />
    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
</head>
<body>
    <header class="top-nav admin-nav">
        <div class="container-shell nav-shell">
            <a class="brand-link" asp-area="Admin" asp-controller="Product" asp-action="Index">Admin Panel</a>
            <nav class="primary-nav">
                <a asp-area="Admin" asp-controller="Product" asp-action="Index">Products</a>
                <a asp-area="Admin" asp-controller="Category" asp-action="Index">Categories</a>
                <a asp-area="" asp-controller="Products" asp-action="Index">Public site</a>
            </nav>
        </div>
    </header>

    <main class="container-shell page-shell">
        @RenderBody()
    </main>

    <script src="~/lib/jquery/dist/jquery.min.js"></script>
    <script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js"></script>
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

- [ ] **Step 4: Create admin product index view**

```cshtml
@model IEnumerable<Product>
@{
    ViewData["Title"] = "Manage Products";
}

<section class="admin-page">
    <div class="section-header">
        <h1>Products</h1>
        <a asp-action="Create" class="button-primary">Add product</a>
    </div>
    <table class="admin-table">
        <thead>
            <tr>
                <th>Name</th>
                <th>Category</th>
                <th>Price</th>
                <th>Images</th>
                <th></th>
            </tr>
        </thead>
        <tbody>
        @foreach (var product in Model)
        {
            <tr>
                <td>@product.Name</td>
                <td>@product.Category?.Name</td>
                <td>@product.Price.ToString("N0") VND</td>
                <td>@product.ProductImages.Count</td>
                <td class="admin-actions">
                    <a asp-action="Details" asp-route-id="@product.Id">Details</a>
                    <a asp-action="Edit" asp-route-id="@product.Id">Edit</a>
                    <a asp-action="Delete" asp-route-id="@product.Id" class="text-danger">Delete</a>
                </td>
            </tr>
        }
        </tbody>
    </table>
</section>
```

- [ ] **Step 5: Create admin product form and detail/delete views**

```cshtml
// Areas/Admin/Views/Product/Create.cshtml and Edit.cshtml
@model ProductFormViewModel
@{
    var title = ViewContext.RouteData.Values["action"]?.ToString() == "Create" ? "Add Product" : "Edit Product";
    ViewData["Title"] = title;
}

<section class="form-page">
    <div class="form-card">
        <h1>@title</h1>
        <form method="post" enctype="multipart/form-data">
            <div asp-validation-summary="ModelOnly" class="text-danger"></div>
            <div class="form-group">
                <label asp-for="Name"></label>
                <input asp-for="Name" class="form-control" />
                <span asp-validation-for="Name" class="text-danger"></span>
            </div>
            <div class="form-group">
                <label asp-for="Price"></label>
                <input asp-for="Price" class="form-control" />
                <span asp-validation-for="Price" class="text-danger"></span>
            </div>
            <div class="form-group">
                <label asp-for="Description"></label>
                <textarea asp-for="Description" class="form-control" rows="5"></textarea>
                <span asp-validation-for="Description" class="text-danger"></span>
            </div>
            <div class="form-group">
                <label asp-for="CategoryId"></label>
                <select asp-for="CategoryId" asp-items="Model.Categories" class="form-select"></select>
                <span asp-validation-for="CategoryId" class="text-danger"></span>
            </div>
            <div class="form-group">
                <label asp-for="ImageFiles"></label>
                <input asp-for="ImageFiles" class="form-control" multiple />
                <span asp-validation-for="ImageFiles" class="text-danger"></span>
            </div>
            @if (Model.ExistingImages.Count > 0)
            {
                <div class="thumbnail-strip">
                    @foreach (var image in Model.ExistingImages)
                    {
                        <img src="~/@image" alt="Existing product image" class="product-thumbnail" />
                    }
                </div>
            }
            <button type="submit" class="button-primary">Save</button>
        </form>
    </div>
</section>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

```cshtml
// Areas/Admin/Views/Product/Details.cshtml
@model Product
@{
    ViewData["Title"] = "Product Details";
}

<section class="detail-page">
    <div class="detail-card">
        <h1>@Model.Name</h1>
        <p><strong>Category:</strong> @Model.Category?.Name</p>
        <p><strong>Price:</strong> @Model.Price.ToString("N0") VND</p>
        <p>@Model.Description</p>
        <div class="thumbnail-strip">
            @foreach (var image in Model.ProductImages)
            {
                <img src="~/@image.ImageUrl" alt="@Model.Name" class="product-thumbnail" />
            }
        </div>
    </div>
</section>
```

```cshtml
// Areas/Admin/Views/Product/Delete.cshtml
@model Product
@{
    ViewData["Title"] = "Delete Product";
}

<section class="form-page">
    <div class="form-card danger-card">
        <h1>Delete product</h1>
        <p>Are you sure you want to delete <strong>@Model.Name</strong>?</p>
        <form method="post">
            <button type="submit" class="button-danger">Delete</button>
        </form>
    </div>
</section>
```

- [ ] **Step 6: Create admin category views including delete warning**

```cshtml
// Areas/Admin/Views/Category/Index.cshtml
@model IEnumerable<Category>
@{
    ViewData["Title"] = "Manage Categories";
}

<section class="admin-page">
    <div class="section-header">
        <h1>Categories</h1>
        <a asp-action="Create" class="button-primary">Add category</a>
    </div>
    <table class="admin-table">
        <thead>
            <tr>
                <th>Name</th>
                <th>Description</th>
                <th>Products</th>
                <th></th>
            </tr>
        </thead>
        <tbody>
        @foreach (var category in Model)
        {
            <tr>
                <td>@category.Name</td>
                <td>@category.Description</td>
                <td>@category.Products.Count</td>
                <td class="admin-actions">
                    <a asp-action="Edit" asp-route-id="@category.Id">Edit</a>
                    <a asp-action="Delete" asp-route-id="@category.Id" class="text-danger">Delete</a>
                </td>
            </tr>
        }
        </tbody>
    </table>
</section>
```

```cshtml
// Areas/Admin/Views/Category/Create.cshtml and Edit.cshtml
@model CategoryFormViewModel
@{
    var title = ViewContext.RouteData.Values["action"]?.ToString() == "Create" ? "Add Category" : "Edit Category";
    ViewData["Title"] = title;
}

<section class="form-page">
    <div class="form-card">
        <h1>@title</h1>
        <form method="post">
            <div asp-validation-summary="ModelOnly" class="text-danger"></div>
            <div class="form-group">
                <label asp-for="Name"></label>
                <input asp-for="Name" class="form-control" />
                <span asp-validation-for="Name" class="text-danger"></span>
            </div>
            <div class="form-group">
                <label asp-for="Description"></label>
                <textarea asp-for="Description" class="form-control" rows="4"></textarea>
                <span asp-validation-for="Description" class="text-danger"></span>
            </div>
            <button type="submit" class="button-primary">Save</button>
        </form>
    </div>
</section>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

```cshtml
// Areas/Admin/Views/Category/Delete.cshtml
@model Category
@{
    ViewData["Title"] = "Delete Category";
}

<section class="form-page">
    <div class="form-card danger-card">
        <h1>Delete category</h1>
        <p>You are about to delete <strong>@Model.Name</strong>.</p>
        <p>All products in this category will also be deleted.</p>
        <p>Products affected: <strong>@Model.Products.Count</strong></p>
        <form method="post">
            <button type="submit" class="button-danger">Confirm delete</button>
        </form>
    </div>
</section>
```

- [ ] **Step 7: Build to verify admin area Razor compilation**

Run: `dotnet build`
Expected: all MVC and admin views compile; remaining issues should be CSS or migrations only

- [ ] **Step 8: Commit**

```bash
git add Areas/Admin/Views
git commit -m "feat: add admin area views"
```

## Task 11: Replace Site CSS to Match `DESIGN.md`

**Files:**
- Modify: `wwwroot/css/site.css`

- [ ] **Step 1: Write the failing design checklist**

```text
Expected after this task:
- Gradient/neon demo styling is gone
- White canvas, near-black text, and Rausch accent are applied
- Public and admin screens share consistent spacing, buttons, cards, and forms
```

- [ ] **Step 2: Replace `wwwroot/css/site.css` with the new design system**

```css
:root {
  --color-primary: #ff385c;
  --color-primary-active: #e00b41;
  --color-canvas: #ffffff;
  --color-surface-soft: #f7f7f7;
  --color-hairline: #dddddd;
  --color-ink: #222222;
  --color-muted: #6a6a6a;
  --color-danger: #c13515;
  --shadow-soft: rgba(0, 0, 0, 0.02) 0 0 0 1px, rgba(0, 0, 0, 0.04) 0 2px 6px 0, rgba(0, 0, 0, 0.10) 0 4px 8px 0;
  --radius-sm: 8px;
  --radius-md: 14px;
  --radius-full: 999px;
  --container-width: 1280px;
  --font-body: "Inter", -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
}

* { box-sizing: border-box; }
html { font-size: 16px; min-height: 100%; }
body {
  margin: 0;
  min-height: 100vh;
  font-family: var(--font-body);
  color: var(--color-ink);
  background: var(--color-canvas);
}
a { color: inherit; text-decoration: none; }
img { display: block; max-width: 100%; }

.container-shell {
  width: min(100% - 32px, var(--container-width));
  margin: 0 auto;
}

.top-nav {
  background: var(--color-canvas);
  border-bottom: 1px solid var(--color-hairline);
}

.nav-shell,
.footer-shell {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  min-height: 80px;
}

.brand-link {
  font-size: 1.5rem;
  font-weight: 700;
  color: var(--color-primary);
}

.primary-nav,
.nav-auth-links {
  display: flex;
  align-items: center;
  gap: 12px;
}

.primary-nav a,
.nav-auth-links a,
.button-link {
  padding: 10px 16px;
  border-radius: var(--radius-full);
  font-weight: 500;
}

.page-shell {
  padding: 32px 0 64px;
}

.site-footer {
  border-top: 1px solid var(--color-hairline);
  margin-top: 48px;
}

.catalog-layout {
  display: grid;
  grid-template-columns: 260px minmax(0, 1fr);
  gap: 24px;
}

.catalog-sidebar,
.product-info-card,
.form-card,
.detail-card,
.simple-card,
.auth-card {
  background: var(--color-canvas);
  border: 1px solid var(--color-hairline);
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-soft);
}

.catalog-sidebar {
  padding: 24px;
  position: sticky;
  top: 100px;
  height: fit-content;
}

.category-pill {
  display: block;
  margin-bottom: 10px;
  padding: 12px 16px;
  border-radius: var(--radius-full);
  background: var(--color-surface-soft);
}

.category-pill.active,
.button-primary {
  background: var(--color-primary);
  color: #fff;
}

.catalog-content,
.admin-page,
.form-page,
.detail-page,
.simple-list-page {
  display: grid;
  gap: 24px;
}

.section-header {
  display: flex;
  align-items: end;
  justify-content: space-between;
  gap: 16px;
}

.section-header h1,
.form-card h1,
.detail-card h1,
.auth-card h1,
.hero-panel h1 {
  margin: 0;
  font-size: 2rem;
  line-height: 1.2;
}

.product-grid,
.simple-card-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: 24px;
}

.product-card {
  overflow: hidden;
  background: var(--color-canvas);
  border-radius: var(--radius-md);
}

.product-card-image {
  width: 100%;
  aspect-ratio: 1 / 1;
  object-fit: cover;
  border-radius: var(--radius-md);
}

.product-card-body {
  padding: 16px 4px 0;
}

.product-category {
  color: var(--color-muted);
  font-size: 0.9rem;
}

.product-price {
  font-size: 1.1rem;
  font-weight: 600;
}

.button-primary,
.button-secondary,
.button-danger {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-height: 48px;
  padding: 0 24px;
  border: 1px solid transparent;
  border-radius: var(--radius-sm);
  font-weight: 500;
}

.button-secondary {
  background: var(--color-canvas);
  border-color: var(--color-ink);
}

.button-danger {
  background: var(--color-danger);
  color: #fff;
}

.product-detail {
  display: grid;
  grid-template-columns: minmax(0, 1.2fr) minmax(320px, 0.8fr);
  gap: 32px;
}

.product-detail-primary-image {
  width: 100%;
  aspect-ratio: 1 / 1;
  object-fit: cover;
  border-radius: var(--radius-md);
}

.product-thumbnail-grid,
.thumbnail-strip {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(90px, 1fr));
  gap: 12px;
  margin-top: 16px;
}

.product-thumbnail {
  width: 100%;
  aspect-ratio: 1 / 1;
  object-fit: cover;
  border-radius: var(--radius-sm);
}

.product-info-card,
.form-card,
.detail-card,
.auth-card,
.simple-card {
  padding: 24px;
}

.hero-panel {
  padding: 48px 0;
  display: grid;
  gap: 16px;
}

.admin-table {
  width: 100%;
  border-collapse: collapse;
  background: var(--color-canvas);
  border: 1px solid var(--color-hairline);
  border-radius: var(--radius-md);
  overflow: hidden;
}

.admin-table th,
.admin-table td {
  padding: 16px;
  border-bottom: 1px solid var(--color-hairline);
  text-align: left;
}

.admin-actions {
  display: flex;
  gap: 12px;
}

.form-group {
  margin-bottom: 16px;
}

.form-control,
.form-select {
  min-height: 56px;
  border-radius: var(--radius-sm);
  border: 1px solid var(--color-hairline);
  padding: 12px 14px;
}

.danger-card {
  border-color: rgba(193, 53, 21, 0.25);
}

@media (max-width: 991.98px) {
  .catalog-layout,
  .product-detail {
    grid-template-columns: 1fr;
  }

  .catalog-sidebar {
    position: static;
  }

  .nav-shell,
  .footer-shell,
  .section-header {
    flex-direction: column;
    align-items: flex-start;
  }
}
```

- [ ] **Step 3: Build to ensure CSS replacement does not affect Razor compilation**

Run: `dotnet build`
Expected: build succeeds or only shows pending migration/db issues, not CSS-related compile errors

- [ ] **Step 4: Commit**

```bash
git add wwwroot/css/site.css
git commit -m "style: apply ecommerce design system"
```

## Task 12: Create Fresh Migration and Apply Database

**Files:**
- Create: `Migrations/<timestamp>_InitialECommerceIdentity.cs`
- Create: `Migrations/<timestamp>_InitialECommerceIdentity.Designer.cs`
- Create: `Migrations/ApplicationDbContextModelSnapshot.cs`

- [ ] **Step 1: Write the failing database checklist**

```text
Expected after this task:
- Old migration files are gone
- New migration contains Product, Category, ProductImage, and Identity tables
- QLBanHang database is created/updated in Docker SQL Server
```

- [ ] **Step 2: Delete old migration files if still present**

Run: `rm -f Migrations/*.cs`
Expected: old migration source files removed

- [ ] **Step 3: Start SQL Server in Docker**

Run: `docker compose up -d`
Expected: `qlbanhang_sqlserver` container starts successfully

- [ ] **Step 4: Wait until SQL Server is ready**

Run: `docker compose ps`
Expected: sqlserver service is listed as running

- [ ] **Step 5: Create the fresh migration**

Run: `dotnet ef migrations add InitialECommerceIdentity`
Expected: EF creates three migration files in `Migrations/`

- [ ] **Step 6: Apply the migration to `QLBanHang`**

Run: `dotnet ef database update`
Expected: database update succeeds and tables are created in SQL Server

- [ ] **Step 7: Build again after migration**

Run: `dotnet build`
Expected: PASS with 0 errors

- [ ] **Step 8: Commit**

```bash
git add Migrations
git commit -m "feat: add initial ecommerce identity migration"
```

## Task 13: Verify End-to-End Behavior

**Files:**
- No code changes expected unless fixes are required

- [ ] **Step 1: Run the application**

Run: `dotnet run`
Expected: app starts and binds to a local HTTP/HTTPS port

- [ ] **Step 2: Verify public routes manually**

Run: `curl -I http://localhost:5000/Products`
Expected: `HTTP/1.1 200 OK` or local redirect to active port if different

- [ ] **Step 3: Verify seeded admin exists through the database**

Run: `docker exec -it qlbanhang_sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 123456 -C -Q "SELECT Name FROM QLBanHang.dbo.AspNetRoles; SELECT UserName, FullName, Address FROM QLBanHang.dbo.AspNetUsers;"`
Expected: rows for `Admin`, `Member`, and seeded users

- [ ] **Step 4: Verify business tables exist**

Run: `docker exec -it qlbanhang_sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 123456 -C -Q "SELECT TABLE_NAME FROM QLBanHang.INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE';"`
Expected: includes `Categories`, `Products`, `ProductImages`, `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`

- [ ] **Step 5: Verify authorization behavior manually**

```text
Manual checks:
- anonymous user can open /Products and /Products/Details/{id}
- anonymous or member user cannot open /Admin/Product
- admin user can log in and access /Admin/Product and /Admin/Category
- category delete page warns that child products will be deleted
```

- [ ] **Step 6: If verification reveals issues, fix them and rerun verification**

Run: `dotnet build && dotnet ef database update`
Expected: all fixes compile and database remains valid

- [ ] **Step 7: Commit**

```bash
git add -A
git commit -m "test: verify ecommerce application end to end"
```

## Task 14: Prepare Delivery Notes

**Files:**
- No code changes required unless adding a short local note file is preferred

- [ ] **Step 1: Gather the final change summary**

```text
Include:
- files deleted from old Book demo
- files added for ecommerce, Identity, repositories, admin area, and views
- Docker SQL Server setup summary
- seed accounts summary
```

- [ ] **Step 2: Gather the final run commands**

```bash
docker compose up -d
dotnet ef migrations add InitialECommerceIdentity
dotnet ef database update
dotnet run
```

- [ ] **Step 3: Gather the default seeded accounts**

```text
Admin:
- username: admin
- password: Admin@123

Member:
- username: member
- password: Member@123
```

- [ ] **Step 4: Confirm the final build result**

Run: `dotnet build`
Expected: PASS with 0 errors

- [ ] **Step 5: Final commit**

```bash
git add -A
git commit -m "docs: prepare ecommerce delivery notes"
```

## Self-Review

Spec coverage check:

- project cleanup: covered in Tasks 2 and 5
- Identity + `ApplicationUser`: covered in Tasks 2, 4, and 8
- repository pattern + async: covered in Task 3
- admin area with role restriction: covered in Task 7
- public product browsing: covered in Task 6 and Task 9
- design alignment with `DESIGN.md`: covered in Task 11
- SQL Server Docker + connection string: covered in Task 1 and Task 12
- migration + database update: covered in Task 12
- seed roles and users: covered in Task 4 and Task 13
- category delete cascade-safe behavior: covered in Task 3 and Task 10

Placeholder scan:

- no `TODO`, `TBD`, or vague “handle appropriately” steps remain
- all tasks list exact file paths and commands

Type consistency:

- entity names are consistently `Category`, `Product`, `ProductImage`, `ApplicationUser`
- repository/controller/viewmodel names align with the approved design
