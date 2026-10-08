// Họ và tên: <Nguyễn Đình Hiệp>
// Mã sinh viên: 23103100230
// Nội dung thực hiện: ViewModel dùng chung cho giao diện (SV1).

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.ViewModels;

/// <summary>10 chỉ số Dashboard theo mục 9.5 của đề.</summary>
public class DashboardViewModel
{
    public DateTime? TuNgay { get; set; }
    public DateTime? DenNgay { get; set; }

    public int TongMonHoc { get; set; }
    public int GiaSuHoatDong { get; set; }
    public int TongHocVien { get; set; }
    public int LopDangMoDangKy { get; set; }
    public int LopDangHoc { get; set; }
    public int DangKyChoXacNhan { get; set; }
    public int LopDaDuSiSo { get; set; }
    public int BuoiHocSapToi { get; set; }
    public decimal TongHocPhiDaThu { get; set; }
    public int DangKyConThieuHocPhi { get; set; }
}
