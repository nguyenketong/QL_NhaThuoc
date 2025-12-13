using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using QL_NhaThuoc.Models;
using QL_NhaThuoc.Filters;
using System.Data;

namespace QL_NhaThuoc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminAuthorize]
    public class ThuocController : Controller
    {
        private readonly string _connectionString;

        public ThuocController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        // Danh sách thuốc
        public async Task<IActionResult> Index(string? search, int? nhomId)
        {
            var danhSach = new List<dynamic>();
            
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new SqlCommand("sp_Admin_Thuoc_DanhSach", connection);
            cmd.CommandType = CommandType.StoredProcedure;

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var thuoc = new
                {
                    MaThuoc = reader.GetInt32(reader.GetOrdinal("MaThuoc")),
                    TenThuoc = reader.GetString(reader.GetOrdinal("TenThuoc")),
                    GiaBan = reader.IsDBNull(reader.GetOrdinal("GiaBan")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("GiaBan")),
                    DonViTinh = reader.IsDBNull(reader.GetOrdinal("DonViTinh")) ? "" : reader.GetString(reader.GetOrdinal("DonViTinh")),
                    HinhAnh = reader.IsDBNull(reader.GetOrdinal("HinhAnh")) ? "" : reader.GetString(reader.GetOrdinal("HinhAnh")),
                    MaNhomThuoc = reader.IsDBNull(reader.GetOrdinal("MaNhomThuoc")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("MaNhomThuoc")),
                    TenNhomThuoc = reader.IsDBNull(reader.GetOrdinal("TenNhomThuoc")) ? "" : reader.GetString(reader.GetOrdinal("TenNhomThuoc")),
                    TenThuongHieu = reader.IsDBNull(reader.GetOrdinal("TenThuongHieu")) ? "" : reader.GetString(reader.GetOrdinal("TenThuongHieu")),
                    TenNuocSX = reader.IsDBNull(reader.GetOrdinal("TenNuocSX")) ? "" : reader.GetString(reader.GetOrdinal("TenNuocSX"))
                };

                // Filter theo search và nhomId
                if (!string.IsNullOrEmpty(search) && !thuoc.TenThuoc.Contains(search, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (nhomId.HasValue && thuoc.MaNhomThuoc != nhomId)
                    continue;

                danhSach.Add(thuoc);
            }

            // Load danh sách nhóm thuốc cho filter
            ViewBag.NhomThuocs = await LoadNhomThuocs(connection);
            
            return View(danhSach);
        }

        // Thêm thuốc - GET
        public async Task<IActionResult> Create()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            await LoadDropdowns(connection);
            await LoadChiTietDropdowns(connection);
            return View();
        }

        // Thêm thuốc - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Thuoc thuoc, int[]? ThanhPhanIds, string[]? HamLuongs, int[]? TacDungPhuIds, string[]? MucDos, int[]? DoiTuongIds)
        {
            if (ModelState.IsValid)
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                // Thêm thuốc
                using var cmd = new SqlCommand("sp_Admin_Thuoc_Them", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TenThuoc", thuoc.TenThuoc);
                cmd.Parameters.AddWithValue("@MaNhomThuoc", (object?)thuoc.MaNhomThuoc ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@MaNuocSX", (object?)thuoc.MaNuocSX ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@MaThuongHieu", (object?)thuoc.MaThuongHieu ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@GiaBan", (object?)thuoc.GiaBan ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DonViTinh", (object?)thuoc.DonViTinh ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@MoTa", (object?)thuoc.MoTa ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@HinhAnh", (object?)thuoc.HinhAnh ?? DBNull.Value);

                var result = await cmd.ExecuteScalarAsync();
                var maThuocMoi = Convert.ToInt32(result);

                // Thêm thành phần
                if (ThanhPhanIds != null)
                {
                    for (int i = 0; i < ThanhPhanIds.Length; i++)
                    {
                        if (ThanhPhanIds[i] > 0)
                        {
                            using var cmdTP = new SqlCommand("INSERT INTO CT_THANH_PHAN (MaThuoc, MaThanhPhan, HamLuong) VALUES (@MaThuoc, @MaThanhPhan, @HamLuong)", connection);
                            cmdTP.Parameters.AddWithValue("@MaThuoc", maThuocMoi);
                            cmdTP.Parameters.AddWithValue("@MaThanhPhan", ThanhPhanIds[i]);
                            cmdTP.Parameters.AddWithValue("@HamLuong", HamLuongs != null && i < HamLuongs.Length ? (object)HamLuongs[i] : DBNull.Value);
                            await cmdTP.ExecuteNonQueryAsync();
                        }
                    }
                }

                // Thêm tác dụng phụ
                if (TacDungPhuIds != null)
                {
                    for (int i = 0; i < TacDungPhuIds.Length; i++)
                    {
                        if (TacDungPhuIds[i] > 0)
                        {
                            using var cmdTDP = new SqlCommand("INSERT INTO CT_TAC_DUNG_PHU (MaThuoc, MaTacDungPhu, MucDo) VALUES (@MaThuoc, @MaTacDungPhu, @MucDo)", connection);
                            cmdTDP.Parameters.AddWithValue("@MaThuoc", maThuocMoi);
                            cmdTDP.Parameters.AddWithValue("@MaTacDungPhu", TacDungPhuIds[i]);
                            cmdTDP.Parameters.AddWithValue("@MucDo", MucDos != null && i < MucDos.Length ? (object)MucDos[i] : DBNull.Value);
                            await cmdTDP.ExecuteNonQueryAsync();
                        }
                    }
                }

                // Thêm đối tượng sử dụng
                if (DoiTuongIds != null)
                {
                    foreach (var doiTuongId in DoiTuongIds)
                    {
                        if (doiTuongId > 0)
                        {
                            using var cmdDT = new SqlCommand("INSERT INTO CT_DOI_TUONG (MaThuoc, MaDoiTuong) VALUES (@MaThuoc, @MaDoiTuong)", connection);
                            cmdDT.Parameters.AddWithValue("@MaThuoc", maThuocMoi);
                            cmdDT.Parameters.AddWithValue("@MaDoiTuong", doiTuongId);
                            await cmdDT.ExecuteNonQueryAsync();
                        }
                    }
                }

                TempData["ThongBao"] = "Thêm thuốc thành công!";
                return RedirectToAction(nameof(Index));
            }

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            await LoadDropdowns(conn);
            await LoadChiTietDropdowns(conn);
            return View(thuoc);
        }


        // Sửa thuốc - GET
        public async Task<IActionResult> Edit(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            Thuoc? thuoc = null;
            var selectedDoiTuongs = new List<int>();

            // Lấy thông tin thuốc
            using (var cmd = new SqlCommand("SELECT * FROM THUOC WHERE MaThuoc = @MaThuoc", connection))
            {
                cmd.Parameters.AddWithValue("@MaThuoc", id);
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    thuoc = new Thuoc
                    {
                        MaThuoc = reader.GetInt32(reader.GetOrdinal("MaThuoc")),
                        TenThuoc = reader.GetString(reader.GetOrdinal("TenThuoc")),
                        MoTa = reader.IsDBNull(reader.GetOrdinal("MoTa")) ? null : reader.GetString(reader.GetOrdinal("MoTa")),
                        GiaBan = reader.IsDBNull(reader.GetOrdinal("GiaBan")) ? null : reader.GetDecimal(reader.GetOrdinal("GiaBan")),
                        DonViTinh = reader.IsDBNull(reader.GetOrdinal("DonViTinh")) ? null : reader.GetString(reader.GetOrdinal("DonViTinh")),
                        HinhAnh = reader.IsDBNull(reader.GetOrdinal("HinhAnh")) ? null : reader.GetString(reader.GetOrdinal("HinhAnh")),
                        MaNhomThuoc = reader.IsDBNull(reader.GetOrdinal("MaNhomThuoc")) ? 0 : reader.GetInt32(reader.GetOrdinal("MaNhomThuoc")),
                        MaThuongHieu = reader.IsDBNull(reader.GetOrdinal("MaThuongHieu")) ? null : reader.GetInt32(reader.GetOrdinal("MaThuongHieu")),
                        MaNuocSX = reader.IsDBNull(reader.GetOrdinal("MaNuocSX")) ? null : reader.GetInt32(reader.GetOrdinal("MaNuocSX"))
                    };
                }
            }

            if (thuoc == null) return NotFound();

            // Lấy đối tượng sử dụng đã chọn
            using (var cmd = new SqlCommand("SELECT MaDoiTuong FROM CT_DOI_TUONG WHERE MaThuoc = @MaThuoc", connection))
            {
                cmd.Parameters.AddWithValue("@MaThuoc", id);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    selectedDoiTuongs.Add(reader.GetInt32(0));
                }
            }

            await LoadDropdowns(connection);
            await LoadChiTietDropdowns(connection);
            ViewBag.SelectedDoiTuongs = selectedDoiTuongs;

            return View(thuoc);
        }

        // Sửa thuốc - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Thuoc thuoc, int[]? ThanhPhanIds, string[]? HamLuongs, int[]? TacDungPhuIds, string[]? MucDos, int[]? DoiTuongIds)
        {
            if (id != thuoc.MaThuoc) return NotFound();

            if (ModelState.IsValid)
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                // Cập nhật thuốc
                using var cmd = new SqlCommand("sp_Admin_Thuoc_Sua", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@MaThuoc", thuoc.MaThuoc);
                cmd.Parameters.AddWithValue("@TenThuoc", thuoc.TenThuoc);
                cmd.Parameters.AddWithValue("@MaNhomThuoc", (object?)thuoc.MaNhomThuoc ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@MaNuocSX", (object?)thuoc.MaNuocSX ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@MaThuongHieu", (object?)thuoc.MaThuongHieu ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@GiaBan", (object?)thuoc.GiaBan ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DonViTinh", (object?)thuoc.DonViTinh ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@MoTa", (object?)thuoc.MoTa ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@HinhAnh", (object?)thuoc.HinhAnh ?? DBNull.Value);
                await cmd.ExecuteNonQueryAsync();

                // Xóa dữ liệu cũ
                using (var cmdDel = new SqlCommand("DELETE FROM CT_THANH_PHAN WHERE MaThuoc = @MaThuoc; DELETE FROM CT_TAC_DUNG_PHU WHERE MaThuoc = @MaThuoc; DELETE FROM CT_DOI_TUONG WHERE MaThuoc = @MaThuoc;", connection))
                {
                    cmdDel.Parameters.AddWithValue("@MaThuoc", id);
                    await cmdDel.ExecuteNonQueryAsync();
                }

                // Thêm thành phần mới
                if (ThanhPhanIds != null)
                {
                    for (int i = 0; i < ThanhPhanIds.Length; i++)
                    {
                        if (ThanhPhanIds[i] > 0)
                        {
                            using var cmdTP = new SqlCommand("INSERT INTO CT_THANH_PHAN (MaThuoc, MaThanhPhan, HamLuong) VALUES (@MaThuoc, @MaThanhPhan, @HamLuong)", connection);
                            cmdTP.Parameters.AddWithValue("@MaThuoc", id);
                            cmdTP.Parameters.AddWithValue("@MaThanhPhan", ThanhPhanIds[i]);
                            cmdTP.Parameters.AddWithValue("@HamLuong", HamLuongs != null && i < HamLuongs.Length ? (object)HamLuongs[i] : DBNull.Value);
                            await cmdTP.ExecuteNonQueryAsync();
                        }
                    }
                }

                // Thêm tác dụng phụ mới
                if (TacDungPhuIds != null)
                {
                    for (int i = 0; i < TacDungPhuIds.Length; i++)
                    {
                        if (TacDungPhuIds[i] > 0)
                        {
                            using var cmdTDP = new SqlCommand("INSERT INTO CT_TAC_DUNG_PHU (MaThuoc, MaTacDungPhu, MucDo) VALUES (@MaThuoc, @MaTacDungPhu, @MucDo)", connection);
                            cmdTDP.Parameters.AddWithValue("@MaThuoc", id);
                            cmdTDP.Parameters.AddWithValue("@MaTacDungPhu", TacDungPhuIds[i]);
                            cmdTDP.Parameters.AddWithValue("@MucDo", MucDos != null && i < MucDos.Length ? (object)MucDos[i] : DBNull.Value);
                            await cmdTDP.ExecuteNonQueryAsync();
                        }
                    }
                }

                // Thêm đối tượng sử dụng mới
                if (DoiTuongIds != null)
                {
                    foreach (var doiTuongId in DoiTuongIds)
                    {
                        if (doiTuongId > 0)
                        {
                            using var cmdDT = new SqlCommand("INSERT INTO CT_DOI_TUONG (MaThuoc, MaDoiTuong) VALUES (@MaThuoc, @MaDoiTuong)", connection);
                            cmdDT.Parameters.AddWithValue("@MaThuoc", id);
                            cmdDT.Parameters.AddWithValue("@MaDoiTuong", doiTuongId);
                            await cmdDT.ExecuteNonQueryAsync();
                        }
                    }
                }

                TempData["ThongBao"] = "Cập nhật thuốc thành công!";
                return RedirectToAction(nameof(Index));
            }

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            await LoadDropdowns(conn);
            await LoadChiTietDropdowns(conn);
            return View(thuoc);
        }

        // Xóa thuốc
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new SqlCommand("sp_Admin_Thuoc_Xoa", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@MaThuoc", id);
            await cmd.ExecuteNonQueryAsync();

            TempData["ThongBao"] = "Xóa thuốc thành công!";
            return RedirectToAction(nameof(Index));
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

        private async Task LoadDropdowns(SqlConnection connection)
        {
            var nhomThuocs = new List<SelectListItem>();
            var thuongHieus = new List<SelectListItem>();
            var nuocSXs = new List<SelectListItem>();

            using (var cmd = new SqlCommand("SELECT MaNhomThuoc, TenNhomThuoc FROM NHOM_THUOC ORDER BY TenNhomThuoc", connection))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                    nhomThuocs.Add(new SelectListItem { Value = reader.GetInt32(0).ToString(), Text = reader.GetString(1) });
            }

            using (var cmd = new SqlCommand("SELECT MaThuongHieu, TenThuongHieu FROM THUONG_HIEU ORDER BY TenThuongHieu", connection))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                    thuongHieus.Add(new SelectListItem { Value = reader.GetInt32(0).ToString(), Text = reader.GetString(1) });
            }

            using (var cmd = new SqlCommand("SELECT MaNuocSX, TenNuocSX FROM NUOC_SAN_XUAT ORDER BY TenNuocSX", connection))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                    nuocSXs.Add(new SelectListItem { Value = reader.GetInt32(0).ToString(), Text = reader.GetString(1) });
            }

            ViewBag.NhomThuocs = new SelectList(nhomThuocs, "Value", "Text");
            ViewBag.ThuongHieus = new SelectList(thuongHieus, "Value", "Text");
            ViewBag.NuocSXs = new SelectList(nuocSXs, "Value", "Text");
        }

        private async Task LoadChiTietDropdowns(SqlConnection connection)
        {
            var thanhPhans = new List<dynamic>();
            var tacDungPhus = new List<dynamic>();
            var doiTuongs = new List<dynamic>();

            using (var cmd = new SqlCommand("SELECT MaThanhPhan, TenThanhPhan FROM THANH_PHAN ORDER BY TenThanhPhan", connection))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                    thanhPhans.Add(new { MaThanhPhan = reader.GetInt32(0), TenThanhPhan = reader.GetString(1) });
            }

            using (var cmd = new SqlCommand("SELECT MaTacDungPhu, TenTacDungPhu FROM TAC_DUNG_PHU ORDER BY TenTacDungPhu", connection))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                    tacDungPhus.Add(new { MaTacDungPhu = reader.GetInt32(0), TenTacDungPhu = reader.GetString(1) });
            }

            using (var cmd = new SqlCommand("SELECT MaDoiTuong, TenDoiTuong FROM DOI_TUONG_SU_DUNG ORDER BY TenDoiTuong", connection))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                    doiTuongs.Add(new { MaDoiTuong = reader.GetInt32(0), TenDoiTuong = reader.GetString(1) });
            }

            ViewBag.ThanhPhans = thanhPhans;
            ViewBag.TacDungPhus = tacDungPhus;
            ViewBag.DoiTuongs = doiTuongs;
        }
    }
}
