// Họ và tên: Nguyễn Đình Hiệp
// Mã sinh viên: 23103100230
// Nội dung thực hiện: Controller Dashboard và Thống kê LINQ (MODULE 5 - SV1).

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Data;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.ViewModels;

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Controllers;

public class ThongKeController : Controller
{
    private readonly AppDbContext _context;

    public ThongKeController(AppDbContext context)
    {
        _context = context;
    }

    // GET: ThongKe/Dashboard
    public async Task<IActionResult> Dashboard(DateTime? tuNgay, DateTime? denNgay)
    {
        var vaiTro = HttpContext.Session.GetString("VaiTro");
        if (vaiTro != VaiTro.Admin)
        {
            return RedirectToAction("KhongCoQuyen", "Home");
        }

        var model = new DashboardViewModel
        {
            TuNgay = tuNgay,
            DenNgay = denNgay
        };

        // 1. Tổng số môn học
        model.TongMonHoc = await _context.MonHoc.CountAsync();

        // 2. Tổng số gia sư đang hoạt động
        model.GiaSuHoatDong = await _context.GiaSu.CountAsync(g => g.TrangThai == TrangThaiChung.HoatDong);

        // 3. Tổng số học viên
        model.TongHocVien = await _context.HocVien.CountAsync();

        // 4. Số lớp đang mở đăng ký
        model.LopDangMoDangKy = await _context.LopHoc.CountAsync(l => l.TrangThai == TrangThaiLop.DangMoDangKy);

        // 5. Số lớp đang học
        model.LopDangHoc = await _context.LopHoc.CountAsync(l => l.TrangThai == TrangThaiLop.DangHoc);

        // 6. Số đăng ký Chờ xác nhận
        model.DangKyChoXacNhan = await _context.DangKyLop.CountAsync(d => d.TrangThai == TrangThaiDangKy.ChoXacNhan);

        // 7. Số lớp đã đủ sĩ số
        model.LopDaDuSiSo = await _context.LopHoc.CountAsync(l =>
            l.TrangThai == TrangThaiLop.DaDu ||
            l.DangKyLops.Count(d => d.TrangThai != TrangThaiDangKy.DaHuy) >= l.SiSoToiDa);

        // 8. Số buổi học sắp tới
        var homNay = DateTime.Today;
        model.BuoiHocSapToi = await _context.LichHoc.CountAsync(l =>
            l.ThoiGianBatDau >= homNay && l.TrangThai == TrangThaiLich.DaLenLich);

        // 9. Tổng học phí đã thu trong kỳ (có lọc khoảng thời gian tuNgay - denNgay nếu được chọn)
        var queryThanhToan = _context.ThanhToanHocPhi.AsQueryable();
        if (tuNgay.HasValue)
        {
            queryThanhToan = queryThanhToan.Where(t => t.NgayThanhToan >= tuNgay.Value.Date);
        }
        if (denNgay.HasValue)
        {
            var denEnd = denNgay.Value.Date.AddDays(1).AddTicks(-1);
            queryThanhToan = queryThanhToan.Where(t => t.NgayThanhToan <= denEnd);
        }
        model.TongHocPhiDaThu = await queryThanhToan.SumAsync(t => (decimal?)t.SoTien) ?? 0m;

        // 10. Số đăng ký còn thiếu học phí
        var dangKyActive = await _context.DangKyLop
            .Include(d => d.LopHoc)
            .Include(d => d.ThanhToans)
            .Where(d => d.TrangThai != TrangThaiDangKy.DaHuy)
            .ToListAsync();

        model.DangKyConThieuHocPhi = dangKyActive.Count(d =>
            d.LopHoc != null && d.ThanhToans.Sum(t => t.SoTien) < d.LopHoc.HocPhi);

        return View(model);
    }

