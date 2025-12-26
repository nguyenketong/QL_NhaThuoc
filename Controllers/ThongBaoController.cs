using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_NhaThuoc.Data;

namespace QL_NhaThuoc.Controllers
{
    public class ThongBaoController : Controller
    {
        private readonly QL_NhaThuocDbContext _context;

        public ThongBaoController(QL_NhaThuocDbContext context)
        {
            _context = context;
        }

        // GET: ThongBao/LaySoLuongChuaDoc - AJAX
        [HttpGet]
        public async Task<IActionResult> LaySoLuongChuaDoc()
        {
            var maNguoiDung = GetCurrentUserId();
            if (!maNguoiDung.HasValue)
                return Json(new { soLuong = 0 });

            var soLuong = await _context.THONG_BAO
                .CountAsync(tb => tb.MaNguoiDung == maNguoiDung.Value && !tb.DaDoc);

            return Json(new { soLuong });
        }

        // GET: ThongBao/LayDanhSach - AJAX
        [HttpGet]
        public async Task<IActionResult> LayDanhSach()
        {
            var maNguoiDung = GetCurrentUserId();
            if (!maNguoiDung.HasValue)
                return Json(new { thongBaos = new List<object>() });

            var thongBaos = await _context.THONG_BAO
                .Where(tb => tb.MaNguoiDung == maNguoiDung.Value)
                .OrderByDescending(tb => tb.NgayTao)
                .Take(10)
                .Select(tb => new
                {
                    tb.MaThongBao,
                    tb.TieuDe,
                    tb.NoiDung,
                    tb.LoaiThongBao,
                    tb.DaDoc,
                    tb.DuongDan,
                    NgayTao = tb.NgayTao.ToString("dd/MM/yyyy HH:mm")
                })
                .ToListAsync();

            return Json(new { thongBaos });
        }

        // POST: ThongBao/DanhDauDaDoc - AJAX
        [HttpPost]
        public async Task<IActionResult> DanhDauDaDoc(int id)
        {
            var maNguoiDung = GetCurrentUserId();
            if (!maNguoiDung.HasValue)
                return Json(new { success = false });

            var thongBao = await _context.THONG_BAO
                .FirstOrDefaultAsync(tb => tb.MaThongBao == id && tb.MaNguoiDung == maNguoiDung.Value);

            if (thongBao != null)
            {
                thongBao.DaDoc = true;
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        // POST: ThongBao/DanhDauTatCaDaDoc - AJAX
        [HttpPost]
        public async Task<IActionResult> DanhDauTatCaDaDoc()
        {
            var maNguoiDung = GetCurrentUserId();
            if (!maNguoiDung.HasValue)
                return Json(new { success = false });

            var thongBaos = await _context.THONG_BAO
                .Where(tb => tb.MaNguoiDung == maNguoiDung.Value && !tb.DaDoc)
                .ToListAsync();

            foreach (var tb in thongBaos)
                tb.DaDoc = true;

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        private int? GetCurrentUserId()
        {
            if (Request.Cookies.TryGetValue("UserId", out var userIdStr) && int.TryParse(userIdStr, out var userId))
                return userId;
            return null;
        }
    }
}
