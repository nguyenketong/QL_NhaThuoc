using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using QL_NhaThuoc.Data;
using QL_NhaThuoc.Models;
using QL_NhaThuoc.Filters;
using System.Data;

namespace QL_NhaThuoc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminAuthorize]
    public class ThuongHieuController : Controller
    {
        private readonly QL_NhaThuocDbContext _context;
        private readonly string _connectionString;

        public ThuongHieuController(QL_NhaThuocDbContext context, IConfiguration config)
        {
            _context = context;
            _connectionString = config.GetConnectionString("DefaultConnection")!;
        }

        // INDEX - Gọi sp_ThuongHieu_DanhSach
        public async Task<IActionResult> Index()
        {
            var data = new List<dynamic>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_ThuongHieu_DanhSach", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            data.Add(new
                            {
                                ThuongHieu = new ThuongHieu
                                {
                                    MaThuongHieu = reader.GetInt32(reader.GetOrdinal("MaThuongHieu")),
                                    TenThuongHieu = reader.GetString(reader.GetOrdinal("TenThuongHieu")),
                                    QuocGia = reader.IsDBNull(reader.GetOrdinal("QuocGia")) ? null : reader.GetString(reader.GetOrdinal("QuocGia"))
                                },
                                SoLuongThuoc = reader.GetInt32(reader.GetOrdinal("SoLuongThuoc"))
                            });
                        }
                    }
                }
            }

            return View(data);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ThuongHieu thuongHieu)
        {
            if (ModelState.IsValid)
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(
                        "INSERT INTO THUONG_HIEU (TenThuongHieu, QuocGia) VALUES (@TenThuongHieu, @QuocGia)", connection))
                    {
                        command.Parameters.AddWithValue("@TenThuongHieu", thuongHieu.TenThuongHieu);
                        command.Parameters.AddWithValue("@QuocGia", (object?)thuongHieu.QuocGia ?? DBNull.Value);

                        await command.ExecuteNonQueryAsync();
                    }
                }

                TempData["ThongBao"] = "Thêm thương hiệu thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(thuongHieu);
        }

        public async Task<IActionResult> Edit(int id)
        {
            ThuongHieu? thuongHieu = null;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(
                    "SELECT * FROM THUONG_HIEU WHERE MaThuongHieu = @MaThuongHieu", connection))
                {
                    command.Parameters.AddWithValue("@MaThuongHieu", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            thuongHieu = new ThuongHieu
                            {
                                MaThuongHieu = reader.GetInt32(reader.GetOrdinal("MaThuongHieu")),
                                TenThuongHieu = reader.GetString(reader.GetOrdinal("TenThuongHieu")),
                                QuocGia = reader.IsDBNull(reader.GetOrdinal("QuocGia")) ? null : reader.GetString(reader.GetOrdinal("QuocGia"))
                            };
                        }
                    }
                }
            }

            if (thuongHieu == null) return NotFound();
            return View(thuongHieu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ThuongHieu thuongHieu)
        {
            if (id != thuongHieu.MaThuongHieu) return NotFound();

            if (ModelState.IsValid)
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(
                        "UPDATE THUONG_HIEU SET TenThuongHieu = @TenThuongHieu, QuocGia = @QuocGia WHERE MaThuongHieu = @MaThuongHieu", connection))
                    {
                        command.Parameters.AddWithValue("@MaThuongHieu", thuongHieu.MaThuongHieu);
                        command.Parameters.AddWithValue("@TenThuongHieu", thuongHieu.TenThuongHieu);
                        command.Parameters.AddWithValue("@QuocGia", (object?)thuongHieu.QuocGia ?? DBNull.Value);

                        await command.ExecuteNonQueryAsync();
                    }
                }

                TempData["ThongBao"] = "Cập nhật thương hiệu thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(thuongHieu);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Kiểm tra ràng buộc
                using (var checkCommand = new SqlCommand(
                    "SELECT COUNT(*) FROM THUOC WHERE MaThuongHieu = @MaThuongHieu", connection))
                {
                    checkCommand.Parameters.AddWithValue("@MaThuongHieu", id);
                    var soThuocSuDung = (int)(await checkCommand.ExecuteScalarAsync())!;

                    if (soThuocSuDung > 0)
                    {
                        TempData["LoiThongBao"] = $"Không thể xóa! Có {soThuocSuDung} thuốc đang sử dụng thương hiệu này.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                // Xóa
                using (var deleteCommand = new SqlCommand(
                    "DELETE FROM THUONG_HIEU WHERE MaThuongHieu = @MaThuongHieu", connection))
                {
                    deleteCommand.Parameters.AddWithValue("@MaThuongHieu", id);
                    await deleteCommand.ExecuteNonQueryAsync();
                }

                TempData["ThongBao"] = "Xóa thương hiệu thành công!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
