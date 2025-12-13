using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using QL_NhaThuoc.Filters;
using System.Data;

namespace QL_NhaThuoc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminAuthorize]
    public class DashboardController : Controller
    {
        private readonly string _connectionString;

        public DashboardController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<IActionResult> Index()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // Thống kê tổng quan
            using (var cmd = new SqlCommand("sp_Dashboard_ThongKe", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                using var reader = await cmd.ExecuteReaderAsync();

                // Tổng số thuốc
                if (await reader.ReadAsync())
                    ViewBag.TongThuoc = reader.GetInt32(0);
                
                // Tổng số đơn hàng
                if (await reader.NextResultAsync() && await reader.ReadAsync())
                    ViewBag.TongDonHang = reader.GetInt32(0);
                
                // Tổng số khách hàng
                if (await reader.NextResultAsync() && await reader.ReadAsync())
                    ViewBag.TongNguoiDung = reader.GetInt32(0);
                
                // Đơn hàng theo trạng thái
                if (await reader.NextResultAsync() && await reader.ReadAsync())
                {
                    ViewBag.DonChoXuLy = reader.GetInt32(0);
                    ViewBag.DonDangGiao = reader.GetInt32(1);
                    ViewBag.DonHoanThanh = reader.GetInt32(2);
                    ViewBag.DonDaHuy = reader.GetInt32(3);
                }
                
                // Doanh thu tháng
                if (await reader.NextResultAsync() && await reader.ReadAsync())
                    ViewBag.DoanhThuThang = reader.GetDecimal(0);
                
                // Đơn hàng gần đây
                var donHangGanDay = new List<dynamic>();
                if (await reader.NextResultAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        donHangGanDay.Add(new
                        {
                            MaDonHang = reader.GetInt32(reader.GetOrdinal("MaDonHang")),
                            NgayDatHang = reader.GetDateTime(reader.GetOrdinal("NgayDatHang")),
                            TrangThai = reader.IsDBNull(reader.GetOrdinal("TrangThai")) ? "" : reader.GetString(reader.GetOrdinal("TrangThai")),
                            TongTien = reader.IsDBNull(reader.GetOrdinal("TongTien")) ? 0 : reader.GetDecimal(reader.GetOrdinal("TongTien")),
                            HoTen = reader.IsDBNull(reader.GetOrdinal("HoTen")) ? "" : reader.GetString(reader.GetOrdinal("HoTen"))
                        });
                    }
                }
                ViewBag.DonHangGanDay = donHangGanDay;
                
                // Top thuốc bán chạy
                var topThuoc = new List<dynamic>();
                if (await reader.NextResultAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        topThuoc.Add(new
                        {
                            TenThuoc = reader.GetString(0),
                            SoLuong = reader.GetInt32(1)
                        });
                    }
                }
                ViewBag.TopThuoc = topThuoc;
            }

            // Thêm TongNhomThuoc
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM NHOM_THUOC", connection))
            {
                ViewBag.TongNhomThuoc = await cmd.ExecuteScalarAsync();
            }

            return View();
        }
    }
}
