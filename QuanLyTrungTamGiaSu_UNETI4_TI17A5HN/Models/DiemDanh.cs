// Họ và tên: <Điền họ và tên>
// Mã sinh viên: 23103100230
// Nội dung thực hiện: Thiết kế Entity dùng chung cho cả nhóm (khung CSDL).
using System.ComponentModel.DataAnnotations;

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models;

public class DiemDanh
{
    [Key]
    public int MaDiemDanh { get; set; }

    [Required]
    [Display(Name = "Buổi học")]
    public int MaLichHoc { get; set; }

    [Required]
    [Display(Name = "Học viên")]
    public int MaHocVien { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "Trạng thái điểm danh")]
    public string TrangThaiDiemDanh { get; set; } = Models.TrangThaiDiemDanh.CoMat;

    [DataType(DataType.DateTime)]
    [Display(Name = "Thời gian ghi nhận")]
    public DateTime ThoiGianGhiNhan { get; set; }

    [StringLength(500)]
    [Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }

    public LichHoc? LichHoc { get; set; }
    public HocVien? HocVien { get; set; }
}
