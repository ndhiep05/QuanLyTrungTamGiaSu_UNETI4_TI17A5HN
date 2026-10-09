// Họ và tên: Nguyễn Trọng Chuyển
// Mã sinh viên: 23103100204
// Nội dung thực hiện: Module 3 - Controller Quản lý Đăng ký lớp & Kiểm soát sĩ số

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Data;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.ViewModels;

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Controllers
{
    public class DangKyLopController : Controller
    {
        private readonly AppDbContext _context;

        public DangKyLopController(AppDbContext context)
        {
            _context = context;
        }

        // 1. DANH SÁCH ĐĂNG KÝ LỚP (Lọc theo Lớp / Học viên / Trạng thái + Phân trang)
        public async Task<IActionResult> Index(int? maLop, int? maHocVien, string? trangThai, int trang = 1)
        {
            int pageSize = 10;
            var query = _context.DangKyLop
                .Include(d => d.HocVien)
                .Include(d => d.LopHoc)
                .AsQueryable();

            if (maLop.HasValue && maLop > 0)
                query = query.Where(d => d.MaLop == maLop);

            if (maHocVien.HasValue && maHocVien > 0)
                query = query.Where(d => d.MaHocVien == maHocVien);

            if (!string.IsNullOrWhiteSpace(trangThai))
                query = query.Where(d => d.TrangThai == trangThai);

            int totalItems = await query.CountAsync();
            var items = await query
                .OrderByDescending(d => d.MaDangKy)
                .Skip((trang - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.MaLop = new SelectList(await _context.LopHoc.ToListAsync(), "MaLop", "TenLop", maLop);
            ViewBag.MaHocVien = new SelectList(await _context.HocVien.ToListAsync(), "MaHocVien", "HoTen", maHocVien);
            ViewBag.TrangThai = trangThai;

            ViewBag.PhanTrang = new PhanTrangViewModel
            {
                CurrentPage = trang,
                PageSize = pageSize,
                TotalItems = totalItems,
                ActionName = "Index"
            };

            return View(items);
        }

        // 2. THÊM ĐĂNG KÝ MỚI (GET)
        public async Task<IActionResult> Create()
        {
            ViewBag.MaHocVien = new SelectList(await _context.HocVien.ToListAsync(), "MaHocVien", "HoTen");
            ViewBag.MaLop = new SelectList(await _context.LopHoc.ToListAsync(), "MaLop", "TenLop");
            return View();
        }

        // 2. THÊM ĐĂNG KÝ MỚI (POST) - RÀNG BUỘC CHỐNG TRÙNG & KIỂM SOÁT SĨ SỐ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int maHocVien, int maLop, string? ghiChu)
        {
            var lopHoc = await _context.LopHoc.FindAsync(maLop);
            if (lopHoc == null)
            {
                TempData["Error"] = "Lớp học không tồn tại!";
                return RedirectToAction(nameof(Index));
            }

            // RÀNG BUỘC 1: Kiểm tra chống đăng ký trùng lớp
            bool daDangKy = await _context.DangKyLop
                .AnyAsync(d => d.MaHocVien == maHocVien && d.MaLop == maLop && d.TrangThai != "DaHuy");
            if (daDangKy)
            {
                TempData["Error"] = "Học viên này đã đăng ký hoặc đang chờ duyệt lớp học này!";
                return RedirectToAction(nameof(Index));
            }

            // RÀNG BUỘC 2: Kiểm tra sĩ số tối đa của lớp
            int siSoDaDuyet = await _context.DangKyLop
                .CountAsync(d => d.MaLop == maLop && (d.TrangThai == "DaDuyet" || d.TrangThai == "DaXacNhan"));

            if (siSoDaDuyet >= lopHoc.SiSoToiDa)
            {
                TempData["Error"] = $"Không thể đăng ký: Lớp học đã đạt sĩ số tối đa ({lopHoc.SiSoToiDa} học viên)!";
                return RedirectToAction(nameof(Index));
            }

            // Thêm lượt đăng ký
            var dangKy = new DangKyLop
            {
                MaHocVien = maHocVien,
                MaLop = maLop,
                NgayDangKy = DateTime.Now,
                GhiChu = ghiChu,
                TrangThai = TrangThaiDangKy.ChoXacNhan
            };

            _context.DangKyLop.Add(dangKy);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đăng ký lớp học thành công! Vui lòng chờ duyệt.";
            return RedirectToAction(nameof(Index));
        }

        // 3. DUYỆT ĐĂNG KÝ LỚP - RÀNG BUỘC KIỂM SOÁT SĨ SỐ LẦN 2
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DuyetDangKy(int id)
        {
            var dangKy = await _context.DangKyLop
                .Include(d => d.LopHoc)
                .FirstOrDefaultAsync(d => d.MaDangKy == id);

            if (dangKy == null || dangKy.LopHoc == null) return NotFound();

            // Kiểm tra sĩ số thực tế trước khi phê duyệt
            int siSoDaDuyet = await _context.DangKyLop
                .CountAsync(d => d.MaLop == dangKy.MaLop && (d.TrangThai == "DaDuyet" || d.TrangThai == "DaXacNhan"));

            if (siSoDaDuyet >= dangKy.LopHoc.SiSoToiDa)
            {
                TempData["Error"] = $"Duyệt thất bại: Lớp học đã đủ sĩ số tối đa ({dangKy.LopHoc.SiSoToiDa} học viên)!";
                return RedirectToAction(nameof(Index));
            }

            dangKy.TrangThai = "DaXacNhan";
            await _context.SaveChangesAsync();

            TempData["Success"] = "Duyệt đăng ký lớp học thành công!";
            return RedirectToAction(nameof(Index));
        }

        // 4. HỦY ĐĂNG KÝ LỚP
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyDangKy(int id)
        {
            var dangKy = await _context.DangKyLop.FindAsync(id);
            if (dangKy == null) return NotFound();

            dangKy.TrangThai = "DaHuy";
            await _context.SaveChangesAsync();

            TempData["Success"] = "Hủy đăng ký lớp học thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}