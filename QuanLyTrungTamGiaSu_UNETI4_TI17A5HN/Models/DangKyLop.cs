// Họ và tên: <Điền họ và tên>
// Mã sinh viên: 23103100230
// Nội dung thực hiện: Thiết kế Entity dùng chung cho cả nhóm (khung CSDL).
using System.ComponentModel.DataAnnotations;

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models;

public class DangKyLop
{
    [Key]
    public int MaDangKy { get; set; }

    [Required]
    [Display(Name = "Học viên")]
    public int MaHocVien { get; set; }

    [Required]
    [Display(Name = "Lớp học")]
    public int MaLop { get; set; }

    [DataType(DataType.DateTime)]
    [Display(Name = "Ngày đăng ký")]
    public DateTime NgayDangKy { get; set; }   // do hệ thống gán (DateTime.Now), không cho người dùng nhập

    [Required]
    [StringLength(20)]
    [Display(Name = "Trạng thái")]
    public string TrangThai { get; set; } = TrangThaiDangKy.ChoXacNhan;

    [StringLength(500)]
    [Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }

    public HocVien? HocVien { get; set; }
    public LopHoc? LopHoc { get; set; }
    public ICollection<ThanhToanHocPhi> ThanhToans { get; set; } = new List<ThanhToanHocPhi>();
}
