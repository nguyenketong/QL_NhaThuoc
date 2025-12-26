using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_NhaThuoc.Data;
using QL_NhaThuoc.Filters;
using QL_NhaThuoc.Models;

namespace QL_NhaThuoc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminAuthorize]
    public class DoiTuongSuDungController : Controller
    {
        private readonly QL_NhaThuocDbContext _context;

        public DoiTuongSuDungController(QL_NhaThuocDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var danhSach = await _context.DOI_TUONG_SU_DUNG.OrderBy(d => d.TenDoiTuong).ToListAsync();
            return View(danhSach);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DoiTuongSuDung doiTuong)
        {
            if (ModelState.IsValid)
            {
                _context.DOI_TUONG_SU_DUNG.Add(doiTuong);
                await _context.SaveChangesAsync();
                TempData["ThongBao"] = "Thêm đối tượng sử dụng thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(doiTuong);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var doiTuong = await _context.DOI_TUONG_SU_DUNG.FindAsync(id);
            return doiTuong == null ? NotFound() : View(doiTuong);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DoiTuongSuDung doiTuong)
        {
            if (id != doiTuong.MaDoiTuong) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(doiTuong);
                await _context.SaveChangesAsync();
                TempData["ThongBao"] = "Cập nhật thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(doiTuong);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var doiTuong = await _context.DOI_TUONG_SU_DUNG.FindAsync(id);
            if (doiTuong != null)
            {
                _context.DOI_TUONG_SU_DUNG.Remove(doiTuong);
                await _context.SaveChangesAsync();
                TempData["ThongBao"] = "Xóa thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
