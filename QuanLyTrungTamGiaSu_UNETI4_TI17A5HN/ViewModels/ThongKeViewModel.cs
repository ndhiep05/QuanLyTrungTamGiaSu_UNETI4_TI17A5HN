// Họ và tên: Nguyễn Đình Hiệp
// Mã sinh viên: 23103100230
// Nội dung thực hiện: ViewModel dùng chung cho giao diện Thống kê LINQ (SV1).

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.ViewModels;

public class MucThongKe
{
    public string Ten { get; set; } = string.Empty;
    public int SoLuong { get; set; }
    public decimal SoTien { get; set; }
    public double TyLe { get; set; }
    public string GhiChu { get; set; } = string.Empty;
}

public class ThongKeBuoiDayGiaSu
{
    public string TenGiaSu { get; set; } = string.Empty;
    public string ThangNam { get; set; } = string.Empty;
    public int SoBuoiDay { get; set; }
}

public class ThongKeDoanhThuThang
{
    public string ThangNam { get; set; } = string.Empty;
    public decimal DoanhThu { get; set; }
}

public class ThongKeTienConPhaiThuLop
{
    public string TenLop { get; set; } = string.Empty;
    public decimal HocPhiLop { get; set; }
    public int SoDangKy { get; set; }
    public decimal TongPhaiThu { get; set; }
    public decimal TongDaThu { get; set; }
    public decimal TongConPhaiThu { get; set; }
}

public class ThongKeLapDaySiSo
{
    public string TenLop { get; set; } = string.Empty;
    public int SiSoHienTai { get; set; }
    public int SiSoToiDa { get; set; }
    public double TyLeLapDay { get; set; }
}

public class ThongKeChuyenCanLop
{
    public string TenLop { get; set; } = string.Empty;
    public int TongSoBuoiDiemDanh { get; set; }
    public int SoBuoiCoMat { get; set; }
    public int SoBuoiVangCoPhep { get; set; }
    public int SoBuoiVangKhongPhep { get; set; }
    public double TyLeChuyenCanTrungBinh { get; set; }
}

/// <summary>Thống kê bằng LINQ theo mục 9.6 của đề bài.</summary>
public class ThongKeViewModel
{
    // 1. Số lớp theo môn học
    public List<MucThongKe> LopTheoMonHoc { get; set; } = new();

    // 2. Số học viên theo từng lớp
    public List<MucThongKe> HocVienTheoLop { get; set; } = new();

    // 3. Số đăng ký theo trạng thái
    public List<MucThongKe> DangKyTheoTrangThai { get; set; } = new();

    // 4. Môn học có nhiều lượt đăng ký nhất
    public MucThongKe? MonHocNhieuDangKyNhat { get; set; }

    // 5. Gia sư đang phụ trách nhiều lớp nhất
    public MucThongKe? GiaSuNhieuLopNhat { get; set; }

    // 6. Số buổi dạy theo gia sư/tháng
    public List<ThongKeBuoiDayGiaSu> SoBuoiDayTheoGiaSuThang { get; set; } = new();

    // 7. Doanh thu học phí theo tháng
    public List<ThongKeDoanhThuThang> DoanhThuTheoThang { get; set; } = new();

    // 8. Tổng số tiền còn phải thu theo lớp
    public List<ThongKeTienConPhaiThuLop> TienConPhaiThuTheoLop { get; set; } = new();

    // 9. Tỷ lệ lấp đầy sĩ số từng lớp
    public List<ThongKeLapDaySiSo> TyLeLapDaySiSoLop { get; set; } = new();

    // 10. Tỷ lệ chuyên cần trung bình theo lớp
    public List<ThongKeChuyenCanLop> TyLeChuyenCanTrungBinhLop { get; set; } = new();
}
