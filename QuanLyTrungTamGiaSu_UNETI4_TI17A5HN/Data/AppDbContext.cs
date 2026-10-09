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

        // ===== Khai báo rõ các khóa ngoại (EF không tự nhận ra vì tên cột không theo quy ước) =====
        modelBuilder.Entity<LopHoc>()
            .HasOne(l => l.MonHoc).WithMany(m => m.LopHocs).HasForeignKey(l => l.MaMonHoc);

        modelBuilder.Entity<ChuyenMonGiaSu>(e =>
        {
            e.HasOne(c => c.GiaSu).WithMany(g => g.ChuyenMons).HasForeignKey(c => c.MaGiaSu);
            e.HasOne(c => c.MonHoc).WithMany(m => m.ChuyenMons).HasForeignKey(c => c.MaMonHoc);
        });

        modelBuilder.Entity<DangKyLop>(e =>
        {
            e.HasOne(d => d.HocVien).WithMany(h => h.DangKyLops).HasForeignKey(d => d.MaHocVien);
            e.HasOne(d => d.LopHoc).WithMany(l => l.DangKyLops).HasForeignKey(d => d.MaLop);
        });

        modelBuilder.Entity<PhanCongGiaSu>(e =>
        {
            e.HasOne(p => p.LopHoc).WithMany(l => l.PhanCongs).HasForeignKey(p => p.MaLop);
            e.HasOne(p => p.GiaSu).WithMany(g => g.PhanCongs).HasForeignKey(p => p.MaGiaSu);
        });

        modelBuilder.Entity<LichHoc>(e =>
        {
            e.HasOne(l => l.LopHoc).WithMany(lop => lop.LichHocs).HasForeignKey(l => l.MaLop);
            e.HasOne(l => l.GiaSu).WithMany(g => g.LichHocs).HasForeignKey(l => l.MaGiaSu);
        });

        modelBuilder.Entity<DiemDanh>(e =>
        {
            e.HasOne(d => d.LichHoc).WithMany(l => l.DiemDanhs).HasForeignKey(d => d.MaLichHoc);
            e.HasOne(d => d.HocVien).WithMany(h => h.DiemDanhs).HasForeignKey(d => d.MaHocVien);
        });

        modelBuilder.Entity<ThanhToanHocPhi>()
            .HasOne(t => t.DangKyLop).WithMany(d => d.ThanhToans).HasForeignKey(t => t.MaDangKy);

        // Tránh lỗi "multiple cascade paths" của SQL Server và bảo toàn dữ liệu:
        // không tự động xóa dây chuyền, phải xóa/kiểm tra thủ công trong Controller.
        foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            fk.DeleteBehavior = DeleteBehavior.Restrict;
    }
}