    // GET: ThongKe/Index
    public async Task<IActionResult> Index()
    {
        var vaiTro = HttpContext.Session.GetString("VaiTro");
        if (vaiTro != VaiTro.Admin)
        {
            return RedirectToAction("KhongCoQuyen", "Home");
        }

        var model = new ThongKeViewModel();

        // 1. Số lớp theo môn học (Count(), GroupBy(), OrderByDescending())
        model.LopTheoMonHoc = await _context.LopHoc
            .GroupBy(l => l.MonHoc != null ? l.MonHoc.TenMonHoc : "Chưa xếp môn")
            .Select(g => new MucThongKe
            {
                Ten = g.Key,
                SoLuong = g.Count()
            })
            .OrderByDescending(x => x.SoLuong)
            .ToListAsync();

        // 2. Số học viên theo từng lớp (GroupBy(), Distinct(), Count())
        model.HocVienTheoLop = await _context.DangKyLop
            .Where(d => d.TrangThai != TrangThaiDangKy.DaHuy)
            .GroupBy(d => d.LopHoc != null ? d.LopHoc.TenLop : "Không xác định")
            .Select(g => new MucThongKe
            {
                Ten = g.Key,
                SoLuong = g.Select(x => x.MaHocVien).Distinct().Count()
            })
            .OrderByDescending(x => x.SoLuong)
            .ToListAsync();

        // 3. Số đăng ký theo trạng thái (GroupBy(), Count())
        model.DangKyTheoTrangThai = await _context.DangKyLop
            .GroupBy(d => d.TrangThai)
            .Select(g => new MucThongKe
            {
                Ten = g.Key,
                SoLuong = g.Count()
            })
            .OrderByDescending(x => x.SoLuong)
            .ToListAsync();

        // 4. Môn học có nhiều lượt đăng ký nhất (GroupBy(), OrderByDescending(), FirstOrDefault())
        model.MonHocNhieuDangKyNhat = await _context.DangKyLop
            .Where(d => d.TrangThai != TrangThaiDangKy.DaHuy && d.LopHoc != null && d.LopHoc.MonHoc != null)
            .GroupBy(d => d.LopHoc!.MonHoc!.TenMonHoc)
            .OrderByDescending(g => g.Count())
            .Select(g => new MucThongKe
            {
                Ten = g.Key,
                SoLuong = g.Count()
            })
            .FirstOrDefaultAsync();

        // 5. Gia sư đang phụ trách nhiều lớp nhất (GroupBy(), OrderByDescending(), FirstOrDefault())
        model.GiaSuNhieuLopNhat = await _context.PhanCongGiaSu
            .Where(p => p.GiaSu != null && p.LopHoc != null)
            .GroupBy(p => p.GiaSu!.HoTen)
            .OrderByDescending(g => g.Select(x => x.MaLop).Distinct().Count())
            .Select(g => new MucThongKe
            {
                Ten = g.Key,
                SoLuong = g.Select(x => x.MaLop).Distinct().Count()
            })
            .FirstOrDefaultAsync();

        // 6. Số buổi dạy theo gia sư/tháng (GroupBy(), Count(), OrderBy())
        var buoiDayList = await _context.LichHoc
            .Include(l => l.GiaSu)
            .Where(l => l.GiaSu != null)
            .ToListAsync();

        model.SoBuoiDayTheoGiaSuThang = buoiDayList
            .GroupBy(l => new { GiaSu = l.GiaSu!.HoTen, ThangNam = l.ThoiGianBatDau.ToString("MM/yyyy") })
            .Select(g => new ThongKeBuoiDayGiaSu
            {
                TenGiaSu = g.Key.GiaSu,
                ThangNam = g.Key.ThangNam,
                SoBuoiDay = g.Count()
            })
            .OrderByDescending(x => x.ThangNam)
            .ThenBy(x => x.TenGiaSu)
            .ToList();

        // 7. Doanh thu học phí theo tháng (GroupBy(), Sum(), OrderBy())
        var thanhToanList = await _context.ThanhToanHocPhi.ToListAsync();
        model.DoanhThuTheoThang = thanhToanList
            .GroupBy(t => t.NgayThanhToan.ToString("MM/yyyy"))
            .Select(g => new ThongKeDoanhThuThang
            {
                ThangNam = g.Key,
                DoanhThu = g.Sum(x => x.SoTien)
            })
            .OrderByDescending(x => x.ThangNam)
            .ToList();

        // 8. Tổng số tiền còn phải thu theo lớp (GroupBy(), Sum(), OrderByDescending())
        var lopHocList = await _context.LopHoc
            .Include(l => l.DangKyLops)
                .ThenInclude(d => d.ThanhToans)
            .ToListAsync();

        model.TienConPhaiThuTheoLop = lopHocList.Select(l =>
        {
            var dangKys = l.DangKyLops.Where(d => d.TrangThai != TrangThaiDangKy.DaHuy).ToList();
            decimal hocPhiLop = l.HocPhi;
            decimal tongPhaiThu = hocPhiLop * dangKys.Count;
            decimal tongDaThu = dangKys.Sum(d => d.ThanhToans.Sum(t => t.SoTien));
            decimal tongConPhaiThu = Math.Max(0, tongPhaiThu - tongDaThu);

            return new ThongKeTienConPhaiThuLop
            {
                TenLop = l.TenLop,
                HocPhiLop = hocPhiLop,
                SoDangKy = dangKys.Count,
                TongPhaiThu = tongPhaiThu,
                TongDaThu = tongDaThu,
                TongConPhaiThu = tongConPhaiThu
            };
        })
        .Where(x => x.SoDangKy > 0)
        .OrderByDescending(x => x.TongConPhaiThu)
        .ToList();

        // 9. Tỷ lệ lấp đầy sĩ số từng lớp (Count(), Select(), OrderByDescending())
        model.TyLeLapDaySiSoLop = await _context.LopHoc
            .Select(l => new ThongKeLapDaySiSo
            {
                TenLop = l.TenLop,
                SiSoHienTai = l.DangKyLops.Count(d => d.TrangThai != TrangThaiDangKy.DaHuy),
                SiSoToiDa = l.SiSoToiDa,
                TyLeLapDay = l.SiSoToiDa > 0
                    ? Math.Round((double)l.DangKyLops.Count(d => d.TrangThai != TrangThaiDangKy.DaHuy) / l.SiSoToiDa * 100.0, 1)
                    : 0.0
            })
            .OrderByDescending(x => x.TyLeLapDay)
            .ToListAsync();

        // 10. Tỷ lệ chuyên cần trung bình theo lớp (9.4 & 9.6: GroupBy(), Average(), Count())
        // Công thức: TyLeChuyenCan = SoBuoiCoMat / TongSoBuoiDaDiemDanh * 100%
        var diemDanhList = await _context.DiemDanh
            .Include(d => d.LichHoc)
                .ThenInclude(l => l!.LopHoc)
            .Where(d => d.LichHoc != null && d.LichHoc.LopHoc != null)
            .ToListAsync();

        model.TyLeChuyenCanTrungBinhLop = diemDanhList
            .GroupBy(d => d.LichHoc!.LopHoc!.TenLop)
            .Select(g =>
            {
                int tongBuoi = g.Count();
                int coMat = g.Count(x => x.TrangThaiDiemDanh == TrangThaiDiemDanh.CoMat);
                int vangPhep = g.Count(x => x.TrangThaiDiemDanh == TrangThaiDiemDanh.VangCoPhep);
                int vangKhongPhep = g.Count(x => x.TrangThaiDiemDanh == TrangThaiDiemDanh.VangKhongPhep);
                double tyLe = tongBuoi > 0 ? (double)coMat / tongBuoi * 100.0 : 0.0;

                return new ThongKeChuyenCanLop
                {
                    TenLop = g.Key,
                    TongSoBuoiDiemDanh = tongBuoi,
                    SoBuoiCoMat = coMat,
                    SoBuoiVangCoPhep = vangPhep,
                    SoBuoiVangKhongPhep = vangKhongPhep,
                    TyLeChuyenCanTrungBinh = Math.Round(tyLe, 1)
                };
            })
            .OrderByDescending(x => x.TyLeChuyenCanTrungBinh)
            .ToList();

        return View(model);
    }
}
