using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_NhaThuoc.Data;
using QL_NhaThuoc.Filters;
using QL_NhaThuoc.Models;

namespace QL_NhaThuoc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminAuthorize]
    public class NhomThuocController : Controller
    {
        private readonly QL_NhaThuocDbContext _context;

        public NhomThuocController(QL_NhaThuocDbContext context)
        {
            _context = context;
        }

        // GET: Admin/NhomThuoc - EF + LINQ
        public async Task<IActionResult> Index()
        {
            var danhSach = await _context.NHOM_THUOC
                .Include(n => n.DanhMucCha)
                .Select(n => new
                {
                    n.MaNhomThuoc,
                    n.TenNhomThuoc,
                    n.MoTa,
                    n.MaDanhMucCha,
                    TenDanhMucCha = n.DanhMucCha != null ? n.DanhMucCha.TenNhomThuoc : null,
                    SoLuongThuoc = n.Thuocs != null ? n.Thuocs.Count : 0
                })
                .OrderBy(n => n.MaDanhMucCha == null ? 0 : 1)
                .ThenBy(n => n.TenNhomThuoc)
                .ToListAsync();

            return View(danhSach);
        }

        // GET: Admin/NhomThuoc/Create
        public async Task<IActionResult> Create()
        {
            // Load danh sách danh mục cha (chỉ lấy danh mục gốc)
            ViewBag.DanhMucChaList = await _context.NHOM_THUOC
                .Where(n => n.MaDanhMucCha == null)
                .OrderBy(n => n.TenNhomThuoc)
                .ToListAsync();
            return View();
        }

        // POST: Admin/NhomThuoc/Create - EF + LINQ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NhomThuoc nhomThuoc)
        {
            if (ModelState.IsValid)
            {
                _context.NHOM_THUOC.Add(nhomThuoc);
                await _context.SaveChangesAsync();
                TempData["ThongBao"] = "Thêm nhóm thuốc thành công!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.DanhMucChaList = await _context.NHOM_THUOC
                .Where(n => n.MaDanhMucCha == null)
                .OrderBy(n => n.TenNhomThuoc)
                .ToListAsync();
            return View(nhomThuoc);
        }

        // GET: Admin/NhomThuoc/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var nhomThuoc = await _context.NHOM_THUOC.FindAsync(id);
            if (nhomThuoc == null)
                return NotFound();

            // Load danh sách danh mục cha (không bao gồm chính nó và các danh mục con của nó)
            ViewBag.DanhMucChaList = await _context.NHOM_THUOC
                .Where(n => n.MaDanhMucCha == null && n.MaNhomThuoc != id)
                .OrderBy(n => n.TenNhomThuoc)
                .ToListAsync();
            return View(nhomThuoc);
        }

        // POST: Admin/NhomThuoc/Edit/5 - EF + LINQ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NhomThuoc nhomThuoc)
        {
            if (id != nhomThuoc.MaNhomThuoc)
                return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(nhomThuoc);
                await _context.SaveChangesAsync();
                TempData["ThongBao"] = "Cập nhật nhóm thuốc thành công!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.DanhMucChaList = await _context.NHOM_THUOC
                .Where(n => n.MaDanhMucCha == null && n.MaNhomThuoc != id)
                .OrderBy(n => n.TenNhomThuoc)
                .ToListAsync();
            return View(nhomThuoc);
        }

        // POST: Admin/NhomThuoc/Delete/5 - EF + LINQ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var nhomThuoc = await _context.NHOM_THUOC.FindAsync(id);
            if (nhomThuoc != null)
            {
                _context.NHOM_THUOC.Remove(nhomThuoc);
                await _context.SaveChangesAsync();
                TempData["ThongBao"] = "Xóa nhóm thuốc thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
