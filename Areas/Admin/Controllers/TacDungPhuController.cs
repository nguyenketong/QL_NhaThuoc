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
    public class TacDungPhuController : Controller
    {
        private readonly QL_NhaThuocDbContext _context;
        private readonly string _connectionString;

        public TacDungPhuController(QL_NhaThuocDbContext context, IConfiguration config)
        {
            _context = context;
            _connectionString = config.GetConnectionString("DefaultConnection")!;
        }

        // INDEX - Gọi sp_TacDungPhu_DanhSach
        public async Task<IActionResult> Index()
        {
            var data = new List<dynamic>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_TacDungPhu_DanhSach", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            data.Add(new
                            {
                                TacDungPhu = new TacDungPhu
                                {
                                    MaTacDungPhu = reader.GetInt32(reader.GetOrdinal("MaTacDungPhu")),
                                    TenTacDungPhu = reader.GetString(reader.GetOrdinal("TenTacDungPhu")),
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
        public async Task<IActionResult> Create(TacDungPhu tacDungPhu)
        {
            if (ModelState.IsValid)
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(
                        "INSERT INTO TAC_DUNG_PHU (TenTacDungPhu, MoTa) VALUES (@TenTacDungPhu, @MoTa)", connection))
                    {
                        command.Parameters.AddWithValue("@TenTacDungPhu", tacDungPhu.TenTacDungPhu);
                        command.Parameters.AddWithValue("@MoTa", (object?)tacDungPhu.MoTa ?? DBNull.Value);

                        await command.ExecuteNonQueryAsync();
                    }
                }

                TempData["ThongBao"] = "Thêm tác dụng phụ thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(tacDungPhu);
        }

        public async Task<IActionResult> Edit(int id)
        {
            TacDungPhu? tacDungPhu = null;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(
                    "SELECT * FROM TAC_DUNG_PHU WHERE MaTacDungPhu = @MaTacDungPhu", connection))
                {
                    command.Parameters.AddWithValue("@MaTacDungPhu", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            tacDungPhu = new TacDungPhu
                            {
                                MaTacDungPhu = reader.GetInt32(reader.GetOrdinal("MaTacDungPhu")),
                                TenTacDungPhu = reader.GetString(reader.GetOrdinal("TenTacDungPhu")),
                                MoTa = reader.IsDBNull(reader.GetOrdinal("MoTa")) ? null : reader.GetString(reader.GetOrdinal("MoTa"))
                            };
                        }
                    }
                }
            }

            if (tacDungPhu == null) return NotFound();
            return View(tacDungPhu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TacDungPhu tacDungPhu)
        {
            if (id != tacDungPhu.MaTacDungPhu) return NotFound();

            if (ModelState.IsValid)
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(
                        "UPDATE TAC_DUNG_PHU SET TenTacDungPhu = @TenTacDungPhu, MoTa = @MoTa WHERE MaTacDungPhu = @MaTacDungPhu", connection))
                    {
                        command.Parameters.AddWithValue("@MaTacDungPhu", tacDungPhu.MaTacDungPhu);
                        command.Parameters.AddWithValue("@TenTacDungPhu", tacDungPhu.TenTacDungPhu);
                        command.Parameters.AddWithValue("@MoTa", (object?)tacDungPhu.MoTa ?? DBNull.Value);

                        await command.ExecuteNonQueryAsync();
                    }
                }

                TempData["ThongBao"] = "Cập nhật tác dụng phụ thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(tacDungPhu);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Kiểm tra ràng buộc
                using (var checkCommand = new SqlCommand(
                    "SELECT COUNT(*) FROM CT_TAC_DUNG_PHU WHERE MaTacDungPhu = @MaTacDungPhu", connection))
                {
                    checkCommand.Parameters.AddWithValue("@MaTacDungPhu", id);
                    var soThuocSuDung = (int)(await checkCommand.ExecuteScalarAsync())!;

                    if (soThuocSuDung > 0)
                    {
                        TempData["LoiThongBao"] = $"Không thể xóa! Có {soThuocSuDung} thuốc đang sử dụng tác dụng phụ này.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                // Xóa
                using (var deleteCommand = new SqlCommand(
                    "DELETE FROM TAC_DUNG_PHU WHERE MaTacDungPhu = @MaTacDungPhu", connection))
                {
                    deleteCommand.Parameters.AddWithValue("@MaTacDungPhu", id);
                    await deleteCommand.ExecuteNonQueryAsync();
                }

                TempData["ThongBao"] = "Xóa tác dụng phụ thành công!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
