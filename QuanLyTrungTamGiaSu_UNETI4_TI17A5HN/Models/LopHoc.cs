// Họ và tên: Nguyễn Trọng Chuyển
// Mã sinh viên: 23103100204
// Nội dung thực hiện: Module 3 - Model Quản lý Lớp học

using System.ComponentModel.DataAnnotations;

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models
{
    public class LopHoc
    {
        [Key]
        public int MaLop { get; set; }

        [Required(ErrorMessage = "Tên lớp học không được để trống")]
        [StringLength(100, ErrorMessage = "Tên lớp không quá 100 ký tự")]
        [Display(Name = "Tên lớp học")]
        public string TenLop { get; set; } = string.Empty;

        [Required(ErrorMessage = "Môn học là bắt buộc")]
        [Display(Name = "Môn học")]
        public int MaMonHoc { get; set; }

        [Required(ErrorMessage = "Sĩ số tối đa là bắt buộc")]
        [Range(1, 500, ErrorMessage = "Sĩ số tối đa phải từ 1 đến 500")]
        [Display(Name = "Sĩ số tối đa")]
        public int SiSoToiDa { get; set; } = 20;

        [Required(ErrorMessage = "Học phí là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Học phí không hợp lệ")]
        [Display(Name = "Học phí")]
        public decimal HocPhi { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "HoatDong";

        // Navigation properties
        public MonHoc? MonHoc { get; set; }
        public ICollection<DangKyLop> DangKyLops { get; set; } = new List<DangKyLop>();
        public ICollection<PhanCongGiaSu> PhanCongGiaSus { get; set; } = new List<PhanCongGiaSu>();
        public ICollection<LichHoc> LichHocs { get; set; } = new List<LichHoc>();
    }
}