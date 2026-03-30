using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace QL_NhaThuoc.Filters
{
    /// <summary>
    /// Filter kiểm tra quyền Admin
    /// </summary>
    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var request = context.HttpContext.Request;
            
            // Check Session first
            var vaiTro = session.GetString("VaiTro");
            var maNguoiDung = session.GetInt32("MaNguoiDung");

            // If session is empty, try to restore from cookies
            if (!maNguoiDung.HasValue || string.IsNullOrEmpty(vaiTro))
            {
                // Try to get from cookies
                if (request.Cookies.TryGetValue("AdminLoggedIn", out var adminCookie) && 
                    adminCookie == "true" &&
                    request.Cookies.TryGetValue("MaNguoiDung", out var maNguoiDungCookie) &&
                    request.Cookies.TryGetValue("VaiTro", out var vaiTroCookie))
                {
                    // Restore session from cookies
                    if (int.TryParse(maNguoiDungCookie, out int parsedMaNguoiDung))
                    {
                        session.SetInt32("MaNguoiDung", parsedMaNguoiDung);
                        session.SetString("VaiTro", vaiTroCookie);
                        
                        maNguoiDung = parsedMaNguoiDung;
                        vaiTro = vaiTroCookie;
                    }
                }
            }

            // Kiểm tra đã đăng nhập và có quyền Admin
            if (!maNguoiDung.HasValue || vaiTro != "Admin")
            {
                // Redirect về trang đăng nhập Admin
                context.Result = new RedirectToActionResult("Login", "Auth", new { area = "Admin" });
            }

            base.OnActionExecuting(context);
        }
    }

    /// <summary>
    /// Filter kiểm tra đã đăng nhập (User hoặc Admin)
    /// </summary>
    public class UserAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var maNguoiDung = session.GetInt32("MaNguoiDung");

            if (!maNguoiDung.HasValue)
            {
                context.Result = new RedirectToActionResult("PhoneLogin", "User", new { area = "" });
            }

            base.OnActionExecuting(context);
        }
    }
}
