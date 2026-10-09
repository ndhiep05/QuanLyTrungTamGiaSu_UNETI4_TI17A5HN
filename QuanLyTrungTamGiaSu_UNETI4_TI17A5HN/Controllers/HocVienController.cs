// Họ và tên: Nguyễn Trọng Chuyển
// Mã sinh viên: 23103100204
// Nội dung thực hiện: Module 3 - Controller Quản lý Học viên

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Data;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.ViewModels;

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Controllers
{
    public class HocVienController : Controller
    {
        private readonly AppDbContext _context;

        public HocVienController(AppDbContext context)
        {
            _context = context;
        }

        // 1. DANH SÁCH HỌC VIÊN (Tìm kiếm + Phân trang)
        public async Task<IActionResult> Index(string? tuKhoa, int trang = 1)
        {
            int pageSize = 10;
            var query = _context.HocVien.AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();
                query = query.Where(h => h.HoTen.Contains(tuKhoa) ||
                                         h.Email.Contains(tuKhoa) ||
                                         h.SoDienThoai.Contains(tuKhoa));
            }

            int totalItems = await query.CountAsync();
            var items = await query
                .OrderByDescending(h => h.MaHocVien)
                .Skip((trang - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.PhanTrang = new PhanTrangViewModel
            {
                CurrentPage = trang,
                PageSize = pageSize,
                TotalItems = totalItems,
                ActionName = "Index"
            };

            return View(items);
        }

        // 2. CHI TIẾT HỌC VIÊN
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var hocVien = await _context.HocVien
                .Include(h => h.DangKyLops)
                    .ThenInclude(d => d.LopHoc)
                .FirstOrDefaultAsync(m => m.MaHocVien == id);

            if (hocVien == null) return NotFound();

            return View(hocVien);
        }

        // 3. THÊM MỚI (GET)
        public IActionResult Create()
        {
            return View();
        }

        // 3. THÊM MỚI (POST) - RÀNG BUỘC TRÙNG SĐT & EMAIL
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("HoTen,NgaySinh,GioiTinh,SoDienThoai,Email,DiaChi,TrinhDoHienTai,TrangThai,MaTaiKhoan")] HocVien hocVien)
        {
            // RÀNG BUỘC: Kiểm tra trùng SĐT
            if (await _context.HocVien.AnyAsync(h => h.SoDienThoai == hocVien.SoDienThoai))
            {
                ModelState.AddModelError("SoDienThoai", "Số điện thoại này đã tồn tại trong hệ thống!");
            }

            // RÀNG BUỘC: Kiểm tra trùng Email
            if (!string.IsNullOrEmpty(hocVien.Email) && await _context.HocVien.AnyAsync(h => h.Email == hocVien.Email))
            {
                ModelState.AddModelError("Email", "Email này đã tồn tại trong hệ thống!");
            }

            if (ModelState.IsValid)
            {
                _context.Add(hocVien);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm mới học viên thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(hocVien);
        }

        // 4. CHỈNH SỬA (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var hocVien = await _context.HocVien.FindAsync(id);
            if (hocVien == null) return NotFound();

            return View(hocVien);
        }

        // 4. CHỈNH SỬA (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaHocVien,HoTen,NgaySinh,GioiTinh,SoDienThoai,Email,DiaChi,TrinhDoHienTai,TrangThai,MaTaiKhoan")] HocVien hocVien)
        {
            if (id != hocVien.MaHocVien) return NotFound();

            if (await _context.HocVien.AnyAsync(h => h.SoDienThoai == hocVien.SoDienThoai && h.MaHocVien != id))
            {
                ModelState.AddModelError("SoDienThoai", "Số điện thoại này thuộc về học viên khác!");
            }
            if (!string.IsNullOrEmpty(hocVien.Email) && await _context.HocVien.AnyAsync(h => h.Email == hocVien.Email && h.MaHocVien != id))
            {
                ModelState.AddModelError("Email", "Email này thuộc về học viên khác!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hocVien);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật thông tin học viên thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.HocVien.AnyAsync(e => e.MaHocVien == hocVien.MaHocVien))
                        return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(hocVien);
        }

        // 5. XÓA (POST) - RÀNG BUỘC KHÓA NGOẠI
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var hocVien = await _context.HocVien.FindAsync(id);
            if (hocVien == null) return NotFound();

            // RÀNG BUỘC: Không cho phép xóa học viên đã phát sinh lịch sử đăng ký lớp
            bool daDangKy = await _context.DangKyLop.AnyAsync(d => d.MaHocVien == id);
            if (daDangKy)
            {
                TempData["Error"] = "Không thể xóa học viên này vì đã có dữ liệu đăng ký lớp học!";
                return RedirectToAction(nameof(Index));
            }

            _context.HocVien.Remove(hocVien);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xóa học viên thành công!";

            return RedirectToAction(nameof(Index));
        }
    }
}