// Họ và tên: <Điền họ và tên>
// Mã sinh viên: 23103100230
// Nội dung thực hiện: Thiết kế Entity dùng chung cho cả nhóm (khung CSDL).

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models;

/// <summary>Các giá trị trạng thái/vai trò dùng thống nhất toàn nhóm, tránh gõ chuỗi sai chính tả.</summary>
public static class VaiTro
{
    public const string Admin = "Admin";
    public const string GiaSu = "GiaSu";
    public const string HocVien = "HocVien";
}

public static class TrangThaiChung
{
    public const string HoatDong = "Hoạt động";
    public const string Khoa = "Khóa";          // dùng cho tài khoản, gia sư, học viên, môn học, chuyên môn
}

public static class TrangThaiLop
{
    public const string ChuaMoDangKy = "Chưa mở đăng ký";
    public const string DangMoDangKy = "Đang mở đăng ký";
    public const string DaDu = "Đã đủ";
    public const string DangHoc = "Đang học";
    public const string DaKetThuc = "Đã kết thúc";
    public const string DaHuy = "Đã hủy";
}

public static class TrangThaiDangKy
{
    public const string ChoXacNhan = "Chờ xác nhận";
    public const string DaXacNhan = "Đã xác nhận";
    public const string DaHuy = "Đã hủy";
    public const string HoanThanh = "Hoàn thành";
}

public static class TrangThaiLich
{
    public const string DaLenLich = "Đã lên lịch";
    public const string DaHoanThanh = "Đã hoàn thành";
    public const string DaHuy = "Đã hủy";
}

public static class TrangThaiDiemDanh
{
    public const string CoMat = "Có mặt";
    public const string VangCoPhep = "Vắng có phép";
    public const string VangKhongPhep = "Vắng không phép";
}
