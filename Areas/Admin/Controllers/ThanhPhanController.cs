using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_NhaThuoc.Data;
using QL_NhaThuoc.Filters;
using QL_NhaThuoc.Models;

namespace QL_NhaThuoc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminAuthorize]
    public class ThanhPhanController : Controller
    {
        private readonly QL_NhaThuocDbContext _context;

        public ThanhPhanController(QL_NhaThuocDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var danhSach = await _context.THANH_PHAN.OrderBy(tp => tp.TenThanhPhan).ToListAsync();
            return View(danhSach);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ThanhPhan thanhPhan)
        {
            if (ModelState.IsValid)
            {
                _context.THANH_PHAN.Add(thanhPhan);
                await _context.SaveChangesAsync();
                TempData["ThongBao"] = "Thêm thành phần thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(thanhPhan);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var thanhPhan = await _context.THANH_PHAN.FindAsync(id);
            return thanhPhan == null ? NotFound() : View(thanhPhan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ThanhPhan thanhPhan)
        {
            if (id != thanhPhan.MaThanhPhan) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(thanhPhan);
                await _context.SaveChangesAsync();
                TempData["ThongBao"] = "Cập nhật thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(thanhPhan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var thanhPhan = await _context.THANH_PHAN.FindAsync(id);
            if (thanhPhan != null)
            {
                _context.THANH_PHAN.Remove(thanhPhan);
                await _context.SaveChangesAsync();
                TempData["ThongBao"] = "Xóa thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
