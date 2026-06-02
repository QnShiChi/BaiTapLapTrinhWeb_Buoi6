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
