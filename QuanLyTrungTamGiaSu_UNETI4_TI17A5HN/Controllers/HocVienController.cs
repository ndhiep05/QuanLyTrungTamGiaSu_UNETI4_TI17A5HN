// Họ và tên: Nguyễn Trọng Chuyển
// Mã sinh viên: 23103100204
// Nội dung thực hiện: Module 3 - Controller Quản lý Học viên

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Data;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Filters;
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

        // 1. DANH SÁCH HỌC VIÊN (Tìm kiếm + Phân trang) - Chỉ Admin
        [AdminOnly]
        public async Task<IActionResult> Index(string? tuKhoa, int trang = 1)
        {
            int pageSize = 10;
            var query = _context.HocVien.AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim().ToLower();
                query = query.Where(h =>
                    (h.HoTen != null && h.HoTen.ToLower().Contains(tuKhoa)) ||
                    (h.Email != null && h.Email.ToLower().Contains(tuKhoa)) ||
                    (h.SoDienThoai != null && h.SoDienThoai.Contains(tuKhoa)) ||
                    (h.TruongHoc != null && h.TruongHoc.ToLower().Contains(tuKhoa)) ||
                    (h.LopHocHienTai != null && h.LopHocHienTai.ToLower().Contains(tuKhoa)) ||
                    (h.DiaChi != null && h.DiaChi.ToLower().Contains(tuKhoa))
                );
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
                Page = trang,
                PageSize = pageSize,
                TotalItems = totalItems,
                ActionName = "Index"
            };

            return View(items);
        }

        // 2. CHI TIẾT HỌC VIÊN
        [AdminOnly]
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
        [AdminOnly]
        public async Task<IActionResult> Create()
        {
            await NapDanhSachTaiKhoanAsync();
            return View(new HocVien { NgaySinh = DateTime.Today.AddYears(-15) });
        }

        // 3. THÊM MỚI (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> Create([Bind("HoTen,NgaySinh,GioiTinh,SoDienThoai,Email,TruongHoc,LopHocHienTai,DiaChi,TrinhDoHienTai,TrangThai,MaTaiKhoan")] HocVien hocVien)
        {
            ModelState.Remove(nameof(HocVien.TaiKhoan));
            ModelState.Remove(nameof(HocVien.DangKyLops));
            ModelState.Remove(nameof(HocVien.DiemDanhs));

            if (!string.IsNullOrEmpty(hocVien.SoDienThoai) && await _context.HocVien.AnyAsync(h => h.SoDienThoai == hocVien.SoDienThoai))
            {
                ModelState.AddModelError("SoDienThoai", "Số điện thoại này đã tồn tại trong hệ thống!");
            }

            if (!string.IsNullOrEmpty(hocVien.Email) && await _context.HocVien.AnyAsync(h => h.Email == hocVien.Email))
            {
                ModelState.AddModelError("Email", "Email này đã tồn tại trong hệ thống!");
            }

            if (hocVien.MaTaiKhoan > 0 && await _context.HocVien.AnyAsync(h => h.MaTaiKhoan == hocVien.MaTaiKhoan))
            {
                ModelState.AddModelError("MaTaiKhoan", "Tài khoản này đã được liên kết với hồ sơ học viên khác!");
            }

            if (ModelState.IsValid)
            {
                _context.Add(hocVien);
                await _context.SaveChangesAsync();
                TempData["ThanhCong"] = TempData["Success"] = "Thêm mới học viên thành công!";
                return RedirectToAction(nameof(Index));
            }

            await NapDanhSachTaiKhoanAsync(hocVien.MaTaiKhoan);
            return View(hocVien);
        }

        // 4. CHỈNH SỬA (GET)
        [AdminOnly]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var hocVien = await _context.HocVien.FindAsync(id);
            if (hocVien == null) return NotFound();

            await NapDanhSachTaiKhoanAsync(hocVien.MaTaiKhoan);
            return View(hocVien);
        }

        // 4. CHỈNH SỬA (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> Edit(int id, [Bind("MaHocVien,HoTen,NgaySinh,GioiTinh,SoDienThoai,Email,TruongHoc,LopHocHienTai,DiaChi,TrinhDoHienTai,TrangThai,MaTaiKhoan")] HocVien hocVien)
        {
            if (id != hocVien.MaHocVien) return NotFound();

            ModelState.Remove(nameof(HocVien.TaiKhoan));
            ModelState.Remove(nameof(HocVien.DangKyLops));
            ModelState.Remove(nameof(HocVien.DiemDanhs));

            if (!string.IsNullOrEmpty(hocVien.SoDienThoai) && await _context.HocVien.AnyAsync(h => h.SoDienThoai == hocVien.SoDienThoai && h.MaHocVien != id))
            {
                ModelState.AddModelError("SoDienThoai", "Số điện thoại này thuộc về học viên khác!");
            }

            if (!string.IsNullOrEmpty(hocVien.Email) && await _context.HocVien.AnyAsync(h => h.Email == hocVien.Email && h.MaHocVien != id))
            {
                ModelState.AddModelError("Email", "Email này thuộc về học viên khác!");
            }

            if (hocVien.MaTaiKhoan > 0 && await _context.HocVien.AnyAsync(h => h.MaTaiKhoan == hocVien.MaTaiKhoan && h.MaHocVien != id))
            {
                ModelState.AddModelError("MaTaiKhoan", "Tài khoản này đã được liên kết với hồ sơ học viên khác!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hocVien);
                    await _context.SaveChangesAsync();
                    TempData["ThanhCong"] = TempData["Success"] = "Cập nhật thông tin học viên thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.HocVien.AnyAsync(e => e.MaHocVien == hocVien.MaHocVien))
                        return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            await NapDanhSachTaiKhoanAsync(hocVien.MaTaiKhoan);
            return View(hocVien);
        }

        // 5. XÓA (POST) - RÀNG BUỘC KHÓA NGOẠI
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> Delete(int id)
        {
            var hocVien = await _context.HocVien.FindAsync(id);
            if (hocVien == null) return NotFound();

            bool daDangKy = await _context.DangKyLop.AnyAsync(d => d.MaHocVien == id);
            if (daDangKy)
            {
                TempData["Loi"] = TempData["Error"] = "Không thể xóa học viên này vì đã có dữ liệu đăng ký lớp học!";
                return RedirectToAction(nameof(Index));
            }
            _context.HocVien.Remove(hocVien);
            await _context.SaveChangesAsync();
            TempData["ThanhCong"] = TempData["Success"] = "Xóa học viên thành công!";

            return RedirectToAction(nameof(Index));
        }

        // 6. HỒ SƠ HỌC VIÊN (Chuyển tiếp đến trang hồ sơ cá nhân dùng chung)
        [HttpGet]
        public IActionResult HoSo()
        {
            return RedirectToAction("HoSo", "TaiKhoan");
        }

        private async Task NapDanhSachTaiKhoanAsync(int? selectedId = null)
        {
            var danhSach = await _context.TaiKhoan
                .Where(t => t.VaiTro == VaiTro.HocVien &&
                            (!_context.HocVien.Any(h => h.MaTaiKhoan == t.MaTaiKhoan) || (selectedId.HasValue && t.MaTaiKhoan == selectedId.Value)))
                .OrderBy(t => t.HoTen)
                .Select(t => new { t.MaTaiKhoan, TenHienThi = t.HoTen + " (" + t.TenDangNhap + ")" })
                .ToListAsync();

            ViewBag.TaiKhoanList = new SelectList(danhSach, "MaTaiKhoan", "TenHienThi", selectedId);
        }
    }
}
