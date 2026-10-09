// Họ và tên: <Điền họ và tên>
// Mã sinh viên: 23103100230
// Nội dung thực hiện: Thiết kế Entity dùng chung cho cả nhóm (khung CSDL).
using System.ComponentModel.DataAnnotations;

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models;

public class ChuyenMonGiaSu
{
    [Key]
    public int MaChuyenMon { get; set; }

    [Required]
    [Display(Name = "Gia sư")]
    public int MaGiaSu { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn môn học")]
    [Display(Name = "Môn học")]
    public int MaMonHoc { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Trình độ chuyên môn")]
    public string TrinhDoChuyenMon { get; set; } = string.Empty;

    [Range(0, 60, ErrorMessage = "Số năm kinh nghiệm phải từ 0 trở lên")]
    [Display(Name = "Số năm kinh nghiệm môn")]
    public int SoNamKinhNghiemMon { get; set; }

    [StringLength(500)]
    [Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "Trạng thái")]
    public string TrangThai { get; set; } = TrangThaiChung.HoatDong;   // "Hoạt động" = đang hiệu lực

    public GiaSu? GiaSu { get; set; }
    public MonHoc? MonHoc { get; set; }
}
