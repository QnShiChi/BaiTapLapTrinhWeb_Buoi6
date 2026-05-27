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

        await SeedCatalogAsync(context);
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

    private static async Task SeedCatalogAsync(ApplicationDbContext context)
    {
        if (await context.Categories.AnyAsync() || await context.Products.AnyAsync())
        {
            return;
        }

        var categories = new List<Category>
        {
            new()
            {
                Name = "Điện tử",
                Description = "Thiết bị và phụ kiện công nghệ cho học tập và làm việc."
            },
            new()
            {
                Name = "Sách lập trình",
                Description = "Tài liệu và giáo trình thực hành cho sinh viên công nghệ thông tin."
            },
            new()
            {
                Name = "Phụ kiện bàn học",
                Description = "Những món đồ gọn gàng giúp góc học tập hiệu quả hơn."
            }
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();

        var products = new List<Product>
        {
            new()
            {
                Name = "Laptop học ASP.NET Core",
                Price = 18990000,
                Description = "Mẫu laptop cấu hình cân bằng cho lập trình web, thực hành SQL Server và chạy dự án ASP.NET Core MVC.",
                CategoryId = categories[0].Id,
                ProductImages = new List<ProductImage>
                {
                    new() { ImageUrl = "Content/ImagesBook/29ced1e3-0445-41e4-95ac-0d95d0f724c2_Screenshot from 2026-04-11 13-32-29.png", IsPrimary = true },
                    new() { ImageUrl = "Content/ImagesBook/52464692-35f8-433a-b540-fbc6a6b55769_Screenshot from 2026-04-15 13-51-07.png", IsPrimary = false }
                }
            },
            new()
            {
                Name = "Giáo trình C# nâng cao",
                Price = 145000,
                Description = "Giáo trình tổng hợp cú pháp, OOP, LINQ và Entity Framework Core cho sinh viên chuyên ngành phần mềm.",
                CategoryId = categories[1].Id,
                ProductImages = new List<ProductImage>
                {
                    new() { ImageUrl = "Content/ImagesBook/lap-trinh-csharp.svg", IsPrimary = true },
                    new() { ImageUrl = "Content/ImagesBook/java-fundamentals.svg", IsPrimary = false }
                }
            },
            new()
            {
                Name = "Bộ phụ kiện góc học tập",
                Price = 299000,
                Description = "Bộ phụ kiện gồm giá đỡ, khay để bút và đèn học tối giản theo phong cách bàn làm việc hiện đại.",
                CategoryId = categories[2].Id,
                ProductImages = new List<ProductImage>
                {
                    new() { ImageUrl = "Content/ImagesBook/cuoc-song-rat-giong-cuoc-doi.svg", IsPrimary = true },
                    new() { ImageUrl = "Content/ImagesBook/cho-toi-xin-mot-ve-di-tuoi-tho.svg", IsPrimary = false }
                }
            }
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();
    }
}
