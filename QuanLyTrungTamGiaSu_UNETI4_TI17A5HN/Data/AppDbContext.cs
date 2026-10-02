// Họ và tên: <Điền họ và tên>
// Mã sinh viên: 23103100230
// Nội dung thực hiện: Thiết kế Entity dùng chung cho cả nhóm (khung CSDL).
using Microsoft.EntityFrameworkCore;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models;

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<TaiKhoan> TaiKhoan => Set<TaiKhoan>();
    public DbSet<MonHoc> MonHoc => Set<MonHoc>();
    public DbSet<GiaSu> GiaSu => Set<GiaSu>();
    public DbSet<ChuyenMonGiaSu> ChuyenMonGiaSu => Set<ChuyenMonGiaSu>();
    public DbSet<HocVien> HocVien => Set<HocVien>();
    public DbSet<LopHoc> LopHoc => Set<LopHoc>();
    public DbSet<DangKyLop> DangKyLop => Set<DangKyLop>();
    public DbSet<PhanCongGiaSu> PhanCongGiaSu => Set<PhanCongGiaSu>();
    public DbSet<LichHoc> LichHoc => Set<LichHoc>();
    public DbSet<DiemDanh> DiemDanh => Set<DiemDanh>();
    public DbSet<ThanhToanHocPhi> ThanhToanHocPhi => Set<ThanhToanHocPhi>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Tên đăng nhập và tên môn học không được trùng
        modelBuilder.Entity<TaiKhoan>().HasIndex(t => t.TenDangNhap).IsUnique();
        modelBuilder.Entity<MonHoc>().HasIndex(m => m.TenMonHoc).IsUnique();

        // TaiKhoan (1) - (0..1) GiaSu / HocVien
        modelBuilder.Entity<GiaSu>()
            .HasOne(g => g.TaiKhoan).WithOne(t => t.GiaSu)
            .HasForeignKey<GiaSu>(g => g.MaTaiKhoan);
        modelBuilder.Entity<HocVien>()
            .HasOne(h => h.TaiKhoan).WithOne(t => t.HocVien)
            .HasForeignKey<HocVien>(h => h.MaTaiKhoan);

        // Mỗi học viên chỉ có một bản ghi điểm danh cho một buổi học
        modelBuilder.Entity<DiemDanh>()
            .HasIndex(d => new { d.MaLichHoc, d.MaHocVien }).IsUnique();

        // Kiểu tiền tệ
        modelBuilder.Entity<LopHoc>().Property(l => l.HocPhi).HasPrecision(18, 2);
        modelBuilder.Entity<ThanhToanHocPhi>().Property(t => t.SoTien).HasPrecision(18, 2);

        // Tránh lỗi "multiple cascade paths" của SQL Server và bảo toàn dữ liệu:
        // không tự động xóa dây chuyền, phải xóa/kiểm tra thủ công trong Controller.
        foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            fk.DeleteBehavior = DeleteBehavior.Restrict;
    }
}
