using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace QL_NhaThuoc.Controllers
{
    public class DonHangController : Controller
    {
        private readonly string _connectionString;
        private readonly ILogger<DonHangController> _logger;

        public DonHangController(IConfiguration configuration, ILogger<DonHangController> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
            _logger = logger;
        }

        // Danh sach don hang cua nguoi dung
        public async Task<IActionResult> DanhSach()
        {
            var maNguoiDung = HttpContext.Session.GetInt32("MaNguoiDung");
            if (!maNguoiDung.HasValue)
            {
                TempData["LoiThongBao"] = "Vui long dang nhap";
                return RedirectToAction("PhoneLogin", "User");
            }

            var danhSachDonHang = new List<dynamic>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new SqlCommand("sp_DonHang_DanhSachTheoNguoiDung", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung.Value);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                danhSachDonHang.Add(new
                {
                    MaDonHang = reader.GetInt32(reader.GetOrdinal("MaDonHang")),
                    NgayDatHang = reader.GetDateTime(reader.GetOrdinal("NgayDatHang")),
                    TrangThai = reader.IsDBNull(reader.GetOrdinal("TrangThai")) ? "" : reader.GetString(reader.GetOrdinal("TrangThai")),
                    TongTien = reader.IsDBNull(reader.GetOrdinal("TongTien")) ? 0 : reader.GetDecimal(reader.GetOrdinal("TongTien")),
                    DiaChiGiaoHang = reader.IsDBNull(reader.GetOrdinal("DiaChiGiaoHang")) ? "" : reader.GetString(reader.GetOrdinal("DiaChiGiaoHang")),
                    PhuongThucThanhToan = reader.IsDBNull(reader.GetOrdinal("PhuongThucThanhToan")) ? "" : reader.GetString(reader.GetOrdinal("PhuongThucThanhToan"))
                });
            }

            return View(danhSachDonHang);
        }

        // Chi tiet don hang
        public async Task<IActionResult> ChiTiet(int id)
        {
            var maNguoiDung = HttpContext.Session.GetInt32("MaNguoiDung");
            if (!maNguoiDung.HasValue)
            {
                return RedirectToAction("PhoneLogin", "User");
            }

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // Kiểm tra đơn hàng thuộc về người dùng
            using var cmdCheck = new SqlCommand("SELECT MaDonHang FROM DON_HANG WHERE MaDonHang = @MaDonHang AND MaNguoiDung = @MaNguoiDung", connection);
            cmdCheck.Parameters.AddWithValue("@MaDonHang", id);
            cmdCheck.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung.Value);
            var exists = await cmdCheck.ExecuteScalarAsync();
            if (exists == null) return NotFound();

            using var cmd = new SqlCommand("sp_DonHang_ChiTiet", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@MaDonHang", id);

            dynamic? donHang = null;
            var chiTietList = new List<dynamic>();

            using var reader = await cmd.ExecuteReaderAsync();
            
            // Đọc thông tin đơn hàng
            if (await reader.ReadAsync())
            {
                donHang = new
                {
                    MaDonHang = reader.GetInt32(reader.GetOrdinal("MaDonHang")),
                    NgayDatHang = reader.GetDateTime(reader.GetOrdinal("NgayDatHang")),
                    TrangThai = reader.IsDBNull(reader.GetOrdinal("TrangThai")) ? "" : reader.GetString(reader.GetOrdinal("TrangThai")),
                    TongTien = reader.IsDBNull(reader.GetOrdinal("TongTien")) ? 0 : reader.GetDecimal(reader.GetOrdinal("TongTien")),
                    PhuongThucThanhToan = reader.IsDBNull(reader.GetOrdinal("PhuongThucThanhToan")) ? "" : reader.GetString(reader.GetOrdinal("PhuongThucThanhToan")),
                    DiaChiGiaoHang = reader.IsDBNull(reader.GetOrdinal("DiaChiGiaoHang")) ? "" : reader.GetString(reader.GetOrdinal("DiaChiGiaoHang")),
                    HoTen = reader.IsDBNull(reader.GetOrdinal("HoTen")) ? "" : reader.GetString(reader.GetOrdinal("HoTen")),
                    SoDienThoai = reader.IsDBNull(reader.GetOrdinal("SoDienThoai")) ? "" : reader.GetString(reader.GetOrdinal("SoDienThoai"))
                };
            }

            if (donHang == null) return NotFound();

            // Đọc chi tiết đơn hàng
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    chiTietList.Add(new
                    {
                        MaChiTiet = reader.GetInt32(reader.GetOrdinal("MaChiTiet")),
                        SoLuong = reader.GetInt32(reader.GetOrdinal("SoLuong")),
                        DonGia = reader.IsDBNull(reader.GetOrdinal("DonGia")) ? 0 : reader.GetDecimal(reader.GetOrdinal("DonGia")),
                        ThanhTien = reader.IsDBNull(reader.GetOrdinal("ThanhTien")) ? 0 : reader.GetDecimal(reader.GetOrdinal("ThanhTien")),
                        MaThuoc = reader.GetInt32(reader.GetOrdinal("MaThuoc")),
                        TenThuoc = reader.GetString(reader.GetOrdinal("TenThuoc")),
                        HinhAnh = reader.IsDBNull(reader.GetOrdinal("HinhAnh")) ? "" : reader.GetString(reader.GetOrdinal("HinhAnh")),
                        DonViTinh = reader.IsDBNull(reader.GetOrdinal("DonViTinh")) ? "" : reader.GetString(reader.GetOrdinal("DonViTinh"))
                    });
                }
            }

            ViewBag.ChiTietDonHangs = chiTietList;
            return View(donHang);
        }

        // Huy don hang
        [HttpPost]
        public async Task<IActionResult> Huy(int id)
        {
            var maNguoiDung = HttpContext.Session.GetInt32("MaNguoiDung");
            if (!maNguoiDung.HasValue)
            {
                return RedirectToAction("PhoneLogin", "User");
            }

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
                TempData["ThongBao"] = thongBao;
            else
                TempData["LoiThongBao"] = thongBao;

            return RedirectToAction(nameof(DanhSach));
        }

        // Theo doi don hang
        public async Task<IActionResult> TheoDoi(int id)
        {
            var maNguoiDung = HttpContext.Session.GetInt32("MaNguoiDung");
            if (!maNguoiDung.HasValue)
            {
                return RedirectToAction("PhoneLogin", "User");
            }

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT d.*, n.HoTen, n.SoDienThoai 
                FROM DON_HANG d 
                INNER JOIN NGUOI_DUNG n ON d.MaNguoiDung = n.MaNguoiDung 
                WHERE d.MaDonHang = @MaDonHang AND d.MaNguoiDung = @MaNguoiDung", connection);
            cmd.Parameters.AddWithValue("@MaDonHang", id);
            cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung.Value);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return NotFound();
            }

            var donHang = new
            {
                MaDonHang = reader.GetInt32(reader.GetOrdinal("MaDonHang")),
                NgayDatHang = reader.GetDateTime(reader.GetOrdinal("NgayDatHang")),
                TrangThai = reader.IsDBNull(reader.GetOrdinal("TrangThai")) ? "" : reader.GetString(reader.GetOrdinal("TrangThai")),
                TongTien = reader.IsDBNull(reader.GetOrdinal("TongTien")) ? 0 : reader.GetDecimal(reader.GetOrdinal("TongTien")),
                PhuongThucThanhToan = reader.IsDBNull(reader.GetOrdinal("PhuongThucThanhToan")) ? "" : reader.GetString(reader.GetOrdinal("PhuongThucThanhToan")),
                DiaChiGiaoHang = reader.IsDBNull(reader.GetOrdinal("DiaChiGiaoHang")) ? "" : reader.GetString(reader.GetOrdinal("DiaChiGiaoHang")),
                HoTen = reader.IsDBNull(reader.GetOrdinal("HoTen")) ? "" : reader.GetString(reader.GetOrdinal("HoTen")),
                SoDienThoai = reader.IsDBNull(reader.GetOrdinal("SoDienThoai")) ? "" : reader.GetString(reader.GetOrdinal("SoDienThoai"))
            };

            return View(donHang);
        }
    }
}
