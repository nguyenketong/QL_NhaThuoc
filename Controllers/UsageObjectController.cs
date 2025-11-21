using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_NhaThuoc.Data;

namespace QL_NhaThuoc.Controllers
{
    public class UsageObjectController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsageObjectController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var usageObjects = await _context.UsageObjects.ToListAsync();
            return View(usageObjects);
        }
    }
}
