// Họ và tên: Nguyễn Trọng Chuyển
// Mã sinh viên: [Điền MSV của bạn]
// Nhiệm vụ: Module 3 - ViewModel quản lý danh sách học viên
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models;

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.ViewModels
{
    public class HocVienViewModel
    {
        public List<HocVien> DanhSachHocVien { get; set; } = new List<HocVien>();
        public string? TuKhoaTimKiem { get; set; }
        public int TrangHienTai { get; set; } = 1;
        public int TongSoTrang { get; set; }
        public int TongSoBanGhi { get; set; }
    }
}