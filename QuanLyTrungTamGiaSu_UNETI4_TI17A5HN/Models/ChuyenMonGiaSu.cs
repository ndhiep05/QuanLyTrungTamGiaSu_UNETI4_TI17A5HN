
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models
{
    public class ChuyenMonGiaSu
    {
        [Key]
        public int ChuyenMonGiaSuId { get; set; }

        [Required]
        public int GiaSuId { get; set; }

        // Khóa ngoại đến môn học
        [Required]
        public int MonHocId { get; set; }

        // Trạng thái chuyên môn
        [StringLength(20)]
        public string TrangThai { get; set; } = "Hoạt động";

        // Quan hệ với gia sư
        [ForeignKey(nameof(GiaSuId))]
        public GiaSu? GiaSu { get; set; }

        // Quan hệ với môn học
        [ForeignKey(nameof(MonHocId))]
        public MonHoc? MonHoc { get; set; }
    }
}