using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_NhaThuoc.Data;
using QL_NhaThuoc.Filters;
using QL_NhaThuoc.Models;

namespace QL_NhaThuoc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminAuthorize]
    public class TacDungPhuController : Controller
    {
        private readonly QL_NhaThuocDbContext _context;

        public TacDungPhuController(QL_NhaThuocDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var danhSach = await _context.TAC_DUNG_PHU.OrderBy(t => t.TenTacDungPhu).ToListAsync();
            return View(danhSach);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TacDungPhu tacDungPhu)
        {
            if (ModelState.IsValid)
            {
                _context.TAC_DUNG_PHU.Add(tacDungPhu);
                await _context.SaveChangesAsync();
                TempData["ThongBao"] = "Thêm tác dụng phụ thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(tacDungPhu);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var tacDungPhu = await _context.TAC_DUNG_PHU.FindAsync(id);
            return tacDungPhu == null ? NotFound() : View(tacDungPhu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TacDungPhu tacDungPhu)
        {
            if (id != tacDungPhu.MaTacDungPhu) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(tacDungPhu);
                await _context.SaveChangesAsync();
                TempData["ThongBao"] = "Cập nhật thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(tacDungPhu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var tacDungPhu = await _context.TAC_DUNG_PHU.FindAsync(id);
            if (tacDungPhu != null)
            {
                _context.TAC_DUNG_PHU.Remove(tacDungPhu);
                await _context.SaveChangesAsync();
                TempData["ThongBao"] = "Xóa thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
