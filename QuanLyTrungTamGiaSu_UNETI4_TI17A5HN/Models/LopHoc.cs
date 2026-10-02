// Họ và tên: <Điền họ và tên>
// Mã sinh viên: 23103100230
// Nội dung thực hiện: Thiết kế Entity dùng chung cho cả nhóm (khung CSDL).
using System.ComponentModel.DataAnnotations;

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models;

public class LopHoc
{
    [Key]
    public int MaLop { get; set; }

    [Required(ErrorMessage = "Tên lớp là bắt buộc")]
    [StringLength(100)]
    [Display(Name = "Tên lớp")]
    public string TenLop { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn môn học")]
    [Display(Name = "Môn học")]
    public int MaMonHoc { get; set; }

    [Required]
    [StringLength(30)]
    [Display(Name = "Trình độ lớp")]
    public string TrinhDoLop { get; set; } = "Cơ bản";

    [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc")]
    [DataType(DataType.Date)]
    [Display(Name = "Ngày bắt đầu")]
    public DateTime NgayBatDau { get; set; }

    [Required(ErrorMessage = "Ngày kết thúc là bắt buộc")]
    [DataType(DataType.Date)]
    [Display(Name = "Ngày kết thúc")]
    public DateTime NgayKetThuc { get; set; }   // kiểm tra >= NgayBatDau trong Controller

    [Range(1, 1000, ErrorMessage = "Sĩ số tối đa phải lớn hơn 0")]
    [Display(Name = "Sĩ số tối đa")]
    public int SiSoToiDa { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Học phí phải từ 0 trở lên")]
    [DataType(DataType.Currency)]
    [Display(Name = "Học phí")]
    public decimal HocPhi { get; set; }

    [Required]
    [StringLength(30)]
    [Display(Name = "Trạng thái")]
    public string TrangThai { get; set; } = TrangThaiLop.ChuaMoDangKy;

    [StringLength(1000)]
    [Display(Name = "Mô tả")]
    public string? MoTa { get; set; }

    public MonHoc? MonHoc { get; set; }
    public ICollection<DangKyLop> DangKyLops { get; set; } = new List<DangKyLop>();
    public ICollection<PhanCongGiaSu> PhanCongs { get; set; } = new List<PhanCongGiaSu>();
    public ICollection<LichHoc> LichHocs { get; set; } = new List<LichHoc>();
}
