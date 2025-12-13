using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace QL_NhaThuoc.Controllers
{
    public class NhomThuocController : Controller
    {
        private readonly string _connectionString;

        public NhomThuocController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        // Danh sach tat ca nhom thuoc
        public async Task<IActionResult> DanhSach()
        {
            var danhSachNhom = new List<dynamic>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new SqlCommand("sp_NhomThuoc_DanhSach", connection);
            cmd.CommandType = CommandType.StoredProcedure;

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                danhSachNhom.Add(new
                {
                    MaNhomThuoc = reader.GetInt32(reader.GetOrdinal("MaNhomThuoc")),
                    TenNhomThuoc = reader.GetString(reader.GetOrdinal("TenNhomThuoc")),
                    MoTa = reader.IsDBNull(reader.GetOrdinal("MoTa")) ? "" : reader.GetString(reader.GetOrdinal("MoTa")),
                    SoLuongThuoc = reader.GetInt32(reader.GetOrdinal("SoLuongThuoc"))
                });
            }

            return View(danhSachNhom);
        }

        // Chi tiet nhom thuoc va cac thuoc trong nhom
        public async Task<IActionResult> ChiTiet(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new SqlCommand("sp_NhomThuoc_ChiTiet", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@MaNhomThuoc", id);

            dynamic? nhomThuoc = null;
            var danhSachThuoc = new List<dynamic>();

            using var reader = await cmd.ExecuteReaderAsync();
            
            // Thông tin nhóm
            if (await reader.ReadAsync())
            {
                nhomThuoc = new
                {
                    MaNhomThuoc = reader.GetInt32(reader.GetOrdinal("MaNhomThuoc")),
                    TenNhomThuoc = reader.GetString(reader.GetOrdinal("TenNhomThuoc")),
                    MoTa = reader.IsDBNull(reader.GetOrdinal("MoTa")) ? "" : reader.GetString(reader.GetOrdinal("MoTa"))
                };
            }

            if (nhomThuoc == null) return NotFound();

            // Danh sách thuốc
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    danhSachThuoc.Add(new
                    {
                        MaThuoc = reader.GetInt32(reader.GetOrdinal("MaThuoc")),
                        TenThuoc = reader.GetString(reader.GetOrdinal("TenThuoc")),
                        MoTa = reader.IsDBNull(reader.GetOrdinal("MoTa")) ? "" : reader.GetString(reader.GetOrdinal("MoTa")),
                        GiaBan = reader.IsDBNull(reader.GetOrdinal("GiaBan")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("GiaBan")),
                        HinhAnh = reader.IsDBNull(reader.GetOrdinal("HinhAnh")) ? "" : reader.GetString(reader.GetOrdinal("HinhAnh")),
                        TenThuongHieu = reader.IsDBNull(reader.GetOrdinal("TenThuongHieu")) ? "" : reader.GetString(reader.GetOrdinal("TenThuongHieu")),
                        TenNuocSX = reader.IsDBNull(reader.GetOrdinal("TenNuocSX")) ? "" : reader.GetString(reader.GetOrdinal("TenNuocSX"))
                    });
                }
            }

            ViewBag.NhomThuoc = nhomThuoc;
            return View(danhSachThuoc);
        }
    }
}
