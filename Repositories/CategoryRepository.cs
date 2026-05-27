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
