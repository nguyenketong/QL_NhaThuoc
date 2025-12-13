using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace QL_NhaThuoc.Controllers
{
    public class ThuocController : Controller
    {
        private readonly string _connectionString;
        private readonly ILogger<ThuocController> _logger;

        public ThuocController(IConfiguration configuration, ILogger<ThuocController> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
            _logger = logger;
        }

        // Danh sach tat ca thuoc
        public async Task<IActionResult> DanhSach(int? maNhom, int? maThuongHieu, decimal? giaMin, decimal? giaMax, string? tuKhoa)
        {
            var danhSachThuoc = new List<dynamic>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new SqlCommand("sp_Thuoc_DanhSach", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@MaNhom", (object?)maNhom ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MaThuongHieu", (object?)maThuongHieu ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@GiaMin", (object?)giaMin ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@GiaMax", (object?)giaMax ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TuKhoa", (object?)tuKhoa ?? DBNull.Value);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                danhSachThuoc.Add(new
                {
                    MaThuoc = reader.GetInt32(reader.GetOrdinal("MaThuoc")),
                    TenThuoc = reader.GetString(reader.GetOrdinal("TenThuoc")),
                    MoTa = reader.IsDBNull(reader.GetOrdinal("MoTa")) ? "" : reader.GetString(reader.GetOrdinal("MoTa")),
                    GiaBan = reader.IsDBNull(reader.GetOrdinal("GiaBan")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("GiaBan")),
                    HinhAnh = reader.IsDBNull(reader.GetOrdinal("HinhAnh")) ? "" : reader.GetString(reader.GetOrdinal("HinhAnh")),
                    TenNhomThuoc = reader.IsDBNull(reader.GetOrdinal("TenNhomThuoc")) ? "" : reader.GetString(reader.GetOrdinal("TenNhomThuoc")),
                    TenThuongHieu = reader.IsDBNull(reader.GetOrdinal("TenThuongHieu")) ? "" : reader.GetString(reader.GetOrdinal("TenThuongHieu")),
                    TenNuocSX = reader.IsDBNull(reader.GetOrdinal("TenNuocSX")) ? "" : reader.GetString(reader.GetOrdinal("TenNuocSX"))
                });
            }

            // Truyen danh sach nhom thuoc va thuong hieu cho filter
            ViewBag.DanhSachNhom = await LoadNhomThuocs(connection);
            ViewBag.DanhSachThuongHieu = await LoadThuongHieus(connection);

            return View(danhSachThuoc);
        }

        // Chi tiet thuoc
        public async Task<IActionResult> ChiTiet(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new SqlCommand("sp_Thuoc_ChiTiet", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@MaThuoc", id);

            dynamic? thuoc = null;
            var thanhPhans = new List<dynamic>();
            var tacDungPhus = new List<dynamic>();
            var doiTuongs = new List<dynamic>();

            using var reader = await cmd.ExecuteReaderAsync();
            
            // Thông tin thuốc
            if (await reader.ReadAsync())
            {
                thuoc = new
                {
                    MaThuoc = reader.GetInt32(reader.GetOrdinal("MaThuoc")),
                    TenThuoc = reader.GetString(reader.GetOrdinal("TenThuoc")),
                    MoTa = reader.IsDBNull(reader.GetOrdinal("MoTa")) ? "" : reader.GetString(reader.GetOrdinal("MoTa")),
                    GiaBan = reader.IsDBNull(reader.GetOrdinal("GiaBan")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("GiaBan")),
                    DonViTinh = reader.IsDBNull(reader.GetOrdinal("DonViTinh")) ? "" : reader.GetString(reader.GetOrdinal("DonViTinh")),
                    HinhAnh = reader.IsDBNull(reader.GetOrdinal("HinhAnh")) ? "" : reader.GetString(reader.GetOrdinal("HinhAnh")),
                    TenNhomThuoc = reader.IsDBNull(reader.GetOrdinal("TenNhomThuoc")) ? "" : reader.GetString(reader.GetOrdinal("TenNhomThuoc")),
                    TenThuongHieu = reader.IsDBNull(reader.GetOrdinal("TenThuongHieu")) ? "" : reader.GetString(reader.GetOrdinal("TenThuongHieu")),
                    TenNuocSX = reader.IsDBNull(reader.GetOrdinal("TenNuocSX")) ? "" : reader.GetString(reader.GetOrdinal("TenNuocSX"))
                };
            }

            if (thuoc == null) return NotFound();

            // Thành phần
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    thanhPhans.Add(new
                    {
                        TenThanhPhan = reader.GetString(reader.GetOrdinal("TenThanhPhan")),
                        HamLuong = reader.IsDBNull(reader.GetOrdinal("HamLuong")) ? "" : reader.GetString(reader.GetOrdinal("HamLuong"))
                    });
                }
            }

            // Tác dụng phụ
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    tacDungPhus.Add(new
                    {
                        TenTacDungPhu = reader.GetString(reader.GetOrdinal("TenTacDungPhu")),
                        MucDo = reader.IsDBNull(reader.GetOrdinal("MucDo")) ? "" : reader.GetString(reader.GetOrdinal("MucDo"))
                    });
                }
            }

            // Đối tượng sử dụng
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    doiTuongs.Add(new
                    {
                        TenDoiTuong = reader.GetString(reader.GetOrdinal("TenDoiTuong"))
                    });
                }
            }

            ViewBag.ThanhPhans = thanhPhans;
            ViewBag.TacDungPhus = tacDungPhus;
            ViewBag.DoiTuongs = doiTuongs;

            return View(thuoc);
        }

        // Tim kiem thuoc
        [HttpGet]
        public async Task<IActionResult> TimKiem(string tuKhoa)
        {
            if (string.IsNullOrEmpty(tuKhoa))
            {
                return RedirectToAction(nameof(DanhSach));
            }

            var ketQua = new List<dynamic>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new SqlCommand("sp_Thuoc_TimKiem", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@TuKhoa", tuKhoa);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                ketQua.Add(new
                {
                    MaThuoc = reader.GetInt32(reader.GetOrdinal("MaThuoc")),
                    TenThuoc = reader.GetString(reader.GetOrdinal("TenThuoc")),
                    MoTa = reader.IsDBNull(reader.GetOrdinal("MoTa")) ? "" : reader.GetString(reader.GetOrdinal("MoTa")),
                    GiaBan = reader.IsDBNull(reader.GetOrdinal("GiaBan")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("GiaBan")),
                    HinhAnh = reader.IsDBNull(reader.GetOrdinal("HinhAnh")) ? "" : reader.GetString(reader.GetOrdinal("HinhAnh")),
                    TenNhomThuoc = reader.IsDBNull(reader.GetOrdinal("TenNhomThuoc")) ? "" : reader.GetString(reader.GetOrdinal("TenNhomThuoc")),
                    TenThuongHieu = reader.IsDBNull(reader.GetOrdinal("TenThuongHieu")) ? "" : reader.GetString(reader.GetOrdinal("TenThuongHieu")),
                    TenNuocSX = reader.IsDBNull(reader.GetOrdinal("TenNuocSX")) ? "" : reader.GetString(reader.GetOrdinal("TenNuocSX"))
                });
            }

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.DanhSachNhom = await LoadNhomThuocs(connection);
            ViewBag.DanhSachThuongHieu = await LoadThuongHieus(connection);
            return View("DanhSach", ketQua);
        }

        // Thuoc theo nhom
        public async Task<IActionResult> TheoNhom(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new SqlCommand("sp_NhomThuoc_ChiTiet", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@MaNhomThuoc", id);

            string? tenNhom = null;
            var danhSachThuoc = new List<dynamic>();

            using var reader = await cmd.ExecuteReaderAsync();
            
            // Thông tin nhóm
            if (await reader.ReadAsync())
            {
                tenNhom = reader.GetString(reader.GetOrdinal("TenNhomThuoc"));
            }

            if (tenNhom == null) return NotFound();

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
                        HinhAnh = reader.IsDBNull(reader.GetOrdinal("HinhAnh")) ? "" : reader.GetString(reader.GetOrdinal("HinhAnh")),
                        TenNhomThuoc = tenNhom,
                        TenThuongHieu = reader.IsDBNull(reader.GetOrdinal("TenThuongHieu")) ? "" : reader.GetString(reader.GetOrdinal("TenThuongHieu")),
                        TenNuocSX = reader.IsDBNull(reader.GetOrdinal("TenNuocSX")) ? "" : reader.GetString(reader.GetOrdinal("TenNuocSX"))
                    });
                }
            }

            ViewBag.TenNhom = tenNhom;
            ViewBag.DanhSachNhom = await LoadNhomThuocs(connection);
            ViewBag.DanhSachThuongHieu = await LoadThuongHieus(connection);
            return View("DanhSach", danhSachThuoc);
        }

        // Thuoc theo thuong hieu
        public async Task<IActionResult> TheoThuongHieu(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new SqlCommand("sp_ThuongHieu_ChiTiet", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@MaThuongHieu", id);

            string? tenThuongHieu = null;
            var danhSachThuoc = new List<dynamic>();

            using var reader = await cmd.ExecuteReaderAsync();
            
            // Thông tin thương hiệu
            if (await reader.ReadAsync())
            {
                tenThuongHieu = reader.GetString(reader.GetOrdinal("TenThuongHieu"));
            }

            if (tenThuongHieu == null) return NotFound();

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
                        HinhAnh = reader.IsDBNull(reader.GetOrdinal("HinhAnh")) ? "" : reader.GetString(reader.GetOrdinal("HinhAnh")),
                        TenNhomThuoc = "",
                        TenThuongHieu = tenThuongHieu,
                        TenNuocSX = ""
                    });
                }
            }

            ViewBag.TenThuongHieu = tenThuongHieu;
            ViewBag.DanhSachNhom = await LoadNhomThuocs(connection);
            ViewBag.DanhSachThuongHieu = await LoadThuongHieus(connection);
            return View("DanhSach", danhSachThuoc);
        }

        private async Task<List<dynamic>> LoadNhomThuocs(SqlConnection connection)
        {
            var list = new List<dynamic>();
            using var cmd = new SqlCommand("SELECT MaNhomThuoc, TenNhomThuoc FROM NHOM_THUOC ORDER BY TenNhomThuoc", connection);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new { MaNhomThuoc = reader.GetInt32(0), TenNhomThuoc = reader.GetString(1) });
            }
            return list;
        }

        private async Task<List<dynamic>> LoadThuongHieus(SqlConnection connection)
        {
            var list = new List<dynamic>();
            using var cmd = new SqlCommand("SELECT MaThuongHieu, TenThuongHieu FROM THUONG_HIEU ORDER BY TenThuongHieu", connection);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new { MaThuongHieu = reader.GetInt32(0), TenThuongHieu = reader.GetString(1) });
            }
            return list;
        }
    }
}
