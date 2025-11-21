using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_NhaThuoc.Data;

namespace QL_NhaThuoc.Controllers
{
    public class SideEffectController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SideEffectController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var sideEffects = await _context.SideEffects.ToListAsync();
            return View(sideEffects);
        }
    }
}
