using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_NhaThuoc.Data;
using QL_NhaThuoc.Services;
using QL_NhaThuoc.ViewModels;

namespace QL_NhaThuoc.Controllers
{
    public class UserController : Controller
    {
        private readonly QL_NhaThuocDbContext _context;
        private readonly OtpServiceVietnamese _otpService;
        private const string UserIdCookie = "UserId";

        public UserController(QL_NhaThuocDbContext context, OtpServiceVietnamese otpService)
        {
            _context = context;
            _otpService = otpService;
        }

        // GET: User/PhoneLogin
        public IActionResult PhoneLogin()
        {
            if (GetCurrentUserId().HasValue)
                return RedirectToAction("Index", "Home");

            return View();
        }

        // POST: User/PhoneLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PhoneLogin(PhoneLoginVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _otpService.GenerateAndSendOtpAsync(model.PhoneNumber);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            TempData["SoDienThoai"] = model.PhoneNumber;
            return RedirectToAction(nameof(VerifyOtp));
        }

        // GET: User/VerifyOtp
        public IActionResult VerifyOtp()
        {
            var soDienThoai = TempData["SoDienThoai"]?.ToString();
            if (string.IsNullOrEmpty(soDienThoai))
                return RedirectToAction(nameof(PhoneLogin));

            TempData.Keep("SoDienThoai");
            return View(new VerifyOtpVM { PhoneNumber = soDienThoai });
        }

        // POST: User/VerifyOtp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(VerifyOtpVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _otpService.VerifyOtpAsync(model.PhoneNumber, model.OTP);
            
            if (result.Success && result.NguoiDung != null)
            {
                // Lưu vào Cookie (30 ngày) - giữ đăng nhập khi restart app
                LuuDangNhap(result.NguoiDung.MaNguoiDung);

                TempData["ThongBao"] = "Đăng nhập thành công!";
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", result.Message);
            return View(model);
        }

        // GET: User/Profile
        public async Task<IActionResult> Profile()
        {
            var maNguoiDung = GetCurrentUserId();
            if (!maNguoiDung.HasValue)
                return RedirectToAction(nameof(PhoneLogin));

            var nguoiDung = await _context.NGUOI_DUNG.FindAsync(maNguoiDung.Value);
            if (nguoiDung == null)
                return NotFound();

            // Lấy thống kê
            ViewBag.TongDonHang = await _context.DON_HANG.CountAsync(d => d.MaNguoiDung == maNguoiDung);
            ViewBag.TongChiTieu = await _context.DON_HANG
                .Where(d => d.MaNguoiDung == maNguoiDung && d.TrangThai == "Hoàn thành")
                .SumAsync(d => d.TongTien ?? 0);

            return View(nguoiDung);
        }

        // POST: User/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string hoTen, string diaChi)
        {
            var maNguoiDung = GetCurrentUserId();
            if (!maNguoiDung.HasValue)
                return RedirectToAction(nameof(PhoneLogin));

            var nguoiDung = await _context.NGUOI_DUNG.FindAsync(maNguoiDung.Value);
            if (nguoiDung != null)
            {
                nguoiDung.HoTen = hoTen;
                nguoiDung.DiaChi = diaChi;
                await _context.SaveChangesAsync();
                TempData["ThongBao"] = "Cập nhật thông tin thành công!";
            }

            return RedirectToAction(nameof(Profile));
        }

        // GET: User/DiaChi
        public async Task<IActionResult> DiaChi()
        {
            var maNguoiDung = GetCurrentUserId();
            if (!maNguoiDung.HasValue)
                return RedirectToAction(nameof(PhoneLogin));

            var nguoiDung = await _context.NGUOI_DUNG.FindAsync(maNguoiDung.Value);
            return View(nguoiDung);
        }

        // GET: User/Logout
        public IActionResult Logout()
        {
            Response.Cookies.Delete(UserIdCookie);
            TempData["ThongBao"] = "Đăng xuất thành công!";
            return RedirectToAction("Index", "Home");
        }

        #region Helper Methods - Cookie based
        private int? GetCurrentUserId()
        {
            if (Request.Cookies.TryGetValue(UserIdCookie, out var userIdStr) && int.TryParse(userIdStr, out var userId))
                return userId;
            return null;
        }

        private void LuuDangNhap(int maNguoiDung)
        {
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(30),
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            };
            Response.Cookies.Append(UserIdCookie, maNguoiDung.ToString(), cookieOptions);
        }
        #endregion
    }
}
