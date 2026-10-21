// Họ và tên: Nguyễn Trọng Chuyển
// Mã sinh viên: 23103100204
// Nội dung thực hiện: Cấu hình hệ thống Program.cs (DbContext, Session, StaticFiles, Retry Policy)

using Microsoft.EntityFrameworkCore;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Data;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models;

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

var app = builder.Build();

// --- KHỞI TẠO TÀI KHOẢN ADMIN MẶC ĐỊNH VÀ TỰ ĐỘNG ĐỒNG BỘ CSDL ---
try
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Tạo CSDL nếu chưa tồn tại
        context.Database.EnsureCreated();

        // Tự động kiểm tra và thêm các cột còn thiếu vào bảng LopHoc để đồng bộ tuyệt đối với Model
        context.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[LopHoc]') AND name = 'CapDo')
                ALTER TABLE [dbo].[LopHoc] ADD [CapDo] NVARCHAR(100) NULL;

            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[LopHoc]') AND name = 'GhiChu')
                ALTER TABLE [dbo].[LopHoc] ADD [GhiChu] NVARCHAR(MAX) NULL;

            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[LopHoc]') AND name = 'TrangThai')
                ALTER TABLE [dbo].[LopHoc] ADD [TrangThai] NVARCHAR(50) NOT NULL DEFAULT (N'Đang mở đăng ký');
        ");

        // Tự động khởi tạo user admin nếu chưa có
        if (!context.TaiKhoan.Any(t => t.TenDangNhap == "admin"))
        {
            context.TaiKhoan.Add(new TaiKhoan
            {
                TenDangNhap = "admin",
                MatKhau = "123456",
                HoTen = "Quản trị viên",
                VaiTro = "Admin"
            });
            context.SaveChanges();
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine("Lỗi khởi tạo CSDL: " + ex.Message);
}

// 4. Configure the HTTP request pipeline
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