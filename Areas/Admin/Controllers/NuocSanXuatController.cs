using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_NhaThuoc.Data;
using QL_NhaThuoc.Filters;
using QL_NhaThuoc.Models;

namespace QL_NhaThuoc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminAuthorize]
    public class NuocSanXuatController : Controller
    {
        private readonly QL_NhaThuocDbContext _context;

        public NuocSanXuatController(QL_NhaThuocDbContext context)
        {
            _context = context;
        }

        // GET: Admin/NuocSanXuat
        public async Task<IActionResult> Index()
        {
            var danhSach = await _context.NUOC_SAN_XUAT
                .OrderBy(n => n.TenNuocSX)
                .Select(n => new NuocSanXuat
                {
                    MaNuocSX = n.MaNuocSX,
                    TenNuocSX = n.TenNuocSX,
                    SoLuongThuoc = n.Thuocs.Count
                })
                .ToListAsync();
            
            return View(danhSach);
        }

        // GET: Admin/NuocSanXuat/Create
        public IActionResult Create() => View();

        // POST: Admin/NuocSanXuat/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NuocSanXuat nuocSX)
        {
            if (ModelState.IsValid)
            {
                _context.NUOC_SAN_XUAT.Add(nuocSX);
                await _context.SaveChangesAsync();
                TempData["ThongBao"] = "Thêm nước sản xuất thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(nuocSX);
        }

        // GET: Admin/NuocSanXuat/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var nuocSX = await _context.NUOC_SAN_XUAT.FindAsync(id);
            return nuocSX == null ? NotFound() : View(nuocSX);
        }

        // POST: Admin/NuocSanXuat/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NuocSanXuat nuocSX)
        {
            if (id != nuocSX.MaNuocSX) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(nuocSX);
                await _context.SaveChangesAsync();
                TempData["ThongBao"] = "Cập nhật thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(nuocSX);
        }

        // POST: Admin/NuocSanXuat/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var nuocSX = await _context.NUOC_SAN_XUAT.FindAsync(id);
            if (nuocSX != null)
            {
                _context.NUOC_SAN_XUAT.Remove(nuocSX);
                await _context.SaveChangesAsync();
                TempData["ThongBao"] = "Xóa thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
