// Họ và tên: <Điền họ và tên>
// Mã sinh viên: 23103100230
// Nội dung thực hiện: Thiết kế Entity dùng chung cho cả nhóm (khung CSDL).
using System.ComponentModel.DataAnnotations;

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models;

public class PhanCongGiaSu
{
    [Key]
    public int MaPhanCong { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn lớp")]
    [Display(Name = "Lớp học")]
    public int MaLop { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn gia sư")]
    [Display(Name = "Gia sư")]
    public int MaGiaSu { get; set; }

    [DataType(DataType.DateTime)]
    [Display(Name = "Ngày phân công")]
    public DateTime NgayPhanCong { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "Trạng thái")]
    public string TrangThai { get; set; } = "Đang phụ trách";

    [StringLength(500)]
    [Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }

    public LopHoc? LopHoc { get; set; }
    public GiaSu? GiaSu { get; set; }
}
