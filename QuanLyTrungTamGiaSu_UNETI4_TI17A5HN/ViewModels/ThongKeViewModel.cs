// Họ và tên: < Nguyễn Đình Hiệp>
// Mã sinh viên: 23103100230
// Nội dung thực hiện: ViewModel dùng chung cho giao diện (SV1).

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.ViewModels;

public class MucThongKe
{
    public string Ten { get; set; } = string.Empty;
    public int SoLuong { get; set; }
}

/// <summary>Thống kê bằng LINQ theo mục 9.6 của đề.</summary>
public class ThongKeViewModel
{
    public List<MucThongKe> LopTheoMonHoc { get; set; } = new();
    public List<MucThongKe> HocVienTheoLop { get; set; } = new();
    public List<MucThongKe> DangKyTheoTrangThai { get; set; } = new();
    public MucThongKe? MonHocNhieuDangKyNhat { get; set; }
    public MucThongKe? GiaSuNhieuLopNhat { get; set; }
}
