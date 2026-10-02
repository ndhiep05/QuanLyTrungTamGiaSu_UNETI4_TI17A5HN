// Họ và tên: <Điền họ và tên>
// Mã sinh viên: 23103100230
// Nội dung thực hiện: Thiết kế Entity dùng chung cho cả nhóm (khung CSDL).
using System.ComponentModel.DataAnnotations;

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models;

public class LichHoc
{
    [Key]
    public int MaLichHoc { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn lớp")]
    [Display(Name = "Lớp học")]
    public int MaLop { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn gia sư")]
    [Display(Name = "Gia sư")]
    public int MaGiaSu { get; set; }

    [Required(ErrorMessage = "Thời gian bắt đầu là bắt buộc")]
    [DataType(DataType.DateTime)]
    [Display(Name = "Thời gian bắt đầu")]
    public DateTime ThoiGianBatDau { get; set; }

    [Required(ErrorMessage = "Thời gian kết thúc là bắt buộc")]
    [DataType(DataType.DateTime)]
    [Display(Name = "Thời gian kết thúc")]
    public DateTime ThoiGianKetThuc { get; set; }

    [StringLength(100)]
    [Display(Name = "Phòng học / Hình thức")]
    public string? PhongHocHoacHinhThuc { get; set; }

    [StringLength(500)]
    [Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "Trạng thái")]
    public string TrangThai { get; set; } = TrangThaiLich.DaLenLich;

    public LopHoc? LopHoc { get; set; }
    public GiaSu? GiaSu { get; set; }
    public ICollection<DiemDanh> DiemDanhs { get; set; } = new List<DiemDanh>();
}
