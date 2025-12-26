using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_NhaThuoc.Data;

namespace QL_NhaThuoc.Controllers
{
    public class ThuocController : Controller
    {
        private readonly QL_NhaThuocDbContext _context;

        public ThuocController(QL_NhaThuocDbContext context)
        {
            _context = context;
        }

        // GET: Thuoc/DanhSach - EF + LINQ
        public async Task<IActionResult> DanhSach(int? maNhom, int? maThuongHieu, decimal? giaMin, decimal? giaMax, string? tuKhoa)
        {
            var query = _context.THUOC
                .Include(t => t.NhomThuoc)
                .Include(t => t.ThuongHieu)
                .Include(t => t.NuocSanXuat)
                .AsQueryable();

            // Lọc theo nhóm thuốc
            if (maNhom.HasValue)
                query = query.Where(t => t.MaNhomThuoc == maNhom.Value);

            // Lọc theo thương hiệu
            if (maThuongHieu.HasValue)
                query = query.Where(t => t.MaThuongHieu == maThuongHieu.Value);

            // Lọc theo giá
            if (giaMin.HasValue)
                query = query.Where(t => t.GiaBan >= giaMin.Value);

            if (giaMax.HasValue)
                query = query.Where(t => t.GiaBan <= giaMax.Value);

            // Tìm kiếm theo từ khóa
            if (!string.IsNullOrEmpty(tuKhoa))
                query = query.Where(t => t.TenThuoc.Contains(tuKhoa) || 
                                         (t.MoTa != null && t.MoTa.Contains(tuKhoa)));

            var danhSachThuoc = await query.OrderBy(t => t.TenThuoc).ToListAsync();

            // Load danh sách nhóm và thương hiệu cho filter
            ViewBag.DanhSachNhom = await _context.NHOM_THUOC.OrderBy(n => n.TenNhomThuoc).ToListAsync();
            ViewBag.DanhSachThuongHieu = await _context.THUONG_HIEU.OrderBy(th => th.TenThuongHieu).ToListAsync();

            return View(danhSachThuoc);
        }


        // GET: Thuoc/ChiTiet/5 - EF + LINQ
        public async Task<IActionResult> ChiTiet(int id)
        {
            var thuoc = await _context.THUOC
                .Include(t => t.NhomThuoc)
                .Include(t => t.ThuongHieu)
                .Include(t => t.NuocSanXuat)
                .Include(t => t.CT_ThanhPhans!)
                    .ThenInclude(ct => ct.ThanhPhan)
                .Include(t => t.CT_TacDungPhus!)
                    .ThenInclude(ct => ct.TacDungPhu)
                .Include(t => t.CT_DoiTuongs!)
                    .ThenInclude(ct => ct.DoiTuongSuDung)
                .FirstOrDefaultAsync(t => t.MaThuoc == id);

            if (thuoc == null)
                return NotFound();

            return View(thuoc);
        }

        // GET: Thuoc/TimKiem - EF + LINQ
        [HttpGet]
        public async Task<IActionResult> TimKiem(string tuKhoa)
        {
            if (string.IsNullOrEmpty(tuKhoa))
                return RedirectToAction(nameof(DanhSach));

            var ketQua = await _context.THUOC
                .Include(t => t.NhomThuoc)
                .Include(t => t.ThuongHieu)
                .Include(t => t.NuocSanXuat)
                .Where(t => t.TenThuoc.Contains(tuKhoa) || 
                           (t.MoTa != null && t.MoTa.Contains(tuKhoa)) ||
                           (t.NhomThuoc != null && t.NhomThuoc.TenNhomThuoc.Contains(tuKhoa)) ||
                           (t.ThuongHieu != null && t.ThuongHieu.TenThuongHieu.Contains(tuKhoa)))
                .OrderBy(t => t.TenThuoc)
                .ToListAsync();

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.DanhSachNhom = await _context.NHOM_THUOC.OrderBy(n => n.TenNhomThuoc).ToListAsync();
            ViewBag.DanhSachThuongHieu = await _context.THUONG_HIEU.OrderBy(th => th.TenThuongHieu).ToListAsync();

            return View("DanhSach", ketQua);
        }

        // GET: Thuoc/TheoNhom/5 - EF + LINQ
        public async Task<IActionResult> TheoNhom(int id)
        {
            var nhomThuoc = await _context.NHOM_THUOC.FindAsync(id);
            if (nhomThuoc == null)
                return NotFound();

            var danhSachThuoc = await _context.THUOC
                .Include(t => t.NhomThuoc)
                .Include(t => t.ThuongHieu)
                .Include(t => t.NuocSanXuat)
                .Where(t => t.MaNhomThuoc == id)
                .OrderBy(t => t.TenThuoc)
                .ToListAsync();

            ViewBag.TenNhom = nhomThuoc.TenNhomThuoc;
            ViewBag.DanhSachNhom = await _context.NHOM_THUOC.OrderBy(n => n.TenNhomThuoc).ToListAsync();
            ViewBag.DanhSachThuongHieu = await _context.THUONG_HIEU.OrderBy(th => th.TenThuongHieu).ToListAsync();

            return View("DanhSach", danhSachThuoc);
        }

        // GET: Thuoc/TheoThuongHieu/5 - EF + LINQ
        public async Task<IActionResult> TheoThuongHieu(int id)
        {
            var thuongHieu = await _context.THUONG_HIEU.FindAsync(id);
            if (thuongHieu == null)
                return NotFound();

            var danhSachThuoc = await _context.THUOC
                .Include(t => t.NhomThuoc)
                .Include(t => t.ThuongHieu)
                .Include(t => t.NuocSanXuat)
                .Where(t => t.MaThuongHieu == id)
                .OrderBy(t => t.TenThuoc)
                .ToListAsync();

            ViewBag.TenThuongHieu = thuongHieu.TenThuongHieu;
            ViewBag.DanhSachNhom = await _context.NHOM_THUOC.OrderBy(n => n.TenNhomThuoc).ToListAsync();
            ViewBag.DanhSachThuongHieu = await _context.THUONG_HIEU.OrderBy(th => th.TenThuongHieu).ToListAsync();

            return View("DanhSach", danhSachThuoc);
        }
    }
}
