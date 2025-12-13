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
    public class NuocSanXuatController : Controller
    {
        private readonly QL_NhaThuocDbContext _context;
        private readonly string _connectionString;

        public NuocSanXuatController(QL_NhaThuocDbContext context, IConfiguration config)
        {
            _context = context;
            _connectionString = config.GetConnectionString("DefaultConnection")!;
        }

        // INDEX - Gọi sp_NuocSanXuat_DanhSach
        public async Task<IActionResult> Index()
        {
            var data = new List<dynamic>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_NuocSanXuat_DanhSach", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            data.Add(new
                            {
                                NuocSanXuat = new NuocSanXuat
                                {
                                    MaNuocSX = reader.GetInt32(reader.GetOrdinal("MaNuocSX")),
                                    TenNuocSX = reader.GetString(reader.GetOrdinal("TenNuocSX"))
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

        // CREATE - Gọi sp_NuocSanXuat_Them
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NuocSanXuat nuocSX)
        {
            if (ModelState.IsValid)
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("sp_NuocSanXuat_Them", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@TenNuocSX", nuocSX.TenNuocSX);

                        await command.ExecuteScalarAsync();
                    }
                }

                TempData["ThongBao"] = "Thêm nước sản xuất thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(nuocSX);
        }

        // EDIT GET - Lấy thông tin để sửa
        public async Task<IActionResult> Edit(int id)
        {
            NuocSanXuat? nuocSX = null;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(
                    "SELECT MaNuocSX, TenNuocSX FROM NUOC_SAN_XUAT WHERE MaNuocSX = @MaNuocSX", connection))
                {
                    command.Parameters.AddWithValue("@MaNuocSX", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            nuocSX = new NuocSanXuat
                            {
                                MaNuocSX = reader.GetInt32(0),
                                TenNuocSX = reader.GetString(1)
                            };
                        }
                    }
                }
            }

            if (nuocSX == null) return NotFound();
            return View(nuocSX);
        }

        // EDIT POST - Gọi sp_NuocSanXuat_Sua
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NuocSanXuat nuocSX)
        {
            if (id != nuocSX.MaNuocSX) return NotFound();

            if (ModelState.IsValid)
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("sp_NuocSanXuat_Sua", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@MaNuocSX", nuocSX.MaNuocSX);
                        command.Parameters.AddWithValue("@TenNuocSX", nuocSX.TenNuocSX);

                        await command.ExecuteNonQueryAsync();
                    }
                }

                TempData["ThongBao"] = "Cập nhật nước sản xuất thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(nuocSX);
        }

        // DELETE - Gọi sp_NuocSanXuat_Xoa
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_NuocSanXuat_Xoa", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@MaNuocSX", id);

                    // Output parameters
                    var ketQuaParam = new SqlParameter("@KetQua", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    var thongBaoParam = new SqlParameter("@ThongBao", SqlDbType.NVarChar, 200) { Direction = ParameterDirection.Output };

                    command.Parameters.Add(ketQuaParam);
                    command.Parameters.Add(thongBaoParam);

                    await command.ExecuteNonQueryAsync();

                    var ketQua = (int)ketQuaParam.Value;
                    var thongBao = thongBaoParam.Value?.ToString();

                    if (ketQua == 1)
                        TempData["ThongBao"] = thongBao;
                    else
                        TempData["LoiThongBao"] = thongBao;
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
