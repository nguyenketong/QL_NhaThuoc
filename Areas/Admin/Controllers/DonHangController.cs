using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using QL_NhaThuoc.Filters;
using System.Data;

namespace QL_NhaThuoc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminAuthorize]
    public class DonHangController : Controller
    {
        private readonly string _connectionString;

        public DonHangController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        // Danh sách đơn hàng
        public async Task<IActionResult> Index(string? trangThai, DateTime? tuNgay, DateTime? denNgay)
        {
            var danhSach = new List<dynamic>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new SqlCommand("sp_Admin_DonHang_DanhSach", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@TrangThai", (object?)trangThai ?? DBNull.Value);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var ngayDatHang = reader.GetDateTime(reader.GetOrdinal("NgayDatHang"));
                
                // Filter theo ngày
                if (tuNgay.HasValue && ngayDatHang < tuNgay.Value) continue;
                if (denNgay.HasValue && ngayDatHang > denNgay.Value) continue;

                danhSach.Add(new
                {
                    MaDonHang = reader.GetInt32(reader.GetOrdinal("MaDonHang")),
                    NgayDatHang = ngayDatHang,
                    TrangThai = reader.IsDBNull(reader.GetOrdinal("TrangThai")) ? "" : reader.GetString(reader.GetOrdinal("TrangThai")),
                    TongTien = reader.IsDBNull(reader.GetOrdinal("TongTien")) ? 0 : reader.GetDecimal(reader.GetOrdinal("TongTien")),
                    PhuongThucThanhToan = reader.IsDBNull(reader.GetOrdinal("PhuongThucThanhToan")) ? "" : reader.GetString(reader.GetOrdinal("PhuongThucThanhToan")),
                    DiaChiGiaoHang = reader.IsDBNull(reader.GetOrdinal("DiaChiGiaoHang")) ? "" : reader.GetString(reader.GetOrdinal("DiaChiGiaoHang")),
                    HoTen = reader.IsDBNull(reader.GetOrdinal("HoTen")) ? "" : reader.GetString(reader.GetOrdinal("HoTen")),
                    SoDienThoai = reader.IsDBNull(reader.GetOrdinal("SoDienThoai")) ? "" : reader.GetString(reader.GetOrdinal("SoDienThoai")),
                    SoSanPham = reader.GetInt32(reader.GetOrdinal("SoSanPham"))
                });
            }

            return View(danhSach);
        }

        // Chi tiết đơn hàng
        public async Task<IActionResult> Details(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

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
                    SoDienThoai = reader.IsDBNull(reader.GetOrdinal("SoDienThoai")) ? "" : reader.GetString(reader.GetOrdinal("SoDienThoai")),
                    DiaChi = reader.IsDBNull(reader.GetOrdinal("DiaChi")) ? "" : reader.GetString(reader.GetOrdinal("DiaChi"))
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

        // Cập nhật trạng thái
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string trangThai)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new SqlCommand("sp_Admin_DonHang_CapNhatTrangThai", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@MaDonHang", id);
            cmd.Parameters.AddWithValue("@TrangThai", trangThai);
            await cmd.ExecuteNonQueryAsync();

            TempData["ThongBao"] = "Cập nhật trạng thái thành công!";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
