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
