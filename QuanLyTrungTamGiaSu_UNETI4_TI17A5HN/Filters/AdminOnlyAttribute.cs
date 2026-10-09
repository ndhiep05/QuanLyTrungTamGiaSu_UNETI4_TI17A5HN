using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Filters
{
    public class AdminOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(
            ActionExecutingContext context)
        {
            var vaiTro = context.HttpContext.Session
                .GetString("VaiTro");

            if (string.IsNullOrEmpty(vaiTro))
            {
                context.Result = new RedirectToActionResult(
                    "DangNhap", "TaiKhoan", null);
                return;
            }

            if (vaiTro != "Admin")
            {
                context.Result = new RedirectToActionResult(
                    "Index", "Home", null);
            }
        }
    }
}
