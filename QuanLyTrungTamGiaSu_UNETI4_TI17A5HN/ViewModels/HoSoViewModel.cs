using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.ViewModels
{
    public class HoSoViewModel
    {
        public int MaTaiKhoan { get; set; }

        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Vai trò")]
        public string VaiTro { get; set; } = string.Empty;

        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = string.Empty;

        [Display(Name = "Ảnh đại diện")]
        public string? AnhDaiDien { get; set; }

        // Thông tin liên hệ chung (cho cả Gia sư và Học viên)
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(15, ErrorMessage = "Số điện thoại không quá 15 số")]
        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [StringLength(200, ErrorMessage = "Địa chỉ không quá 200 ký tự")]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        // Thông tin chuyên môn (Dành cho Gia sư)
        [StringLength(100)]
        [Display(Name = "Trình độ / Bằng cấp")]
        public string? TrinhDo { get; set; }

        [Range(0, 100, ErrorMessage = "Số năm kinh nghiệm không hợp lệ")]
        [Display(Name = "Kinh nghiệm (năm)")]
        public int? KinhNghiem { get; set; }

        public List<string> DanhSachChuyenMon { get; set; } = new();
        public int SoLopPhanCong { get; set; }

        // Thông tin học tập (Dành cho Học viên)
        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime? NgaySinh { get; set; }

        [StringLength(10)]
        [Display(Name = "Giới tính")]
        public string? GioiTinh { get; set; }

        [StringLength(100)]
        [Display(Name = "Trường học")]
        public string? TruongHoc { get; set; }

        [StringLength(50)]
        [Display(Name = "Lớp học hiện tại")]
        public string? LopHocHienTai { get; set; }

        [StringLength(50)]
        [Display(Name = "Trình độ hiện tại")]
        public string? TrinhDoHienTai { get; set; }

        public int SoLopDangKy { get; set; }
        public List<string> DanhSachLopHoc { get; set; } = new();
    }
}
