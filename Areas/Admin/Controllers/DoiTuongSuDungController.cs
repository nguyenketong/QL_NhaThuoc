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
    public class DoiTuongSuDungController : Controller
    {
        private readonly QL_NhaThuocDbContext _context;
        private readonly string _connectionString;

        public DoiTuongSuDungController(QL_NhaThuocDbContext context, IConfiguration config)
        {
            _context = context;
            _connectionString = config.GetConnectionString("DefaultConnection")!;
        }

        // INDEX - Gọi sp_DoiTuongSuDung_DanhSach
        public async Task<IActionResult> Index()
        {
            var data = new List<dynamic>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_DoiTuongSuDung_DanhSach", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            data.Add(new
                            {
                                DoiTuong = new DoiTuongSuDung
                                {
                                    MaDoiTuong = reader.GetInt32(reader.GetOrdinal("MaDoiTuong")),
                                    TenDoiTuong = reader.GetString(reader.GetOrdinal("TenDoiTuong")),
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
        public async Task<IActionResult> Create(DoiTuongSuDung doiTuong)
        {
            if (ModelState.IsValid)
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(
                        "INSERT INTO DOI_TUONG_SU_DUNG (TenDoiTuong, MoTa) VALUES (@TenDoiTuong, @MoTa)", connection))
                    {
                        command.Parameters.AddWithValue("@TenDoiTuong", doiTuong.TenDoiTuong);
                        command.Parameters.AddWithValue("@MoTa", (object?)doiTuong.MoTa ?? DBNull.Value);

                        await command.ExecuteNonQueryAsync();
                    }
                }

                TempData["ThongBao"] = "Thêm đối tượng sử dụng thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(doiTuong);
        }

        public async Task<IActionResult> Edit(int id)
        {
            DoiTuongSuDung? doiTuong = null;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(
                    "SELECT * FROM DOI_TUONG_SU_DUNG WHERE MaDoiTuong = @MaDoiTuong", connection))
                {
                    command.Parameters.AddWithValue("@MaDoiTuong", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            doiTuong = new DoiTuongSuDung
                            {
                                MaDoiTuong = reader.GetInt32(reader.GetOrdinal("MaDoiTuong")),
                                TenDoiTuong = reader.GetString(reader.GetOrdinal("TenDoiTuong")),
                                MoTa = reader.IsDBNull(reader.GetOrdinal("MoTa")) ? null : reader.GetString(reader.GetOrdinal("MoTa"))
                            };
                        }
                    }
                }
            }

            if (doiTuong == null) return NotFound();
            return View(doiTuong);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DoiTuongSuDung doiTuong)
        {
            if (id != doiTuong.MaDoiTuong) return NotFound();

            if (ModelState.IsValid)
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(
                        "UPDATE DOI_TUONG_SU_DUNG SET TenDoiTuong = @TenDoiTuong, MoTa = @MoTa WHERE MaDoiTuong = @MaDoiTuong", connection))
                    {
                        command.Parameters.AddWithValue("@MaDoiTuong", doiTuong.MaDoiTuong);
                        command.Parameters.AddWithValue("@TenDoiTuong", doiTuong.TenDoiTuong);
                        command.Parameters.AddWithValue("@MoTa", (object?)doiTuong.MoTa ?? DBNull.Value);

                        await command.ExecuteNonQueryAsync();
                    }
                }

                TempData["ThongBao"] = "Cập nhật đối tượng sử dụng thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(doiTuong);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Kiểm tra ràng buộc
                using (var checkCommand = new SqlCommand(
                    "SELECT COUNT(*) FROM CT_DOI_TUONG WHERE MaDoiTuong = @MaDoiTuong", connection))
                {
                    checkCommand.Parameters.AddWithValue("@MaDoiTuong", id);
                    var soThuocSuDung = (int)(await checkCommand.ExecuteScalarAsync())!;

                    if (soThuocSuDung > 0)
                    {
                        TempData["LoiThongBao"] = $"Không thể xóa! Có {soThuocSuDung} thuốc đang sử dụng đối tượng này.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                // Xóa
                using (var deleteCommand = new SqlCommand(
                    "DELETE FROM DOI_TUONG_SU_DUNG WHERE MaDoiTuong = @MaDoiTuong", connection))
                {
                    deleteCommand.Parameters.AddWithValue("@MaDoiTuong", id);
                    await deleteCommand.ExecuteNonQueryAsync();
                }

                TempData["ThongBao"] = "Xóa đối tượng sử dụng thành công!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
