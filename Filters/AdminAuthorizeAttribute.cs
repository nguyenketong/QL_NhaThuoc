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
            var vaiTro = session.GetString("VaiTro");
            var maNguoiDung = session.GetInt32("MaNguoiDung");

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
