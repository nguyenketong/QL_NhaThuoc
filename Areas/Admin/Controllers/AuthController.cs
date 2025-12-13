using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_NhaThuoc.Data;

namespace QL_NhaThuoc.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AuthController : Controller
    {
        private readonly QL_NhaThuocDbContext _context;

        public AuthController(QL_NhaThuocDbContext context)
        {
            _context = context;
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

        // Xử lý đăng nhập Admin (dùng SĐT + mật khẩu đơn giản)
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

            // Mật khẩu mặc định cho Admin: "admin123" (trong thực tế nên hash)
            if (matKhau != "admin123")
            {
                ViewBag.Error = "Mật khẩu không đúng!";
                return View();
            }

            // Lưu session
            HttpContext.Session.SetInt32("MaNguoiDung", admin.MaNguoiDung);
            HttpContext.Session.SetString("HoTen", admin.HoTen ?? admin.SoDienThoai);
            HttpContext.Session.SetString("SoDienThoai", admin.SoDienThoai);
            HttpContext.Session.SetString("VaiTro", "Admin");

            // Lưu cookie ghi nhớ đăng nhập (7 ngày)
            Response.Cookies.Append("AdminLoggedIn", "true", new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(7),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            TempData["ThongBao"] = "Đăng nhập Admin thành công!";
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
