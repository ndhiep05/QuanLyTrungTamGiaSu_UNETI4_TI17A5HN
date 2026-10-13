using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Filters
{
    public class AdminOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(
            ActionExecutingContext context)
        {
            // Kiểm tra người dùng đã đăng nhập chưa
            var maTaiKhoan = context.HttpContext.Session
                .GetString("MaTaiKhoan");

            var vaiTro = context.HttpContext.Session
                .GetString("VaiTro");

            if (string.IsNullOrEmpty(maTaiKhoan))
            {
                context.Result = new RedirectToActionResult(
                    "Login",
                    "TaiKhoan",
                    null
                );

                return;
            }

            // Chỉ cho phép tài khoản Admin truy cập
            if (vaiTro != "Admin")
            {
                context.Result = new RedirectToActionResult(
                    "AccessDenied",
                    "TaiKhoan",
                    null
                );

                return;
            }

            base.OnActionExecuting(context);
        }
    }
}