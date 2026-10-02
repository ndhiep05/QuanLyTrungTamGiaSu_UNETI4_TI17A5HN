// Họ và tên: <Điền họ và tên>
// Mã sinh viên: 23103100230
// Nội dung thực hiện: Thiết kế Entity dùng chung cho cả nhóm (khung CSDL).
using System.ComponentModel.DataAnnotations;

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models;

public class MonHoc
{
    [Key]
    public int MaMonHoc { get; set; }

    [Required(ErrorMessage = "Tên môn học là bắt buộc")]
    [StringLength(100)]
    [Display(Name = "Tên môn học")]
    public string TenMonHoc { get; set; } = string.Empty;

    [StringLength(1000)]
    [Display(Name = "Mô tả")]
    public string? MoTa { get; set; }

    [Required]
    [StringLength(30)]
    [Display(Name = "Cấp độ")]
    public string CapDo { get; set; } = "Cơ bản";   // Cơ bản / Trung bình / Nâng cao

    [Required]
    [StringLength(20)]
    [Display(Name = "Trạng thái")]
    public string TrangThai { get; set; } = TrangThaiChung.HoatDong;

    public ICollection<ChuyenMonGiaSu> ChuyenMons { get; set; } = new List<ChuyenMonGiaSu>();
    public ICollection<LopHoc> LopHocs { get; set; } = new List<LopHoc>();
}
