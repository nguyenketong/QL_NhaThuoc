using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using QL_NhaThuoc.Filters;
using System.Data;

namespace QL_NhaThuoc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminAuthorize]
    public class NguoiDungController : Controller
    {
        private readonly string _connectionString;

        public NguoiDungController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var danhSach = new List<dynamic>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new SqlCommand("sp_Admin_NguoiDung_DanhSach", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@VaiTro", DBNull.Value);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var hoTen = reader.IsDBNull(reader.GetOrdinal("HoTen")) ? "" : reader.GetString(reader.GetOrdinal("HoTen"));
                var soDienThoai = reader.GetString(reader.GetOrdinal("SoDienThoai"));

                // Filter theo search
                if (!string.IsNullOrEmpty(search))
                {
                    if (!soDienThoai.Contains(search) && !hoTen.Contains(search, StringComparison.OrdinalIgnoreCase))
                        continue;
                }

                danhSach.Add(new
                {
                    MaNguoiDung = reader.GetInt32(reader.GetOrdinal("MaNguoiDung")),
                    HoTen = hoTen,
                    SoDienThoai = soDienThoai,
                    DiaChi = reader.IsDBNull(reader.GetOrdinal("DiaChi")) ? "" : reader.GetString(reader.GetOrdinal("DiaChi")),
                    VaiTro = reader.IsDBNull(reader.GetOrdinal("VaiTro")) ? "" : reader.GetString(reader.GetOrdinal("VaiTro")),
                    NgayTao = reader.IsDBNull(reader.GetOrdinal("NgayTao")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("NgayTao")),
                    SoDonHang = reader.GetInt32(reader.GetOrdinal("SoDonHang")),
                    TongChiTieu = reader.GetDecimal(reader.GetOrdinal("TongChiTieu"))
                });
            }

            return View(danhSach);
        }

        public async Task<IActionResult> Details(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new SqlCommand("sp_Admin_NguoiDung_ChiTiet", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@MaNguoiDung", id);

            dynamic? nguoiDung = null;
            var donHangs = new List<dynamic>();

            using var reader = await cmd.ExecuteReaderAsync();
            
            // Đọc thông tin người dùng
            if (await reader.ReadAsync())
            {
                nguoiDung = new
                {
                    MaNguoiDung = reader.GetInt32(reader.GetOrdinal("MaNguoiDung")),
                    HoTen = reader.IsDBNull(reader.GetOrdinal("HoTen")) ? "" : reader.GetString(reader.GetOrdinal("HoTen")),
                    SoDienThoai = reader.GetString(reader.GetOrdinal("SoDienThoai")),
                    DiaChi = reader.IsDBNull(reader.GetOrdinal("DiaChi")) ? "" : reader.GetString(reader.GetOrdinal("DiaChi")),
                    VaiTro = reader.IsDBNull(reader.GetOrdinal("VaiTro")) ? "" : reader.GetString(reader.GetOrdinal("VaiTro")),
                    NgayTao = reader.IsDBNull(reader.GetOrdinal("NgayTao")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("NgayTao"))
                };
            }

            if (nguoiDung == null) return NotFound();

            // Đọc đơn hàng
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    donHangs.Add(new
                    {
                        MaDonHang = reader.GetInt32(reader.GetOrdinal("MaDonHang")),
                        NgayDatHang = reader.GetDateTime(reader.GetOrdinal("NgayDatHang")),
                        TrangThai = reader.IsDBNull(reader.GetOrdinal("TrangThai")) ? "" : reader.GetString(reader.GetOrdinal("TrangThai")),
                        TongTien = reader.IsDBNull(reader.GetOrdinal("TongTien")) ? 0 : reader.GetDecimal(reader.GetOrdinal("TongTien"))
                    });
                }
            }

            ViewBag.DonHangs = donHangs;
            return View(nguoiDung);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new SqlCommand("sp_Admin_NguoiDung_Xoa", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@MaNguoiDung", id);
            
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

            return RedirectToAction(nameof(Index));
        }
    }
}
