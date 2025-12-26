using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_NhaThuoc.Data;

namespace QL_NhaThuoc.Controllers
{
    public class ThuongHieuController : Controller
    {
        private readonly QL_NhaThuocDbContext _context;

        public ThuongHieuController(QL_NhaThuocDbContext context)
        {
            _context = context;
        }

        // GET: ThuongHieu/DanhSach
        public async Task<IActionResult> DanhSach()
        {
            var danhSach = await _context.THUONG_HIEU
                .OrderBy(th => th.TenThuongHieu)
                .ToListAsync();

            return View(danhSach);
        }

        // GET: ThuongHieu/ChiTiet/5 - Hiển thị sản phẩm theo thương hiệu
        public async Task<IActionResult> ChiTiet(int id, int page = 1, string? sapXep = null)
        {
            var thuongHieu = await _context.THUONG_HIEU
                .FirstOrDefaultAsync(th => th.MaThuongHieu == id);

            if (thuongHieu == null)
            {
                TempData["Error"] = "Không tìm thấy thương hiệu này!";
                return RedirectToAction("DanhSach");
            }

            // Load sản phẩm của thương hiệu
            var allThuocs = await _context.THUOC
                .Include(t => t.NhomThuoc)
                .Where(t => t.MaThuongHieu == id)
                .ToListAsync();

            // Sắp xếp
            var sortedThuocs = sapXep switch
            {
                "gia-tang" => allThuocs.OrderBy(t => t.GiaBan).ToList(),
                "gia-giam" => allThuocs.OrderByDescending(t => t.GiaBan).ToList(),
                "ten-az" => allThuocs.OrderBy(t => t.TenThuoc).ToList(),
                "ten-za" => allThuocs.OrderByDescending(t => t.TenThuoc).ToList(),
                "moi-nhat" => allThuocs.OrderByDescending(t => t.NgayTao).ToList(),
                _ => allThuocs.OrderByDescending(t => t.MaThuoc).ToList()
            };

            // Phân trang
            int pageSize = 12;
            int totalItems = sortedThuocs.Count;
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var thuocs = sortedThuocs
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            thuongHieu.Thuocs = thuocs;

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.SapXep = sapXep;

            return View(thuongHieu);
        }
    }
}
