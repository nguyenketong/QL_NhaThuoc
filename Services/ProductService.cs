using Microsoft.EntityFrameworkCore;
using QL_NhaThuoc.Data;
using QL_NhaThuoc.Models;

namespace QL_NhaThuoc.Services
{
    public class ProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Country)
                .ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Country)
                .Include(p => p.UsageObject)
                .Include(p => p.ProductIngredients!)
                    .ThenInclude(pi => pi.Ingredient)
                .Include(p => p.ProductSideEffects!)
                    .ThenInclude(ps => ps.SideEffect)
                .FirstOrDefaultAsync(p => p.ProductId == id);
        }

        public async Task<List<Product>> SearchProductsAsync(string keyword)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.Name.Contains(keyword) || p.Description!.Contains(keyword))
                .ToListAsync();
        }
    }
}
