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
    public class ThanhPhanController : Controller
    {
        private readonly QL_NhaThuocDbContext _context;
        private readonly string _connectionString;

        public ThanhPhanController(QL_NhaThuocDbContext context, IConfiguration config)
        {
            _context = context;
            _connectionString = config.GetConnectionString("DefaultConnection")!;
        }

        // INDEX - Gọi sp_ThanhPhan_DanhSach
        public async Task<IActionResult> Index()
        {
            var data = new List<dynamic>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_ThanhPhan_DanhSach", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            data.Add(new
                            {
                                ThanhPhan = new ThanhPhan
                                {
                                    MaThanhPhan = reader.GetInt32(reader.GetOrdinal("MaThanhPhan")),
                                    TenThanhPhan = reader.GetString(reader.GetOrdinal("TenThanhPhan")),
                                    MoTa = reader.IsDBNull(reader.GetOrdinal("MoTa")) ? null : reader.GetString(reader.GetOrdinal("MoTa"))
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
        public async Task<IActionResult> Create(ThanhPhan thanhPhan)
        {
            if (ModelState.IsValid)
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(
                        "INSERT INTO THANH_PHAN (TenThanhPhan, MoTa) VALUES (@TenThanhPhan, @MoTa)", connection))
                    {
                        command.Parameters.AddWithValue("@TenThanhPhan", thanhPhan.TenThanhPhan);
                        command.Parameters.AddWithValue("@MoTa", (object?)thanhPhan.MoTa ?? DBNull.Value);

                        await command.ExecuteNonQueryAsync();
                    }
                }

                TempData["ThongBao"] = "Thêm thành phần thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(thanhPhan);
        }

        public async Task<IActionResult> Edit(int id)
        {
            ThanhPhan? thanhPhan = null;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(
                    "SELECT * FROM THANH_PHAN WHERE MaThanhPhan = @MaThanhPhan", connection))
                {
                    command.Parameters.AddWithValue("@MaThanhPhan", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            thanhPhan = new ThanhPhan
                            {
                                MaThanhPhan = reader.GetInt32(reader.GetOrdinal("MaThanhPhan")),
                                TenThanhPhan = reader.GetString(reader.GetOrdinal("TenThanhPhan")),
                                MoTa = reader.IsDBNull(reader.GetOrdinal("MoTa")) ? null : reader.GetString(reader.GetOrdinal("MoTa"))
                            };
                        }
                    }
                }
            }

            if (thanhPhan == null) return NotFound();
            return View(thanhPhan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ThanhPhan thanhPhan)
        {
            if (id != thanhPhan.MaThanhPhan) return NotFound();

            if (ModelState.IsValid)
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(
                        "UPDATE THANH_PHAN SET TenThanhPhan = @TenThanhPhan, MoTa = @MoTa WHERE MaThanhPhan = @MaThanhPhan", connection))
                    {
                        command.Parameters.AddWithValue("@MaThanhPhan", thanhPhan.MaThanhPhan);
                        command.Parameters.AddWithValue("@TenThanhPhan", thanhPhan.TenThanhPhan);
                        command.Parameters.AddWithValue("@MoTa", (object?)thanhPhan.MoTa ?? DBNull.Value);

                        await command.ExecuteNonQueryAsync();
                    }
                }

                TempData["ThongBao"] = "Cập nhật thành phần thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(thanhPhan);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Kiểm tra ràng buộc
                using (var checkCommand = new SqlCommand(
                    "SELECT COUNT(*) FROM CT_THANH_PHAN WHERE MaThanhPhan = @MaThanhPhan", connection))
                {
                    checkCommand.Parameters.AddWithValue("@MaThanhPhan", id);
                    var soThuocSuDung = (int)(await checkCommand.ExecuteScalarAsync())!;

                    if (soThuocSuDung > 0)
                    {
                        TempData["LoiThongBao"] = $"Không thể xóa! Có {soThuocSuDung} thuốc đang sử dụng thành phần này.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                // Xóa
                using (var deleteCommand = new SqlCommand(
                    "DELETE FROM THANH_PHAN WHERE MaThanhPhan = @MaThanhPhan", connection))
                {
                    deleteCommand.Parameters.AddWithValue("@MaThanhPhan", id);
                    await deleteCommand.ExecuteNonQueryAsync();
                }

                TempData["ThongBao"] = "Xóa thành phần thành công!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
