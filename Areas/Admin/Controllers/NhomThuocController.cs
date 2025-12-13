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
    public class NhomThuocController : Controller
    {
        private readonly QL_NhaThuocDbContext _context;
        private readonly string _connectionString;

        public NhomThuocController(QL_NhaThuocDbContext context, IConfiguration config)
        {
            _context = context;
            _connectionString = config.GetConnectionString("DefaultConnection")!;
        }

        // INDEX - Gọi sp_NhomThuoc_DanhSach
        public async Task<IActionResult> Index()
        {
            var data = new List<dynamic>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_NhomThuoc_DanhSach", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            data.Add(new
                            {
                                NhomThuoc = new NhomThuoc
                                {
                                    MaNhomThuoc = reader.GetInt32(reader.GetOrdinal("MaNhomThuoc")),
                                    TenNhomThuoc = reader.GetString(reader.GetOrdinal("TenNhomThuoc")),
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
        public async Task<IActionResult> Create(NhomThuoc nhomThuoc)
        {
            if (ModelState.IsValid)
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(
                        "INSERT INTO NHOM_THUOC (TenNhomThuoc, MoTa) VALUES (@TenNhomThuoc, @MoTa)", connection))
                    {
                        command.Parameters.AddWithValue("@TenNhomThuoc", nhomThuoc.TenNhomThuoc);
                        command.Parameters.AddWithValue("@MoTa", (object?)nhomThuoc.MoTa ?? DBNull.Value);

                        await command.ExecuteNonQueryAsync();
                    }
                }

                TempData["ThongBao"] = "Thêm nhóm thuốc thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(nhomThuoc);
        }

        public async Task<IActionResult> Edit(int id)
        {
            NhomThuoc? nhomThuoc = null;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(
                    "SELECT * FROM NHOM_THUOC WHERE MaNhomThuoc = @MaNhomThuoc", connection))
                {
                    command.Parameters.AddWithValue("@MaNhomThuoc", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            nhomThuoc = new NhomThuoc
                            {
                                MaNhomThuoc = reader.GetInt32(reader.GetOrdinal("MaNhomThuoc")),
                                TenNhomThuoc = reader.GetString(reader.GetOrdinal("TenNhomThuoc")),
                                MoTa = reader.IsDBNull(reader.GetOrdinal("MoTa")) ? null : reader.GetString(reader.GetOrdinal("MoTa"))
                            };
                        }
                    }
                }
            }

            if (nhomThuoc == null) return NotFound();
            return View(nhomThuoc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NhomThuoc nhomThuoc)
        {
            if (id != nhomThuoc.MaNhomThuoc) return NotFound();

            if (ModelState.IsValid)
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(
                        "UPDATE NHOM_THUOC SET TenNhomThuoc = @TenNhomThuoc, MoTa = @MoTa WHERE MaNhomThuoc = @MaNhomThuoc", connection))
                    {
                        command.Parameters.AddWithValue("@MaNhomThuoc", nhomThuoc.MaNhomThuoc);
                        command.Parameters.AddWithValue("@TenNhomThuoc", nhomThuoc.TenNhomThuoc);
                        command.Parameters.AddWithValue("@MoTa", (object?)nhomThuoc.MoTa ?? DBNull.Value);

                        await command.ExecuteNonQueryAsync();
                    }
                }

                TempData["ThongBao"] = "Cập nhật nhóm thuốc thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(nhomThuoc);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Kiểm tra ràng buộc
                using (var checkCommand = new SqlCommand(
                    "SELECT COUNT(*) FROM THUOC WHERE MaNhomThuoc = @MaNhomThuoc", connection))
                {
                    checkCommand.Parameters.AddWithValue("@MaNhomThuoc", id);
                    var soThuocSuDung = (int)(await checkCommand.ExecuteScalarAsync())!;

                    if (soThuocSuDung > 0)
                    {
                        TempData["LoiThongBao"] = $"Không thể xóa! Có {soThuocSuDung} thuốc đang thuộc nhóm này.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                // Xóa
                using (var deleteCommand = new SqlCommand(
                    "DELETE FROM NHOM_THUOC WHERE MaNhomThuoc = @MaNhomThuoc", connection))
                {
                    deleteCommand.Parameters.AddWithValue("@MaNhomThuoc", id);
                    await deleteCommand.ExecuteNonQueryAsync();
                }

                TempData["ThongBao"] = "Xóa nhóm thuốc thành công!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
