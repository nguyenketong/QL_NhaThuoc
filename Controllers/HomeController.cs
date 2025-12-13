using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using QL_NhaThuoc.Models;
using System.Data;

namespace QL_NhaThuoc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly string _connectionString;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        // Trang chu - Hien thi danh sach thuoc
        public async Task<IActionResult> Index()
        {
            var danhSachThuoc = new List<dynamic>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new SqlCommand("sp_Home_ThuocMoiNhat", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@SoLuong", 12);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                danhSachThuoc.Add(new
                {
                    MaThuoc = reader.GetInt32(reader.GetOrdinal("MaThuoc")),
                    TenThuoc = reader.GetString(reader.GetOrdinal("TenThuoc")),
                    GiaBan = reader.IsDBNull(reader.GetOrdinal("GiaBan")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("GiaBan")),
                    HinhAnh = reader.IsDBNull(reader.GetOrdinal("HinhAnh")) ? "" : reader.GetString(reader.GetOrdinal("HinhAnh")),
                    MoTa = reader.IsDBNull(reader.GetOrdinal("MoTa")) ? "" : reader.GetString(reader.GetOrdinal("MoTa")),
                    TenNhomThuoc = reader.IsDBNull(reader.GetOrdinal("TenNhomThuoc")) ? "" : reader.GetString(reader.GetOrdinal("TenNhomThuoc")),
                    TenThuongHieu = reader.IsDBNull(reader.GetOrdinal("TenThuongHieu")) ? "" : reader.GetString(reader.GetOrdinal("TenThuongHieu")),
                    TenNuocSX = reader.IsDBNull(reader.GetOrdinal("TenNuocSX")) ? "" : reader.GetString(reader.GetOrdinal("TenNuocSX"))
                });
            }

            return View(danhSachThuoc);
        }

        // Gioi thieu
        public IActionResult GioiThieu()
        {
            return View();
        }

        // Lien he
        public IActionResult LienHe()
        {
            return View();
        }

        // Trang loi
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
