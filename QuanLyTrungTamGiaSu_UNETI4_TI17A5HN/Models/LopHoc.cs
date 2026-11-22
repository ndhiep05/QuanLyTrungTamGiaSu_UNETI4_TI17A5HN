// Họ và tên: Nguyễn Trọng Chuyển | Mã sinh viên: 23103100204 | Nội dung thực hiện: Model Lớp học
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models
{
    [Table("LopHoc")]
    public class LopHoc
    {
        [Key]
        [Column("MaLop")]
        public int MaLop { get; set; }

        [Required(ErrorMessage = "Tên lớp không được để trống")]
        [StringLength(100)]
        [Column("TenLop")]
        [Display(Name = "Tên lớp")]
        public string TenLop { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn môn học")]
        [Column("MaMonHoc")]
        [Display(Name = "Môn học")]
        public int MaMonHoc { get; set; }

        [ForeignKey("MaMonHoc")]
        public virtual MonHoc? MonHoc { get; set; }

        [Column("TrinhDoLop")]
        [Display(Name = "Trình độ lớp")]
        public string? TrinhDoLop { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc")]
        [DataType(DataType.Date)]
        [Column("NgayBatDau")]
        [Display(Name = "Ngày bắt đầu")]
        public DateTime NgayBatDau { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc là bắt buộc")]
        [DataType(DataType.Date)]
        [Column("NgayKetThuc")]
        [Display(Name = "Ngày kết thúc")]
        public DateTime NgayKetThuc { get; set; }

        [Required(ErrorMessage = "Sĩ số tối đa phải > 0")]
        [Range(1, 500, ErrorMessage = "Sĩ số tối đa từ 1 đến 500")]
        [Column("SiSoToiDa")]
        [Display(Name = "Sĩ số tối đa")]
        public int SiSoToiDa { get; set; }

        [Required(ErrorMessage = "Học phí >= 0")]
        [Range(0, double.MaxValue, ErrorMessage = "Học phí phải lớn hơn hoặc bằng 0")]
        [Column("HocPhi")]
        [Display(Name = "Học phí")]
        public decimal HocPhi { get; set; }

        [Column("TrangThai")]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "Đang mở đăng ký";

        [Column("CapDo")]
        [Display(Name = "Cấp độ")]
        public string? CapDo { get; set; }

        [Column("MoTa")]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [Column("GhiChu")]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        // --- NAVIGATION PROPERTIES ---
        public virtual ICollection<DangKyLop> DangKyLops { get; set; } = new List<DangKyLop>();
        public virtual ICollection<PhanCongGiaSu> PhanCongs { get; set; } = new List<PhanCongGiaSu>();
        [NotMapped]
        public virtual ICollection<PhanCongGiaSu> PhanCongGiaSus { get; set; } = new List<PhanCongGiaSu>();
        public virtual ICollection<LichHoc> LichHocs { get; set; } = new List<LichHoc>();
    }
}