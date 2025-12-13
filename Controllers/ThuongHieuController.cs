using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace QL_NhaThuoc.Controllers
{
    public class ThuongHieuController : Controller
    {
        private readonly string _connectionString;

        public ThuongHieuController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        // Chi tiết thương hiệu - hiển thị các thuốc của thương hiệu
        public async Task<IActionResult> ChiTiet(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new SqlCommand("sp_ThuongHieu_ChiTiet", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@MaThuongHieu", id);

            dynamic? thuongHieu = null;
            var danhSachThuoc = new List<dynamic>();

            using var reader = await cmd.ExecuteReaderAsync();
            
            // Thông tin thương hiệu
            if (await reader.ReadAsync())
            {
                thuongHieu = new
                {
                    MaThuongHieu = reader.GetInt32(reader.GetOrdinal("MaThuongHieu")),
                    TenThuongHieu = reader.GetString(reader.GetOrdinal("TenThuongHieu")),
                    QuocGia = reader.IsDBNull(reader.GetOrdinal("QuocGia")) ? "" : reader.GetString(reader.GetOrdinal("QuocGia"))
                };
            }

            if (thuongHieu == null) return NotFound();

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
                        HinhAnh = reader.IsDBNull(reader.GetOrdinal("HinhAnh")) ? "" : reader.GetString(reader.GetOrdinal("HinhAnh"))
                    });
                }
            }

            ViewBag.Thuocs = danhSachThuoc;
            return View(thuongHieu);
        }
    }
}
