using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QL_NhaThuoc.Data;
using QL_NhaThuoc.Filters;
using System.Data;

namespace QL_NhaThuoc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminAuthorize]
    public class DashboardController : Controller
    {
        private readonly string _connectionString;
        private readonly QL_NhaThuocDbContext _context;

        public DashboardController(IConfiguration configuration, QL_NhaThuocDbContext context)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
            _context = context;
        }

        // GET: Admin/Dashboard - Stored Procedure (thống kê phức tạp)
        public async Task<IActionResult> Index()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new SqlCommand("sp_Dashboard_ThongKe", connection);
            cmd.CommandType = CommandType.StoredProcedure;

            using var reader = await cmd.ExecuteReaderAsync();

            // 1. Tổng số thuốc
            if (await reader.ReadAsync())
            {
                ViewBag.TongThuoc = reader.GetInt32(0);
            }

            // 2. Tổng số đơn hàng
            if (await reader.NextResultAsync() && await reader.ReadAsync())
            {
                ViewBag.TongDonHang = reader.GetInt32(0);
            }

            // 3. Tổng số khách hàng
            if (await reader.NextResultAsync() && await reader.ReadAsync())
            {
                ViewBag.TongKhachHang = reader.GetInt32(0);
            }

            // 4. Đơn hàng theo trạng thái
            if (await reader.NextResultAsync() && await reader.ReadAsync())
            {
                ViewBag.ChoXuLy = reader.IsDBNull(reader.GetOrdinal("ChoXuLy")) ? 0 : reader.GetInt32(reader.GetOrdinal("ChoXuLy"));
                ViewBag.DangGiao = reader.IsDBNull(reader.GetOrdinal("DangGiao")) ? 0 : reader.GetInt32(reader.GetOrdinal("DangGiao"));
                ViewBag.HoanThanh = reader.IsDBNull(reader.GetOrdinal("HoanThanh")) ? 0 : reader.GetInt32(reader.GetOrdinal("HoanThanh"));
                ViewBag.DaHuy = reader.IsDBNull(reader.GetOrdinal("DaHuy")) ? 0 : reader.GetInt32(reader.GetOrdinal("DaHuy"));
            }

            // 5. Doanh thu tháng này
            if (await reader.NextResultAsync() && await reader.ReadAsync())
            {
                ViewBag.DoanhThuThang = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
            }

            // 6. Đơn hàng mới nhất (Top 5)
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

            // 7. Top thuốc bán chạy (Top 5)
            var topThuocBanChay = new List<dynamic>();
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    topThuocBanChay.Add(new
                    {
                        TenThuoc = reader.GetString(reader.GetOrdinal("TenThuoc")),
                        TongBan = reader.GetInt32(reader.GetOrdinal("TongBan"))
                    });
                }
            }
            ViewBag.TopThuocBanChay = topThuocBanChay;

            // 8. Doanh thu 7 ngày gần nhất (EF + LINQ)
            var ngayBatDau = DateTime.Today.AddDays(-6);
            var doanhThu7Ngay = await _context.DON_HANG
                .Where(dh => dh.NgayDatHang >= ngayBatDau && dh.TrangThai != "Da huy")
                .GroupBy(dh => dh.NgayDatHang.Date)
                .Select(g => new { Ngay = g.Key, DoanhThu = g.Sum(dh => dh.TongTien ?? 0) })
                .OrderBy(x => x.Ngay)
                .ToListAsync();

            // Tạo dữ liệu đầy đủ 7 ngày (kể cả ngày không có đơn)
            var labels = new List<string>();
            var data = new List<decimal>();
            for (int i = 6; i >= 0; i--)
            {
                var ngay = DateTime.Today.AddDays(-i);
                labels.Add(ngay.ToString("dd/MM"));
                var dt = doanhThu7Ngay.FirstOrDefault(x => x.Ngay == ngay);
                data.Add(dt?.DoanhThu ?? 0);
            }
            ViewBag.ChartLabels = labels;
            ViewBag.ChartData = data;

            return View();
        }
    }
}
