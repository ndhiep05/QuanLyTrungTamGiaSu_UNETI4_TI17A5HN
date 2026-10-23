// Họ và tên: Nguyễn Trọng Chuyển
// Mã sinh viên: 23103100204
// Nội dung thực hiện: Module 3 - Model Đăng ký Lớp học & Kiểm soát Sĩ số

using System.ComponentModel.DataAnnotations;

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models
{
    public class DangKyLop
    {
        [Key]
        public int MaDangKy { get; set; }

        [Required]
        public int MaHocVien { get; set; }

        [Required]
        public int MaLop { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Ngày đăng ký")]
        public DateTime NgayDangKy { get; set; } = DateTime.Now;

        [Required]
        [StringLength(20)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = TrangThaiDangKy.ChoXacNhan;

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        // Navigation properties
        public HocVien? HocVien { get; set; }
        public LopHoc? LopHoc { get; set; }
        public ICollection<ThanhToanHocPhi> ThanhToans { get; set; } = new List<ThanhToanHocPhi>();
    }
}