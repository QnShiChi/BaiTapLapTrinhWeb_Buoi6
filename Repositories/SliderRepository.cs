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
        return await _context.SliderImages
            .FirstOrDefaultAsync(item => item.Id == id);
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
