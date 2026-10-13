using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Data;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Filters;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.ViewModels;

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Controllers
{
    [AdminOnly]
    public class MonHocController : Controller
    {
        private readonly AppDbContext _context;

        public MonHocController(AppDbContext context)
        {
            _context = context;
        }

        // GET: MonHoc
        [HttpGet]
        public async Task<IActionResult> Index(string? tuKhoa, int page = 1)
        {
            const int kichThuocTrang = 10;
            if (page < 1) page = 1;

            var truyVan = _context.MonHoc.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();

                truyVan = truyVan.Where(m =>
                    m.TenMonHoc.Contains(tuKhoa) ||
                    (m.CapDo != null && m.CapDo.Contains(tuKhoa)) ||
                    (m.MoTa != null && m.MoTa.Contains(tuKhoa)));
            }

            var tongSoBanGhi = await truyVan.CountAsync();
            var tongSoTrang = (int)Math.Ceiling((double)tongSoBanGhi / kichThuocTrang);

            if (tongSoTrang > 0 && page > tongSoTrang)
            {
                page = tongSoTrang;
            }

            var danhSach = await truyVan
                .OrderBy(m => m.TenMonHoc)
                .Skip((page - 1) * kichThuocTrang)
                .Take(kichThuocTrang)
                .ToListAsync();

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.PhanTrang = new PhanTrangViewModel
            {
                Page = page,
                PageSize = kichThuocTrang,
                TotalItems = tongSoBanGhi
            };

            return View(danhSach);
        }

        // GET: MonHoc/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var monHoc = await _context.MonHoc
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.MaMonHoc == id);

            if (monHoc == null)
                return NotFound();

            return View(monHoc);
        }

        // GET: MonHoc/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new MonHoc());
        }

        // POST: MonHoc/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MonHoc monHoc)
        {
            // Bỏ validate navigation properties
            ModelState.Remove(nameof(MonHoc.ChuyenMons));
            ModelState.Remove(nameof(MonHoc.LopHocs));

            var tenMon = monHoc.TenMonHoc?.Trim() ?? "";

            // Chặn trùng tên môn học
            if (await _context.MonHoc.AnyAsync(m => m.TenMonHoc.ToLower() == tenMon.ToLower()))
            {
                ModelState.AddModelError(nameof(monHoc.TenMonHoc), "Tên môn học đã tồn tại trong hệ thống.");
            }

            if (!ModelState.IsValid)
            {
                return View(monHoc);
            }

            monHoc.TenMonHoc = tenMon;
            monHoc.MoTa = monHoc.MoTa?.Trim();

            _context.MonHoc.Add(monHoc);
            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = "Thêm môn học thành công.";
            TempData["ThongBao"] = TempData["ThanhCong"];

            return RedirectToAction(nameof(Index));
        }

        // GET: MonHoc/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var monHoc = await _context.MonHoc.FindAsync(id);

            if (monHoc == null)
                return NotFound();

            return View(monHoc);
        }

        // POST: MonHoc/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MonHoc monHoc)
        {
            if (id != monHoc.MaMonHoc)
                return NotFound();

            ModelState.Remove(nameof(MonHoc.ChuyenMons));
            ModelState.Remove(nameof(MonHoc.LopHocs));

            var tenMon = monHoc.TenMonHoc?.Trim() ?? "";

            // Chặn trùng tên môn học với các môn khác
            if (await _context.MonHoc.AnyAsync(m => m.MaMonHoc != id && m.TenMonHoc.ToLower() == tenMon.ToLower()))
            {
                ModelState.AddModelError(nameof(monHoc.TenMonHoc), "Tên môn học đã tồn tại trong hệ thống.");
            }

            if (!ModelState.IsValid)
            {
                return View(monHoc);
            }

            var monHocDb = await _context.MonHoc.FirstOrDefaultAsync(m => m.MaMonHoc == id);
            if (monHocDb == null)
            {
                return NotFound();
            }

            monHocDb.TenMonHoc = tenMon;
            monHocDb.CapDo = monHoc.CapDo;
            monHocDb.MoTa = monHoc.MoTa?.Trim();
            monHocDb.TrangThai = monHoc.TrangThai;

            try
            {
                await _context.SaveChangesAsync();
                TempData["ThanhCong"] = "Cập nhật môn học thành công.";
                TempData["ThongBao"] = TempData["ThanhCong"];
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.MonHoc.AnyAsync(m => m.MaMonHoc == id))
                {
                    return NotFound();
                }
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: MonHoc/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var monHoc = await _context.MonHoc
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.MaMonHoc == id);

            if (monHoc == null)
                return NotFound();

            // Kiểm tra ràng buộc dữ liệu
            var soGiaSu = await _context.ChuyenMonGiaSu.CountAsync(c => c.MaMonHoc == id);
            var soLopHoc = await _context.LopHoc.CountAsync(l => l.MaMonHoc == id);

            ViewBag.CoLienQuan = (soGiaSu > 0 || soLopHoc > 0);
            ViewBag.SoGiaSu = soGiaSu;
            ViewBag.SoLopHoc = soLopHoc;

            return View(monHoc);
        }

        // POST: MonHoc/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Kiểm tra chặn xóa khi còn gia sư hoặc lớp liên quan
            var coGiaSu = await _context.ChuyenMonGiaSu.AnyAsync(c => c.MaMonHoc == id);
            var coLopHoc = await _context.LopHoc.AnyAsync(l => l.MaMonHoc == id);

            if (coGiaSu || coLopHoc)
            {
                var lyDo = new List<string>();
                if (coGiaSu) lyDo.Add("gia sư chuyên môn");
                if (coLopHoc) lyDo.Add("lớp học đang mở");

                TempData["Loi"] = $"Không thể xóa môn học vì đang có {string.Join(" và ", lyDo)} liên quan. Hãy chuyển trạng thái sang Khóa.";
                TempData["ThongBao"] = TempData["Loi"];
                return RedirectToAction(nameof(Index));
            }

            var monHoc = await _context.MonHoc.FindAsync(id);
            if (monHoc != null)
            {
                _context.MonHoc.Remove(monHoc);
                try
                {
                    await _context.SaveChangesAsync();
                    TempData["ThanhCong"] = "Xóa môn học thành công.";
                    TempData["ThongBao"] = TempData["ThanhCong"];
                }
                catch (DbUpdateException)
                {
                    TempData["Loi"] = "Không thể xóa môn học do có ràng buộc dữ liệu trong hệ thống.";
                    TempData["ThongBao"] = TempData["Loi"];
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}