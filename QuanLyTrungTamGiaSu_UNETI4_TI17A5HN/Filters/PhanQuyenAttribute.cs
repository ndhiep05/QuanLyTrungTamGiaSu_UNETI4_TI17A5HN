using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models;

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Filters
{
    /// <summary>
    /// Bộ lọc phân quyền tái sử dụng cho Controller/Action.
    /// Cho phép cấu hình danh sách vai trò được phép truy cập (Admin, GiaSu, HocVien).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class PhanQuyenAttribute : ActionFilterAttribute
    {
        private readonly string[] _vaiTroChoPhep;

        public PhanQuyenAttribute(params string[] vaiTroChoPhep)
        {
            _vaiTroChoPhep = vaiTroChoPhep ?? Array.Empty<string>();
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            var session = httpContext.Session;

            // Kiểm tra trạng thái đăng nhập
            var maTaiKhoan = session.GetInt32("MaTaiKhoan");
            var vaiTro = session.GetString("VaiTro");

            if (maTaiKhoan == null || string.IsNullOrEmpty(vaiTro))
            {
                // Chưa đăng nhập -> chuyển hướng về trang đăng nhập
                context.Result = new RedirectToActionResult(
                    "DangNhap",
                    "TaiKhoan",
                    new { returnUrl = httpContext.Request.Path + httpContext.Request.QueryString }
                );
                return;
            }

            // Nếu có chỉ định vai trò và vai trò người dùng không khớp -> từ chối truy cập
            if (_vaiTroChoPhep.Length > 0 && !_vaiTroChoPhep.Contains(vaiTro))
            {
                context.Result = new RedirectToActionResult(
                    "KhongCoQuyen",
                    "Home",
                    null
                );
                return;
            }

            base.OnActionExecuting(context);
        }
    }

    /// <summary>
    /// Chỉ cho phép vai trò Admin
    /// </summary>
    public class GiaSuOnlyAttribute : PhanQuyenAttribute
    {
        public GiaSuOnlyAttribute() : base(VaiTro.GiaSu) { }
    }

    /// <summary>
    /// Chỉ cho phép vai trò Học viên
    /// </summary>
    public class HocVienOnlyAttribute : PhanQuyenAttribute
    {
        public HocVienOnlyAttribute() : base(VaiTro.HocVien) { }
    }
}
