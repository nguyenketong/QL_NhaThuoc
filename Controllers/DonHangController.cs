using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QL_NhaThuoc.Data;
using System.Data;

namespace QL_NhaThuoc.Controllers
{
    public class DonHangController : Controller
    {
        private readonly QL_NhaThuocDbContext _context;
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;

        public DonHangController(QL_NhaThuocDbContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
            _configuration = configuration;
        }

        // GET: DonHang/DanhSach - EF + LINQ
        public async Task<IActionResult> DanhSach()
        {
            var maNguoiDung = GetCurrentUserId();
            if (!maNguoiDung.HasValue)
                return RedirectToAction("PhoneLogin", "User");

            var donHangs = await _context.DON_HANG
                .Include(dh => dh.ChiTietDonHangs!)
                    .ThenInclude(ct => ct.Thuoc)
                .Where(dh => dh.MaNguoiDung == maNguoiDung.Value)
                .OrderByDescending(dh => dh.NgayDatHang)
                .ToListAsync();

            return View(donHangs);
        }

        // GET: DonHang/ChiTiet/5 - EF + LINQ
        public async Task<IActionResult> ChiTiet(int id)
        {
            var maNguoiDung = GetCurrentUserId();
            if (!maNguoiDung.HasValue)
                return RedirectToAction("PhoneLogin", "User");

            var donHang = await _context.DON_HANG
                .Include(dh => dh.ChiTietDonHangs!)
                    .ThenInclude(ct => ct.Thuoc)
                .FirstOrDefaultAsync(dh => dh.MaDonHang == id && dh.MaNguoiDung == maNguoiDung.Value);

            if (donHang == null)
                return NotFound();

            return View(donHang);
        }

        // GET: DonHang/TheoDoi/5 - EF + LINQ
        public async Task<IActionResult> TheoDoi(int id)
        {
            var maNguoiDung = GetCurrentUserId();
            if (!maNguoiDung.HasValue)
                return RedirectToAction("PhoneLogin", "User");

            var donHang = await _context.DON_HANG
                .FirstOrDefaultAsync(dh => dh.MaDonHang == id && dh.MaNguoiDung == maNguoiDung.Value);

            if (donHang == null)
                return NotFound();

            return View(donHang);
        }

        // POST: DonHang/HuyDon/5 - Stored Procedure (logic phức tạp)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyDon(int id)
        {
            var maNguoiDung = GetCurrentUserId();
            if (!maNguoiDung.HasValue)
                return RedirectToAction("PhoneLogin", "User");

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new SqlCommand("sp_DonHang_HuyDonHang", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@MaDonHang", id);
            cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung.Value);

            var ketQuaParam = new SqlParameter("@KetQua", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var thongBaoParam = new SqlParameter("@ThongBao", SqlDbType.NVarChar, 200) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(ketQuaParam);
            cmd.Parameters.Add(thongBaoParam);

            await cmd.ExecuteNonQueryAsync();

            var ketQua = (int)ketQuaParam.Value;
            var thongBao = thongBaoParam.Value?.ToString() ?? "";

            if (ketQua == 1)
            {
                TempData["ThongBao"] = thongBao;
            }
            else
            {
                TempData["LoiThongBao"] = thongBao;
            }

            return RedirectToAction(nameof(ChiTiet), new { id });
        }

        // GET: DonHang/ThanhToanQR/5 - Hiển thị QR thanh toán
        public async Task<IActionResult> ThanhToanQR(int id)
        {
            var maNguoiDung = GetCurrentUserId();
            if (!maNguoiDung.HasValue)
                return RedirectToAction("PhoneLogin", "User");

            var donHang = await _context.DON_HANG
                .FirstOrDefaultAsync(dh => dh.MaDonHang == id && dh.MaNguoiDung == maNguoiDung.Value);

            if (donHang == null)
                return NotFound();

            // Lấy thông tin ngân hàng từ config
            ViewBag.BankId = _configuration["BankInfo:BankId"];
            ViewBag.AccountNo = _configuration["BankInfo:AccountNo"];
            ViewBag.AccountName = _configuration["BankInfo:AccountName"];
            ViewBag.Template = _configuration["BankInfo:Template"];

            return View(donHang);
        }

        private int? GetCurrentUserId()
        {
            if (Request.Cookies.TryGetValue("UserId", out var userIdStr) && int.TryParse(userIdStr, out var userId))
                return userId;
            return null;
        }
    }
}
