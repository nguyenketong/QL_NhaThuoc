using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_NhaThuoc.Data;
using QL_NhaThuoc.Filters;

namespace QL_NhaThuoc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminAuthorize]
    public class NguoiDungController : Controller
    {
        private readonly QL_NhaThuocDbContext _context;

        public NguoiDungController(QL_NhaThuocDbContext context)
        {
            _context = context;
        }

        // GET: Admin/NguoiDung - EF + LINQ
        public async Task<IActionResult> Index()
        {
            var danhSach = await _context.NGUOI_DUNG
                .Select(nd => new
                {
                    nd.MaNguoiDung,
                    nd.HoTen,
                    nd.SoDienThoai,
                    nd.DiaChi,
                    nd.VaiTro,
                    nd.NgayTao,
                    SoDonHang = nd.DonHangs != null ? nd.DonHangs.Count : 0,
                    TongChiTieu = nd.DonHangs != null 
                        ? nd.DonHangs.Where(d => d.TrangThai == "Hoan thanh").Sum(d => d.TongTien ?? 0) 
                        : 0
                })
                .OrderByDescending(nd => nd.NgayTao)
                .ToListAsync();

            return View(danhSach);
        }

        // GET: Admin/NguoiDung/Details/5 - EF + LINQ
        public async Task<IActionResult> Details(int id)
        {
            var nguoiDung = await _context.NGUOI_DUNG
                .Include(nd => nd.DonHangs!)
                    .ThenInclude(dh => dh.ChiTietDonHangs)
                .FirstOrDefaultAsync(nd => nd.MaNguoiDung == id);

            if (nguoiDung == null)
                return NotFound();

            return View(nguoiDung);
        }

        // POST: Admin/NguoiDung/CapNhatVaiTro - EF + LINQ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatVaiTro(int id, string vaiTro)
        {
            var nguoiDung = await _context.NGUOI_DUNG.FindAsync(id);
            if (nguoiDung != null)
            {
                nguoiDung.VaiTro = vaiTro;
                await _context.SaveChangesAsync();
                TempData["ThongBao"] = "Cập nhật vai trò thành công!";
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Admin/NguoiDung/Delete/5 - EF + LINQ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var nguoiDung = await _context.NGUOI_DUNG.FindAsync(id);
            if (nguoiDung != null)
            {
                _context.NGUOI_DUNG.Remove(nguoiDung);
                await _context.SaveChangesAsync();
                TempData["ThongBao"] = "Xóa người dùng thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
