using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_NhaThuoc.Data;

namespace QL_NhaThuoc.Controllers
{
    public class IngredientController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IngredientController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var ingredients = await _context.Ingredients.ToListAsync();
            return View(ingredients);
        }

        public async Task<IActionResult> Details(int id)
        {
            var ingredient = await _context.Ingredients
                .Include(i => i.ProductIngredients!)
                    .ThenInclude(pi => pi.Product)
                .FirstOrDefaultAsync(i => i.IngredientId == id);

            if (ingredient == null)
                return NotFound();

            return View(ingredient);
        }
    }
}
