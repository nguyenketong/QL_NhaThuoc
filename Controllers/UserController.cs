using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using QL_NhaThuoc.ViewModels;
using QL_NhaThuoc.Services;
using System.Data;

namespace QL_NhaThuoc.Controllers
{
    public class UserController : Controller
    {
        private readonly OtpServiceVietnamese _otpService;
        private readonly string _connectionString;

        public UserController(OtpServiceVietnamese otpService, IConfiguration configuration)
        {
            _otpService = otpService;
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        // =============================================
        // TRANG CÁ NHÂN
        // =============================================

        // Trang thông tin cá nhân
        public async Task<IActionResult> Profile()
        {
            var maNguoiDung = HttpContext.Session.GetInt32("MaNguoiDung");
            if (!maNguoiDung.HasValue)
            {
                return RedirectToAction("PhoneLogin");
            }

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // Lấy thông tin người dùng
            dynamic? nguoiDung = null;
            using (var cmd = new SqlCommand("SELECT * FROM NGUOI_DUNG WHERE MaNguoiDung = @MaNguoiDung", connection))
            {
                cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung.Value);
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    nguoiDung = new
                    {
                        MaNguoiDung = reader.GetInt32(reader.GetOrdinal("MaNguoiDung")),
                        HoTen = reader.IsDBNull(reader.GetOrdinal("HoTen")) ? "" : reader.GetString(reader.GetOrdinal("HoTen")),
                        SoDienThoai = reader.GetString(reader.GetOrdinal("SoDienThoai")),
                        DiaChi = reader.IsDBNull(reader.GetOrdinal("DiaChi")) ? "" : reader.GetString(reader.GetOrdinal("DiaChi")),
                        VaiTro = reader.IsDBNull(reader.GetOrdinal("VaiTro")) ? "" : reader.GetString(reader.GetOrdinal("VaiTro")),
                        NgayTao = reader.IsDBNull(reader.GetOrdinal("NgayTao")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("NgayTao"))
                    };
                }
            }

            if (nguoiDung == null)
            {
                return RedirectToAction("PhoneLogin");
            }

            // Thống kê đơn hàng
            using (var cmd = new SqlCommand("sp_NguoiDung_ThongKe", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung.Value);
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    ViewBag.TongDonHang = reader.GetInt32(0);
                    ViewBag.TongChiTieu = reader.GetDecimal(1);
                }
            }

            return View(nguoiDung);
        }

        // Cập nhật thông tin cá nhân
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string hoTen, string diaChi)
        {
            var maNguoiDung = HttpContext.Session.GetInt32("MaNguoiDung");
            if (!maNguoiDung.HasValue)
            {
                return RedirectToAction("PhoneLogin");
            }

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new SqlCommand("sp_NguoiDung_CapNhatThongTin", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung.Value);
            cmd.Parameters.AddWithValue("@HoTen", hoTen ?? "");
            cmd.Parameters.AddWithValue("@DiaChi", (object?)diaChi ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync();

            // Cập nhật session
            HttpContext.Session.SetString("HoTen", hoTen ?? "");

            TempData["ThongBao"] = "Cập nhật thông tin thành công!";
            return RedirectToAction("Profile");
        }

        // Quản lý địa chỉ
        public async Task<IActionResult> DiaChi()
        {
            var maNguoiDung = HttpContext.Session.GetInt32("MaNguoiDung");
            if (!maNguoiDung.HasValue)
            {
                return RedirectToAction("PhoneLogin");
            }

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new SqlCommand("SELECT * FROM NGUOI_DUNG WHERE MaNguoiDung = @MaNguoiDung", connection);
            cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung.Value);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return RedirectToAction("PhoneLogin");
            }

            var nguoiDung = new
            {
                MaNguoiDung = reader.GetInt32(reader.GetOrdinal("MaNguoiDung")),
                HoTen = reader.IsDBNull(reader.GetOrdinal("HoTen")) ? "" : reader.GetString(reader.GetOrdinal("HoTen")),
                SoDienThoai = reader.GetString(reader.GetOrdinal("SoDienThoai")),
                DiaChi = reader.IsDBNull(reader.GetOrdinal("DiaChi")) ? "" : reader.GetString(reader.GetOrdinal("DiaChi"))
            };

            return View(nguoiDung);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // =============================================
        // ĐĂNG NHẬP BẰNG SỐ ĐIỆN THOẠI VÀ OTP
        // =============================================

        [HttpGet]
        public IActionResult PhoneLogin()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PhoneLogin(PhoneLoginVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _otpService.GenerateAndSendOtpAsync(model.PhoneNumber);
            
            if (result.Success)
            {
                TempData["PhoneNumber"] = model.PhoneNumber;
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction("VerifyOtp");
            }

            ModelState.AddModelError("", result.Message);
            return View(model);
        }

        [HttpGet]
        public IActionResult VerifyOtp()
        {
            var phoneNumber = TempData["PhoneNumber"]?.ToString();
            if (string.IsNullOrEmpty(phoneNumber))
            {
                return RedirectToAction("PhoneLogin");
            }

            TempData.Keep("PhoneNumber");
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            
            return View(new VerifyOtpVM { PhoneNumber = phoneNumber });
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOtp(VerifyOtpVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _otpService.VerifyOtpAsync(model.PhoneNumber, model.OTP);
            
            if (result.Success && result.NguoiDung != null)
            {
                // Luu thong tin nguoi dung vao session
                HttpContext.Session.SetInt32("MaNguoiDung", result.NguoiDung.MaNguoiDung);
                HttpContext.Session.SetString("HoTen", result.NguoiDung.HoTen ?? "");
                HttpContext.Session.SetString("SoDienThoai", result.NguoiDung.SoDienThoai);
                
                TempData["ThongBao"] = "Đăng nhập thành công!";
                return RedirectToAction("Profile");
            }

            ModelState.AddModelError("", result.Message);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResendOtp(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                return Json(new { success = false, message = "Số điện thoại không hợp lệ" });
            }

            var result = await _otpService.GenerateAndSendOtpAsync(phoneNumber);
            return Json(new { success = result.Success, message = result.Message });
        }
    }
}
