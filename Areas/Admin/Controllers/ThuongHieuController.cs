using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_NhaThuoc.Data;
using QL_NhaThuoc.Filters;
using QL_NhaThuoc.Models;

namespace QL_NhaThuoc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminAuthorize]
    public class ThuongHieuController : Controller
    {
        private readonly QL_NhaThuocDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ThuongHieuController(QL_NhaThuocDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Admin/ThuongHieu - EF + LINQ
        public async Task<IActionResult> Index()
        {
            var danhSach = await _context.THUONG_HIEU
                .Select(th => new
                {
                    th.MaThuongHieu,
                    th.TenThuongHieu,
                    th.QuocGia,
                    th.DiaChi,
                    th.HinhAnh,
                    SoLuongThuoc = th.Thuocs != null ? th.Thuocs.Count : 0
                })
                .OrderBy(th => th.TenThuongHieu)
                .ToListAsync();

            return View(danhSach);
        }

        // GET: Admin/ThuongHieu/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/ThuongHieu/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ThuongHieu thuongHieu, IFormFile? LogoFile)
        {
            if (ModelState.IsValid)
            {
                // Upload logo
                if (LogoFile != null && LogoFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "brands");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(LogoFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await LogoFile.CopyToAsync(stream);
                    }
                    thuongHieu.HinhAnh = "/images/brands/" + fileName;
                }

                _context.THUONG_HIEU.Add(thuongHieu);
                await _context.SaveChangesAsync();
                TempData["ThongBao"] = "Thêm thương hiệu thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(thuongHieu);
        }

        // GET: Admin/ThuongHieu/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var thuongHieu = await _context.THUONG_HIEU.FindAsync(id);
            if (thuongHieu == null)
                return NotFound();

            return View(thuongHieu);
        }

        // POST: Admin/ThuongHieu/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ThuongHieu thuongHieu, IFormFile? LogoFile)
        {
            if (id != thuongHieu.MaThuongHieu)
                return NotFound();

            if (ModelState.IsValid)
            {
                var existing = await _context.THUONG_HIEU.AsNoTracking().FirstOrDefaultAsync(t => t.MaThuongHieu == id);
                
                // Upload logo mới
                if (LogoFile != null && LogoFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "brands");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(LogoFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await LogoFile.CopyToAsync(stream);
                    }
                    thuongHieu.HinhAnh = "/images/brands/" + fileName;
                }
                else
                {
                    thuongHieu.HinhAnh = existing?.HinhAnh;
                }

                _context.Update(thuongHieu);
                await _context.SaveChangesAsync();
                TempData["ThongBao"] = "Cập nhật thương hiệu thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(thuongHieu);
        }

        // POST: Admin/ThuongHieu/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var thuongHieu = await _context.THUONG_HIEU.FindAsync(id);
            if (thuongHieu != null)
            {
                _context.THUONG_HIEU.Remove(thuongHieu);
                await _context.SaveChangesAsync();
                TempData["ThongBao"] = "Xóa thương hiệu thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
