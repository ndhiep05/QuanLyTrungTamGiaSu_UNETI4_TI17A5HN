// Họ và tên: <Nguyễn Đình Hiệp>
// Mã sinh viên: 23103100230
// Nội dung thực hiện: ViewModel dùng chung cho giao diện (SV1).
using System.ComponentModel.DataAnnotations;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models;

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.ViewModels;

public class DiemDanhDong
{
    public int MaHocVien { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public string TrangThaiDiemDanh { get; set; } = Models.TrangThaiDiemDanh.CoMat;
    [StringLength(500)]
    public string? GhiChu { get; set; }
}

/// <summary>Màn hình điểm danh cả lớp cho một buổi học.</summary>
public class DiemDanhTheoBuoiViewModel
{
    public int MaLichHoc { get; set; }
    public string TenLop { get; set; } = string.Empty;
    public string TenGiaSu { get; set; } = string.Empty;
    public DateTime ThoiGianBatDau { get; set; }
    public DateTime ThoiGianKetThuc { get; set; }
    public List<DiemDanhDong> DanhSach { get; set; } = new();
}
