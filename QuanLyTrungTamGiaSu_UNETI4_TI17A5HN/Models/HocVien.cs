// Họ và tên: <Điền họ và tên>
// Mã sinh viên: 23103100230
// Nội dung thực hiện: Thiết kế Entity dùng chung cho cả nhóm (khung CSDL).
using System.ComponentModel.DataAnnotations;

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models;

public class HocVien
{
    [Key]
    public int MaHocVien { get; set; }

    [Required]
    public int MaTaiKhoan { get; set; }

    [Required(ErrorMessage = "Họ tên là bắt buộc")]
    [StringLength(100)]
    [Display(Name = "Họ tên")]
    public string HoTen { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
    [DataType(DataType.Date)]
    [Display(Name = "Ngày sinh")]
    public DateTime NgaySinh { get; set; }

    [Required]
    [StringLength(10)]
    [Display(Name = "Giới tính")]
    public string GioiTinh { get; set; } = "Nam";

    [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [StringLength(15)]
    [Display(Name = "Số điện thoại")]
    public string SoDienThoai { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [StringLength(200)]
    [Display(Name = "Địa chỉ")]
    public string? DiaChi { get; set; }

    [StringLength(50)]
    [Display(Name = "Trình độ hiện tại")]
    public string? TrinhDoHienTai { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "Trạng thái")]
    public string TrangThai { get; set; } = TrangThaiChung.HoatDong;

    public TaiKhoan? TaiKhoan { get; set; }
    public ICollection<DangKyLop> DangKyLops { get; set; } = new List<DangKyLop>();
    public ICollection<DiemDanh> DiemDanhs { get; set; } = new List<DiemDanh>();
}
