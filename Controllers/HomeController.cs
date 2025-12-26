using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_NhaThuoc.Data;

namespace QL_NhaThuoc.Controllers
{
    public class HomeController : Controller
    {
        private readonly QL_NhaThuocDbContext _context;

        public HomeController(QL_NhaThuocDbContext context)
        {
            _context = context;
        }

        // GET: Home/Index - EF + LINQ
        public async Task<IActionResult> Index()
        {
            // Sản phẩm mới (chỉ lấy sản phẩm được đánh dấu IsNew = true)
            var sanPhamMoi = await _context.THUOC
                .Include(t => t.NhomThuoc)
                .Include(t => t.ThuongHieu)
                .Where(t => t.IsNew == true && t.IsActive != false)
                .OrderByDescending(t => t.NgayTao ?? DateTime.MinValue)
                .Take(10)
                .ToListAsync();

            // Sản phẩm Hot/Khuyến mãi (chỉ lấy sản phẩm được đánh dấu IsHot = true hoặc đang khuyến mãi)
            var sanPhamHot = await _context.THUOC
                .Include(t => t.NhomThuoc)
                .Include(t => t.ThuongHieu)
                .Where(t => t.IsActive != false && 
                    (t.IsHot == true || 
                    (t.PhanTramGiam > 0 && 
                     (t.NgayBatDauKM == null || t.NgayBatDauKM <= DateTime.Now) &&
                     (t.NgayKetThucKM == null || t.NgayKetThucKM >= DateTime.Now))))
                .OrderByDescending(t => t.PhanTramGiam ?? 0)
                .ThenByDescending(t => t.MaThuoc)
                .Take(10)
                .ToListAsync();

            // Sản phẩm bán chạy (chỉ tính đơn hàng thành công)
            var sanPhamBanChay = await _context.CHI_TIET_DON_HANG
                .Include(ct => ct.DonHang)
                .Where(ct => ct.DonHang != null && 
                    (ct.DonHang.TrangThai == "Dang giao" || ct.DonHang.TrangThai == "Hoan thanh"))
                .GroupBy(ct => ct.MaThuoc)
                .Select(g => new { MaThuoc = g.Key, TongBan = g.Sum(x => x.SoLuong) })
                .OrderByDescending(x => x.TongBan)
                .Take(10)
                .Join(_context.THUOC.Include(t => t.ThuongHieu).Where(t => t.IsActive != false),
                    ct => ct.MaThuoc,
                    t => t.MaThuoc,
                    (ct, t) => t)
                .ToListAsync();

            // Nếu chưa có đơn hàng thành công, lấy sản phẩm ngẫu nhiên
            if (!sanPhamBanChay.Any())
            {
                sanPhamBanChay = await _context.THUOC
                    .Include(t => t.ThuongHieu)
                    .Where(t => t.IsActive != false)
                    .OrderBy(t => Guid.NewGuid())
                    .Take(10)
                    .ToListAsync();
            }

            // Danh sách nhóm thuốc
            var nhomThuocs = await _context.NHOM_THUOC
                .OrderBy(n => n.TenNhomThuoc)
                .Take(8)
                .ToListAsync();

            // Danh sách thương hiệu
            var thuongHieus = await _context.THUONG_HIEU
                .OrderBy(th => th.TenThuongHieu)
                .Take(8)
                .ToListAsync();

            // Bài viết cho Góc sức khỏe
            var baiViets = await _context.BAI_VIET
                .Where(b => b.IsActive == true)
                .OrderByDescending(b => b.NgayDang)
                .Take(10)
                .ToListAsync();

            ViewBag.SanPhamMoi = sanPhamMoi;
            ViewBag.SanPhamHot = sanPhamHot;
            ViewBag.SanPhamBanChay = sanPhamBanChay;
            ViewBag.NhomThuocs = nhomThuocs;
            ViewBag.ThuongHieus = thuongHieus;
            ViewBag.BaiViets = baiViets;

            return View(sanPhamMoi);
        }

        // GET: Home/GioiThieu
        public IActionResult GioiThieu()
        {
            return View();
        }

        // GET: Home/LienHe
        public IActionResult LienHe()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
