
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models
{
    public class GiaSu
    {
        [Key]
        public int GiaSuId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên gia sư")]
        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [StringLength(15)]
        public string SoDienThoai { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(200)]
        public string? DiaChi { get; set; }

        [StringLength(100)]
        public string? TrinhDo { get; set; }

        [Range(0, 100, ErrorMessage = "Số năm kinh nghiệm không hợp lệ")]
        public int KinhNghiem { get; set; }

        [StringLength(20)]
        public string TrangThai { get; set; } = "Hoạt động";

        // Danh sách chuyên môn của gia sư
        public ICollection<ChuyenMonGiaSu> ChuyenMons { get; set; }
            = new List<ChuyenMonGiaSu>();
    }
}