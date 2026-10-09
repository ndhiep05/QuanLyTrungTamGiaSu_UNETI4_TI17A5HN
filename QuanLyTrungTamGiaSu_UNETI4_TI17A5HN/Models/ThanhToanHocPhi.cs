// Họ và tên: <Điền họ và tên>
// Mã sinh viên: 23103100230
// Nội dung thực hiện: Thiết kế Entity dùng chung cho cả nhóm (khung CSDL).
using System.ComponentModel.DataAnnotations;

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models;

public class ThanhToanHocPhi
{
    [Key]
    public int MaThanhToan { get; set; }

    [Required]
    [Display(Name = "Đăng ký")]
    public int MaDangKy { get; set; }

    [DataType(DataType.DateTime)]
    [Display(Name = "Ngày thanh toán")]
    public DateTime NgayThanhToan { get; set; }

    [Range(1, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn 0")]
    [DataType(DataType.Currency)]
    [Display(Name = "Số tiền")]
    public decimal SoTien { get; set; }   // kiểm tra không vượt số tiền còn lại trong Controller

    [Required]
    [StringLength(30)]
    [Display(Name = "Phương thức")]
    public string PhuongThuc { get; set; } = "Tiền mặt";

    [Display(Name = "Người ghi nhận")]
    public int? NguoiGhiNhan { get; set; }   // MaTaiKhoan của người ghi nhận (không đặt khóa ngoại để đơn giản)

    [StringLength(500)]
    [Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }

    public DangKyLop? DangKyLop { get; set; }
}
