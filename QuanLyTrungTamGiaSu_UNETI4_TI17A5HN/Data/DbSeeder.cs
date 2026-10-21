// Họ và tên: Nguyễn Trọng Chuyển
// Mã sinh viên: 23103100204
// Nội dung thực hiện: Seed Data tự động 30 Tài khoản và 30 Học viên mẫu

using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models;

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Data
{
    public static class DbSeeder
    {
        public static void SeedData(AppDbContext context)
        {
            // Kiểm tra nếu chưa có học viên thì mới sinh dữ liệu
            if (!context.HocVien.Any())
            {
                var hoTenList = new string[]
                {
                    "Nguyễn Văn An", "Trần Thị Bích", "Lê Hoàng Cường", "Phạm Minh Đức", "Vũ Thị Hoa",
                    "Đỗ Kim Ngân", "Hoàng Văn Nam", "Bùi Thị Mai", "Đặng Tiến Dũng", "Phan Thanh Hà",
                    "Ngô Quốc Bảo", "Dương Thu Trang", "Lý Văn Hùng", "Trịnh Thảo Nhi", "Đinh Xuân Lộc",
                    "Đoàn Phương Anh", "Mai Văn Phúc", "Trương Khánh Linh", "Lê Tấn Phát", "Nguyễn Mỹ Duyên",
                    "Trần Bảo Long", "Vũ Hoàng Yến", "Phạm Đức Anh", "Hoàng Thu Thảo", "Đỗ Quỳnh Như",
                    "Bùi Hoàng Nam", "Đặng Ngọc Ánh", "Phan Văn Thịnh", "Ngô Tấn Tài", "Trịnh Ngọc Diệp"
                };

                for (int i = 0; i < 30; i++)
                {
                    string tenDangNhap = $"hocvien{i + 1}";

                    // 1. Tạo tài khoản tương ứng
                    var taiKhoan = context.TaiKhoan.FirstOrDefault(t => t.TenDangNhap == tenDangNhap);
                    if (taiKhoan == null)
                    {
                        taiKhoan = new TaiKhoan
                        {
                            TenDangNhap = tenDangNhap,
                            MatKhau = "123456",
                            HoTen = hoTenList[i],
                            Email = $"hocvien{i + 1}@gmail.com",
                            VaiTro = "HocVien",
                            TrangThai = "HoatDong"
                        };
                        context.TaiKhoan.Add(taiKhoan);
                        context.SaveChanges();
                    }

                    // 2. Tạo hồ sơ Học viên liên kết với MaTaiKhoan
                    var hocVien = new HocVien
                    {
                        MaTaiKhoan = taiKhoan.MaTaiKhoan,
                        HoTen = hoTenList[i],
                        NgaySinh = new DateTime(2005, 1, 1).AddDays(i * 15),
                        GioiTinh = (i % 2 == 0) ? "Nam" : "Nữ",
                        SoDienThoai = $"0981000{i + 1:D3}",
                        Email = $"hocvien{i + 1}@gmail.com",
                        DiaChi = $"Số {i + 1} Đường Hoàng Mai, Hà Nội",
                        TrinhDoHienTai = (i % 3 == 0) ? "Lớp 10" : ((i % 3 == 1) ? "Lớp 11" : "Lớp 12"),
                        TrangThai = "HoatDong"
                    };

                    context.HocVien.Add(hocVien);
                }

                context.SaveChanges();
            }
        }
    }
}