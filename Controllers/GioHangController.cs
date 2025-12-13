using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;

namespace QL_NhaThuoc.Controllers
{
    public class GioHangController : Controller
    {
        private readonly string _connectionString;
        private readonly ILogger<GioHangController> _logger;
        private const string GIO_HANG_KEY = "GioHang";

        public GioHangController(IConfiguration configuration, ILogger<GioHangController> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
            _logger = logger;
        }

        // Model cho gio hang
        public class GioHangItem
        {
            public int MaThuoc { get; set; }
            public string TenThuoc { get; set; } = string.Empty;
            public decimal GiaBan { get; set; }
            public int SoLuong { get; set; }
            public decimal ThanhTien => GiaBan * SoLuong;
        }

        // Xem gio hang
        public IActionResult Index()
        {
            var gioHang = LayGioHang();
            return View(gioHang);
        }

        // Them vao gio hang
        [HttpPost]
        public async Task<IActionResult> Them(int maThuoc, int soLuong = 1)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new SqlCommand("SELECT MaThuoc, TenThuoc, GiaBan FROM THUOC WHERE MaThuoc = @MaThuoc", connection);
            cmd.Parameters.AddWithValue("@MaThuoc", maThuoc);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return NotFound();
            }

            var tenThuoc = reader.GetString(1);
            var giaBan = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2);

            var gioHang = LayGioHang();

            // Kiem tra thuoc da co trong gio hang chua
            var item = gioHang.FirstOrDefault(g => g.MaThuoc == maThuoc);
            if (item != null)
            {
                item.SoLuong += soLuong;
            }
            else
            {
                gioHang.Add(new GioHangItem
                {
                    MaThuoc = maThuoc,
                    TenThuoc = tenThuoc,
                    GiaBan = giaBan,
                    SoLuong = soLuong
                });
            }

            LuuGioHang(gioHang);

            TempData["ThongBao"] = $"Da them {tenThuoc} vao gio hang";
            return RedirectToAction("Index");
        }

        // Cap nhat so luong
        [HttpPost]
        public IActionResult CapNhat(int maThuoc, int soLuong)
        {
            if (soLuong <= 0)
            {
                return RedirectToAction(nameof(Xoa), new { maThuoc });
            }

            var gioHang = LayGioHang();
            var item = gioHang.FirstOrDefault(g => g.MaThuoc == maThuoc);
            
            if (item != null)
            {
                item.SoLuong = soLuong;
                LuuGioHang(gioHang);
            }

            return RedirectToAction("Index");
        }

        // Xoa khoi gio hang
        public IActionResult Xoa(int maThuoc)
        {
            var gioHang = LayGioHang();
            var item = gioHang.FirstOrDefault(g => g.MaThuoc == maThuoc);
            
            if (item != null)
            {
                gioHang.Remove(item);
                LuuGioHang(gioHang);
                TempData["ThongBao"] = "Da xoa san pham khoi gio hang";
            }

            return RedirectToAction("Index");
        }

        // Xoa toan bo gio hang
        public IActionResult XoaTatCa()
        {
            HttpContext.Session.Remove(GIO_HANG_KEY);
            TempData["ThongBao"] = "Da xoa tat ca san pham trong gio hang";
            return RedirectToAction("Index");
        }

        // Thanh toan
        public IActionResult ThanhToan()
        {
            var gioHang = LayGioHang();
            if (!gioHang.Any())
            {
                TempData["LoiThongBao"] = "Gio hang trong";
                return RedirectToAction("Index");
            }

            var maNguoiDung = HttpContext.Session.GetInt32("MaNguoiDung");
            if (!maNguoiDung.HasValue)
            {
                TempData["LoiThongBao"] = "Vui long dang nhap de thanh toan";
                return RedirectToAction("PhoneLogin", "User");
            }

            return View(gioHang);
        }

        // Xu ly thanh toan
        [HttpPost]
        public async Task<IActionResult> XuLyThanhToan(string diaChiGiaoHang, string phuongThucThanhToan)
        {
            var maNguoiDung = HttpContext.Session.GetInt32("MaNguoiDung");
            if (!maNguoiDung.HasValue)
            {
                return RedirectToAction("PhoneLogin", "User");
            }

            var gioHang = LayGioHang();
            if (!gioHang.Any())
            {
                return RedirectToAction("Index");
            }

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                // Tao don hang
                using var cmd = new SqlCommand("sp_DonHang_TaoDonHang", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung.Value);
                cmd.Parameters.AddWithValue("@DiaChiGiaoHang", diaChiGiaoHang);
                cmd.Parameters.AddWithValue("@PhuongThucThanhToan", phuongThucThanhToan);
                cmd.Parameters.AddWithValue("@TongTien", gioHang.Sum(g => g.ThanhTien));
                
                var maDonHangParam = new SqlParameter("@MaDonHangMoi", SqlDbType.Int) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(maDonHangParam);

                await cmd.ExecuteNonQueryAsync();
                var maDonHang = (int)maDonHangParam.Value;

                // Them chi tiet don hang
                foreach (var item in gioHang)
                {
                    using var cmdCT = new SqlCommand("sp_DonHang_ThemChiTiet", connection);
                    cmdCT.CommandType = CommandType.StoredProcedure;
                    cmdCT.Parameters.AddWithValue("@MaDonHang", maDonHang);
                    cmdCT.Parameters.AddWithValue("@MaThuoc", item.MaThuoc);
                    cmdCT.Parameters.AddWithValue("@SoLuong", item.SoLuong);
                    cmdCT.Parameters.AddWithValue("@DonGia", item.GiaBan);
                    cmdCT.Parameters.AddWithValue("@ThanhTien", item.ThanhTien);
                    await cmdCT.ExecuteNonQueryAsync();
                }

                // Xoa gio hang
                HttpContext.Session.Remove(GIO_HANG_KEY);

                TempData["ThongBao"] = "Dat hang thanh cong!";
                return RedirectToAction("ChiTiet", "DonHang", new { id = maDonHang });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Loi khi tao don hang");
                TempData["LoiThongBao"] = "Co loi xay ra. Vui long thu lai";
                return RedirectToAction("ThanhToan");
            }
        }

        // Lay gio hang tu session
        private List<GioHangItem> LayGioHang()
        {
            var gioHangJson = HttpContext.Session.GetString(GIO_HANG_KEY);
            if (string.IsNullOrEmpty(gioHangJson))
            {
                return new List<GioHangItem>();
            }

            return JsonSerializer.Deserialize<List<GioHangItem>>(gioHangJson) ?? new List<GioHangItem>();
        }

        // Luu gio hang vao session
        private void LuuGioHang(List<GioHangItem> gioHang)
        {
            var gioHangJson = JsonSerializer.Serialize(gioHang);
            HttpContext.Session.SetString(GIO_HANG_KEY, gioHangJson);
        }

        // API lay so luong gio hang
        [HttpGet]
        public IActionResult LaySoLuong()
        {
            var gioHang = LayGioHang();
            var soLuong = gioHang.Sum(g => g.SoLuong);
            return Json(new { soLuong });
        }

        // API them vao gio hang (AJAX)
        [HttpPost]
        public async Task<IActionResult> ThemAjax(int maThuoc, int soLuong = 1)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new SqlCommand("SELECT MaThuoc, TenThuoc, GiaBan FROM THUOC WHERE MaThuoc = @MaThuoc", connection);
            cmd.Parameters.AddWithValue("@MaThuoc", maThuoc);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm" });
            }

            var tenThuoc = reader.GetString(1);
            var giaBan = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2);

            var gioHang = LayGioHang();

            var item = gioHang.FirstOrDefault(g => g.MaThuoc == maThuoc);
            if (item != null)
            {
                item.SoLuong += soLuong;
            }
            else
            {
                gioHang.Add(new GioHangItem
                {
                    MaThuoc = maThuoc,
                    TenThuoc = tenThuoc,
                    GiaBan = giaBan,
                    SoLuong = soLuong
                });
            }

            LuuGioHang(gioHang);

            var tongSoLuong = gioHang.Sum(g => g.SoLuong);
            return Json(new { success = true, message = $"Đã thêm {tenThuoc} vào giỏ hàng", soLuong = tongSoLuong });
        }
    }
}
