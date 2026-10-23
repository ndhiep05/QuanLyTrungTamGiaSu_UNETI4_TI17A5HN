// Họ và tên: Nguyễn Trọng Chuyển
// Mã sinh viên: 23103100204
// Nội dung thực hiện: Module 3 - Controller Quản lý Lớp học

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Data;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Filters;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.ViewModels;

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Controllers
{
    public class LopHocController : Controller
    {
        private readonly AppDbContext _context;

        public LopHocController(AppDbContext context)
        {
            _context = context;
        }

        // 1. DANH SÁCH LỚP HỌC (Tìm kiếm + Lọc + Phân trang)
        public async Task<IActionResult> Index(string? tuKhoa, int? maMonHoc, int trang = 1)
        {
            int pageSize = 10;
            var query = _context.LopHoc
                .Include(l => l.MonHoc)
                .Include(l => l.DangKyLops)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var kw = tuKhoa.Trim();
                query = query.Where(l => l.TenLop.Contains(kw) || (l.MoTa != null && l.MoTa.Contains(kw)));
            }

            if (maMonHoc.HasValue && maMonHoc > 0)
            {
                query = query.Where(l => l.MaMonHoc == maMonHoc);
            }

            int totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(l => l.MaLop)
                .Skip((trang - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.MaMonHoc = new SelectList(await _context.MonHoc.ToListAsync(), "MaMonHoc", "TenMonHoc", maMonHoc);
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

        // 2. CHI TIẾT LỚP HỌC
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var lopHoc = await _context.LopHoc
                .Include(l => l.MonHoc)
                .Include(l => l.DangKyLops)
                    .ThenInclude(d => d.HocVien)
                .FirstOrDefaultAsync(m => m.MaLop == id);

            if (lopHoc == null) return NotFound();

            return View(lopHoc);
        }

        // 3. THÊM MỚI LỚP HỌC (GET) - Chỉ Admin
        [AdminOnly]
        public async Task<IActionResult> Create()
        {
            ViewBag.MaMonHoc = new SelectList(await _context.MonHoc.ToListAsync(), "MaMonHoc", "TenMonHoc");
            return View(new LopHoc
            {
                NgayBatDau = DateTime.Today.AddDays(7),
                NgayKetThuc = DateTime.Today.AddMonths(3),
                SiSoToiDa = 20,
                HocPhi = 0,
                TrangThai = TrangThaiLop.DangMoDangKy
            });
        }

        // 3. THÊM MỚI LỚP HỌC (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> Create([Bind("MaLop,TenLop,MaMonHoc,TrinhDoLop,NgayBatDau,NgayKetThuc,SiSoToiDa,HocPhi,TrangThai,CapDo,MoTa,GhiChu")] LopHoc lopHoc)
        {
            ModelState.Remove(nameof(LopHoc.MonHoc));
            ModelState.Remove(nameof(LopHoc.DangKyLops));
            ModelState.Remove(nameof(LopHoc.PhanCongs));
            ModelState.Remove(nameof(LopHoc.LichHocs));

            if (lopHoc.NgayKetThuc <= lopHoc.NgayBatDau)
            {
                ModelState.AddModelError(nameof(lopHoc.NgayKetThuc), "Ngày kết thúc phải sau ngày bắt đầu!");
            }

            if (ModelState.IsValid)
            {
                _context.Add(lopHoc);
                await _context.SaveChangesAsync();
                TempData["ThanhCong"] = TempData["Success"] = "Thêm mới lớp học thành công!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.MaMonHoc = new SelectList(await _context.MonHoc.ToListAsync(), "MaMonHoc", "TenMonHoc", lopHoc.MaMonHoc);
            return View(lopHoc);
        }

        // 4. CẬP NHẬT LỚP HỌC (GET) - Chỉ Admin
        [AdminOnly]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var lopHoc = await _context.LopHoc.FindAsync(id);
            if (lopHoc == null) return NotFound();

            ViewBag.MaMonHoc = new SelectList(await _context.MonHoc.ToListAsync(), "MaMonHoc", "TenMonHoc", lopHoc.MaMonHoc);
            return View(lopHoc);
        }

        // 4. CẬP NHẬT LỚP HỌC (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> Edit(int id, [Bind("MaLop,TenLop,MaMonHoc,TrinhDoLop,NgayBatDau,NgayKetThuc,SiSoToiDa,HocPhi,TrangThai,CapDo,MoTa,GhiChu")] LopHoc lopHoc)
        {
            if (id != lopHoc.MaLop) return NotFound();

            ModelState.Remove(nameof(LopHoc.MonHoc));
            ModelState.Remove(nameof(LopHoc.DangKyLops));
            ModelState.Remove(nameof(LopHoc.PhanCongs));
            ModelState.Remove(nameof(LopHoc.LichHocs));

            if (lopHoc.NgayKetThuc <= lopHoc.NgayBatDau)
            {
                ModelState.AddModelError(nameof(lopHoc.NgayKetThuc), "Ngày kết thúc phải sau ngày bắt đầu!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(lopHoc);
                    await _context.SaveChangesAsync();
                    TempData["ThanhCong"] = TempData["Success"] = "Cập nhật thông tin lớp học thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.LopHoc.AnyAsync(e => e.MaLop == lopHoc.MaLop))
                        return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.MaMonHoc = new SelectList(await _context.MonHoc.ToListAsync(), "MaMonHoc", "TenMonHoc", lopHoc.MaMonHoc);
            return View(lopHoc);
        }

        // 5. XÓA LỚP HỌC (POST) - Chỉ Admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> Delete(int id)
        {
            var lopHoc = await _context.LopHoc.FindAsync(id);
            if (lopHoc == null) return NotFound();

            bool daCoDangKy = await _context.DangKyLop.AnyAsync(d => d.MaLop == id);
            if (daCoDangKy)
            {
                TempData["Loi"] = TempData["Error"] = "Không thể xóa lớp học này vì đã có dữ liệu học viên đăng ký!";
                return RedirectToAction(nameof(Index));
            }

            _context.LopHoc.Remove(lopHoc);
            await _context.SaveChangesAsync();
            TempData["ThanhCong"] = TempData["Success"] = "Xóa lớp học thành công!";

            return RedirectToAction(nameof(Index));
        }
    }
}
