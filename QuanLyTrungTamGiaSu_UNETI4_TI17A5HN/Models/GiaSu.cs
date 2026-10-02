// Họ và tên: <Điền họ và tên>
// Mã sinh viên: 23103100230
// Nội dung thực hiện: Thiết kế Entity dùng chung cho cả nhóm (khung CSDL).
using System.ComponentModel.DataAnnotations;

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models;

public class GiaSu
{
    [Key]
    public int MaGiaSu { get; set; }

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

    [Required]
    [StringLength(50)]
    [Display(Name = "Trình độ")]
    public string TrinhDo { get; set; } = string.Empty;   // Cao đẳng / Đại học / Thạc sĩ / Tiến sĩ

    [Range(0, 60, ErrorMessage = "Số năm kinh nghiệm phải từ 0 trở lên")]
    [Display(Name = "Số năm kinh nghiệm")]
    public int SoNamKinhNghiem { get; set; }

    [StringLength(2000)]
    [Display(Name = "Giới thiệu")]
    public string? GioiThieu { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "Trạng thái")]
    public string TrangThai { get; set; } = TrangThaiChung.HoatDong;

    public TaiKhoan? TaiKhoan { get; set; }
    public ICollection<ChuyenMonGiaSu> ChuyenMons { get; set; } = new List<ChuyenMonGiaSu>();
    public ICollection<PhanCongGiaSu> PhanCongs { get; set; } = new List<PhanCongGiaSu>();
    public ICollection<LichHoc> LichHocs { get; set; } = new List<LichHoc>();
}
