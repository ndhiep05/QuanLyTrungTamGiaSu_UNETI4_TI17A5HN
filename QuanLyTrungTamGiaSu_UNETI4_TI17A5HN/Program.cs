// Họ và tên: Nguyễn Trọng Chuyển
// Mã sinh viên: 23103100204
// Nội dung thực hiện: Cấu hình hệ thống Program.cs (DbContext, Session, StaticFiles, Retry Policy)

using Microsoft.EntityFrameworkCore;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Data;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Đăng ký Controllers với Views
builder.Services.AddControllersWithViews();

// 2. Đăng ký AppDbContext + Cấu hình EnableRetryOnFailure (Chống văng kết nối SQL Server)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));

// 3. Đăng ký Session (Lưu thông tin đăng nhập & vai trò)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 4. Đăng ký dịch vụ xuất Excel
builder.Services.AddScoped<ExcelExportService>();

var app = builder.Build();

// --- KHỞI TẠO TÀI KHOẢN ADMIN MẶC ĐỊNH VÀ TỰ ĐỘNG ĐỒNG BỘ CSDL ---
try
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Tạo CSDL nếu chưa tồn tại
        context.Database.EnsureCreated();

        // Tự động kiểm tra và thêm các cột còn thiếu vào bảng LopHoc, HocVien, TaiKhoan để đồng bộ tuyệt đối với Model
        context.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[LopHoc]') AND name = 'CapDo')
                ALTER TABLE [dbo].[LopHoc] ADD [CapDo] NVARCHAR(100) NULL;

            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[LopHoc]') AND name = 'GhiChu')
                ALTER TABLE [dbo].[LopHoc] ADD [GhiChu] NVARCHAR(MAX) NULL;

            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[LopHoc]') AND name = 'TrangThai')
                ALTER TABLE [dbo].[LopHoc] ADD [TrangThai] NVARCHAR(50) NOT NULL DEFAULT (N'Đang mở đăng ký');

            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HocVien]') AND name = 'TruongHoc')
                ALTER TABLE [dbo].[HocVien] ADD [TruongHoc] NVARCHAR(100) NULL;

            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HocVien]') AND name = 'LopHocHienTai')
                ALTER TABLE [dbo].[HocVien] ADD [LopHocHienTai] NVARCHAR(50) NULL;

            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TaiKhoan]') AND name = 'AnhDaiDien')
                ALTER TABLE [dbo].[TaiKhoan] ADD [AnhDaiDien] NVARCHAR(255) NULL;

            IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ChuyenMonGiaSu]') AND name = 'TrinhDoChuyenMon')
                ALTER TABLE [dbo].[ChuyenMonGiaSu] ALTER COLUMN [TrinhDoChuyenMon] NVARCHAR(200) NULL;

            IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ChuyenMonGiaSu]') AND name = 'SoNamKinhNghiemMon')
                ALTER TABLE [dbo].[ChuyenMonGiaSu] ALTER COLUMN [SoNamKinhNghiemMon] INT NULL;

            IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[LopHoc]') AND name = 'TrinhDoLop')
                ALTER TABLE [dbo].[LopHoc] ALTER COLUMN [TrinhDoLop] NVARCHAR(100) NULL;

            UPDATE [dbo].[LopHoc]
            SET [NgayBatDau] = '2026-11-25', [NgayKetThuc] = '2027-02-25'
            WHERE [NgayBatDau] < '1900-01-01' OR [NgayKetThuc] < '1900-01-01';
        ");

        // Tự động khởi tạo user admin nếu chưa có
        if (!context.TaiKhoan.Any(t => t.TenDangNhap == "admin"))
        {
            context.TaiKhoan.Add(new TaiKhoan
            {
                TenDangNhap = "admin",
                MatKhau = "Admin@123",
                HoTen = "Quản trị viên",
                Email = "admin@uneti.edu.vn",
                VaiTro = "Admin",
                TrangThai = "HoatDong"
            });
            context.SaveChanges();
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine("Lỗi khởi tạo CSDL: " + ex.Message);
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Phục vụ CSS, JS, hình ảnh từ wwwroot
app.UseRouting();

// Middleware Session & Phân quyền
app.UseSession();
app.UseAuthorization();

// Route mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
