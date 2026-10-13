using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models;

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Filters
{
    /// <summary>
    /// Bộ lọc chỉ cho phép Quản trị viên (Admin) truy cập.
    /// Kế thừa từ bộ lọc phân quyền dùng chung PhanQuyenAttribute.
    /// </summary>
    public class AdminOnlyAttribute : PhanQuyenAttribute
    {
        public AdminOnlyAttribute() : base(VaiTro.Admin)
        {
        }
    }
}