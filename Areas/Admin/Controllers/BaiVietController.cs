using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_NhaThuoc.Data;
using QL_NhaThuoc.Filters;
using QL_NhaThuoc.Models;

namespace QL_NhaThuoc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminAuthorize]
    public class BaiVietController : Controller
    {
        private readonly QL_NhaThuocDbContext _context;
        private readonly IWebHostEnvironment _env;

        public BaiVietController(QL_NhaThuocDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Admin/BaiViet
        public async Task<IActionResult> Index()
        {
            var danhSach = await _context.BAI_VIET
                .OrderByDescending(b => b.NgayDang)
                .ToListAsync();
            return View(danhSach);
        }

        // GET: Admin/BaiViet/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/BaiViet/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BaiViet baiViet, IFormFile? hinhAnhFile)
        {
            if (ModelState.IsValid)
            {
                // Upload hình ảnh
                if (hinhAnhFile != null && hinhAnhFile.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(hinhAnhFile.FileName);
                    var filePath = Path.Combine(_env.WebRootPath, "images", "baiviet", fileName);
                    
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await hinhAnhFile.CopyToAsync(stream);
                    }
                    baiViet.HinhAnh = "/images/baiviet/" + fileName;
                }

                baiViet.NgayDang = DateTime.Now;
                baiViet.LuotXem = 0;
                baiViet.IsActive = true;

                _context.BAI_VIET.Add(baiViet);
                await _context.SaveChangesAsync();

                TempData["ThongBao"] = "Thêm bài viết thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(baiViet);
        }

        // GET: Admin/BaiViet/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var baiViet = await _context.BAI_VIET.FindAsync(id);
            if (baiViet == null)
                return NotFound();
            return View(baiViet);
        }

        // POST: Admin/BaiViet/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BaiViet baiViet, IFormFile? hinhAnhFile)
        {
            if (id != baiViet.MaBaiViet)
                return NotFound();

            if (ModelState.IsValid)
            {
                // Upload hình ảnh mới
                if (hinhAnhFile != null && hinhAnhFile.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(hinhAnhFile.FileName);
                    var filePath = Path.Combine(_env.WebRootPath, "images", "baiviet", fileName);
                    
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await hinhAnhFile.CopyToAsync(stream);
                    }
                    baiViet.HinhAnh = "/images/baiviet/" + fileName;
                }

                _context.Update(baiViet);
                await _context.SaveChangesAsync();

                TempData["ThongBao"] = "Cập nhật bài viết thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(baiViet);
        }

        // POST: Admin/BaiViet/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var baiViet = await _context.BAI_VIET.FindAsync(id);
            if (baiViet != null)
            {
                _context.BAI_VIET.Remove(baiViet);
                await _context.SaveChangesAsync();
                TempData["ThongBao"] = "Xóa bài viết thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Toggle nổi bật
        [HttpPost]
        public async Task<IActionResult> ToggleNoiBat(int id)
        {
            var baiViet = await _context.BAI_VIET.FindAsync(id);
            if (baiViet != null)
            {
                baiViet.IsNoiBat = !(baiViet.IsNoiBat ?? false);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Toggle active
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var baiViet = await _context.BAI_VIET.FindAsync(id);
            if (baiViet != null)
            {
                baiViet.IsActive = !(baiViet.IsActive ?? false);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
