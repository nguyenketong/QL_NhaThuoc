using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_NhaThuoc.Data;
using QL_NhaThuoc.Services;

namespace QL_NhaThuoc.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AuthController : Controller
    {
        private readonly QL_NhaThuocDbContext _context;
        private readonly PasswordService _passwordService;

        public AuthController(QL_NhaThuocDbContext context, PasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        // Trang đăng nhập Admin
        [HttpGet]
        public IActionResult Login()
        {
            // Nếu đã đăng nhập Admin thì redirect về Dashboard
            if (HttpContext.Session.GetString("VaiTro") == "Admin")
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }

        // Xử lý đăng nhập Admin (dùng SĐT + mật khẩu hash)
        [HttpPost]
        public async Task<IActionResult> Login(string soDienThoai, string matKhau)
        {
            // Tìm người dùng có quyền Admin
            var admin = await _context.NGUOI_DUNG
                .FirstOrDefaultAsync(n => n.SoDienThoai == soDienThoai && n.VaiTro == "Admin");

            if (admin == null)
            {
                ViewBag.Error = "Tài khoản không có quyền quản trị!";
                return View();
            }

            // Kiểm tra mật khẩu
            bool matKhauHopLe = false;
            bool dangDungMatKhauMacDinh = false;

            if (!string.IsNullOrEmpty(admin.MatKhauHash))
            {
                // Xác thực bằng mật khẩu đã hash
                matKhauHopLe = _passwordService.VerifyPassword(matKhau, admin.MatKhauHash);
                
                // Kiểm tra xem có đang dùng mật khẩu mặc định không
                dangDungMatKhauMacDinh = _passwordService.VerifyPassword("admin123", admin.MatKhauHash);
            }
            else
            {
                // Fallback: Nếu chưa có hash, dùng mật khẩu mặc định và tự động hash
                if (matKhau == "admin123")
                {
                    matKhauHopLe = true;
                    dangDungMatKhauMacDinh = true;
                    // Tự động hash mật khẩu mặc định và lưu vào DB
                    admin.MatKhauHash = _passwordService.HashPassword("admin123");
                    await _context.SaveChangesAsync();
                }
            }

            if (!matKhauHopLe)
            {
                ViewBag.Error = "Mật khẩu không đúng!";
                return View();
            }

            // Lưu session
            HttpContext.Session.SetInt32("MaNguoiDung", admin.MaNguoiDung);
            HttpContext.Session.SetString("HoTen", admin.HoTen ?? admin.SoDienThoai);
            HttpContext.Session.SetString("SoDienThoai", admin.SoDienThoai);
            HttpContext.Session.SetString("VaiTro", "Admin");

            // Lưu cookie ghi nhớ đăng nhập (7 ngày) - Backup cho session
            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(7),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax, // Changed from Strict to Lax for better compatibility
                IsEssential = true
            };
            
            Response.Cookies.Append("AdminLoggedIn", "true", cookieOptions);
            Response.Cookies.Append("MaNguoiDung", admin.MaNguoiDung.ToString(), cookieOptions);
            Response.Cookies.Append("VaiTro", "Admin", cookieOptions);

            // Thông báo cảnh báo nếu đang dùng mật khẩu mặc định
            if (dangDungMatKhauMacDinh)
            {
                TempData["CanhBaoMatKhau"] = "⚠️ Bạn đang sử dụng mật khẩu mặc định. Vui lòng đổi mật khẩu ngay để bảo mật tài khoản!";
            }
            else
            {
                TempData["ThongBao"] = "Đăng nhập Admin thành công!";
            }
            
            return RedirectToAction("Index", "Dashboard");
        }

        // GET: Đổi mật khẩu Admin
        [HttpGet]
        public IActionResult DoiMatKhau()
        {
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
                return RedirectToAction("Login");
            
            return View();
        }

        // POST: Đổi mật khẩu Admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiMatKhau(string matKhauCu, string matKhauMoi, string xacNhanMatKhau)
        {
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
                return RedirectToAction("Login");

            // Validate
            if (string.IsNullOrEmpty(matKhauMoi) || matKhauMoi.Length < 6)
            {
                ViewBag.Error = "Mật khẩu mới phải có ít nhất 6 ký tự!";
                return View();
            }

            if (matKhauMoi != xacNhanMatKhau)
            {
                ViewBag.Error = "Xác nhận mật khẩu không khớp!";
                return View();
            }

            var maNguoiDung = HttpContext.Session.GetInt32("MaNguoiDung");
            var admin = await _context.NGUOI_DUNG.FindAsync(maNguoiDung);

            if (admin == null)
            {
                ViewBag.Error = "Không tìm thấy tài khoản!";
                return View();
            }

            // Kiểm tra mật khẩu cũ
            bool matKhauCuDung = false;
            if (!string.IsNullOrEmpty(admin.MatKhauHash))
            {
                matKhauCuDung = _passwordService.VerifyPassword(matKhauCu, admin.MatKhauHash);
            }
            else
            {
                // Nếu chưa có hash, kiểm tra mật khẩu mặc định
                matKhauCuDung = matKhauCu == "admin123";
            }

            if (!matKhauCuDung)
            {
                ViewBag.Error = "Mật khẩu cũ không đúng!";
                return View();
            }

            // Hash và lưu mật khẩu mới
            admin.MatKhauHash = _passwordService.HashPassword(matKhauMoi);
            await _context.SaveChangesAsync();

            TempData["ThongBao"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Index", "Dashboard");
        }

        // Đăng xuất Admin
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("AdminLoggedIn");
            return RedirectToAction("Login");
        }
    }
}
