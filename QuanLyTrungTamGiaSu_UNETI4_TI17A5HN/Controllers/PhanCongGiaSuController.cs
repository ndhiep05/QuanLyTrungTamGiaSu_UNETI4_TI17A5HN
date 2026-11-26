// Họ và tên: SV4
// Mã sinh viên: SV4
// Nội dung thực hiện: Nhóm nghiệp vụ SV4 - Phân công gia sư cho lớp học (2.1)
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Data;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Filters;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.ViewModels;

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Controllers
{
    [PhanQuyen(VaiTro.Admin, VaiTro.GiaSu)]
    public class PhanCongGiaSuController : Controller
    {
        private const int PageSize = 10;
        private readonly AppDbContext _context;

        public PhanCongGiaSuController(AppDbContext context)
        {
            _context = context;
        }

        // GET: PhanCongGiaSu
        public async Task<IActionResult> Index(string? tuKhoa, int page = 1)
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");

            var query = _context.PhanCongGiaSu
                .Include(p => p.LopHoc)
                    .ThenInclude(l => l!.MonHoc)
                .Include(p => p.GiaSu)
                .AsQueryable();

            // Nếu là Gia sư: chỉ hiển thị phân công của chính mình
            if (vaiTro == VaiTro.GiaSu)
            {
                var giaSu = await _context.GiaSu.FirstOrDefaultAsync(g => g.MaTaiKhoan == maTaiKhoan);
                if (giaSu != null)
                {
                    query = query.Where(p => p.MaGiaSu == giaSu.MaGiaSu);
                }
                else
                {
                    query = query.Where(p => false);
                }
            }

            // Tìm kiếm theo từ khóa
            tuKhoa = tuKhoa?.Trim();
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                query = query.Where(p =>
                    (p.LopHoc != null && p.LopHoc.TenLop.Contains(tuKhoa)) ||
                    (p.GiaSu != null && p.GiaSu.HoTen.Contains(tuKhoa)) ||
                    (p.LopHoc != null && p.LopHoc.MonHoc != null && p.LopHoc.MonHoc.TenMonHoc.Contains(tuKhoa)));
            }

            var tongSoBanGhi = await query.CountAsync();
            var tongSoTrang = (int)Math.Ceiling((double)tongSoBanGhi / PageSize);
            page = Math.Clamp(page, 1, Math.Max(tongSoTrang, 1));

            var danhSach = await query
                .OrderByDescending(p => p.NgayPhanCong)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.PhanTrang = new PhanTrangViewModel
            {
                Page = page,
                PageSize = PageSize,
                TotalItems = tongSoBanGhi
            };

            return View(danhSach);
        }

        // GET: PhanCongGiaSu/Create
        [AdminOnly]
        public async Task<IActionResult> Create()
        {
            await NapDanhSachLuaChonAsync();
            return View(new PhanCongGiaSu
            {
                NgayPhanCong = DateTime.Now,
                TrangThai = "Đang phụ trách"
            });
        }

        // POST: PhanCongGiaSu/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> Create([Bind("MaLop,MaGiaSu,GhiChu")] PhanCongGiaSu phanCong)
        {
            var lop = await _context.LopHoc
                .Include(l => l.MonHoc)
                .FirstOrDefaultAsync(l => l.MaLop == phanCong.MaLop);

            if (lop == null)
            {
                ModelState.AddModelError(nameof(phanCong.MaLop), "Lớp học không tồn tại.");
            }

            var giaSu = await _context.GiaSu.FirstOrDefaultAsync(g => g.MaGiaSu == phanCong.MaGiaSu);
            if (giaSu == null)
            {
                ModelState.AddModelError(nameof(phanCong.MaGiaSu), "Gia sư không tồn tại.");
            }

            // Kiểm tra chuyên môn của gia sư có phù hợp với môn học của lớp không
            if (lop != null && giaSu != null)
            {
                var coChuyenMon = await _context.ChuyenMonGiaSu.AnyAsync(c =>
                    c.MaGiaSu == phanCong.MaGiaSu &&
                    c.MaMonHoc == lop.MaMonHoc &&
                    (c.TrangThai == TrangThaiChung.HoatDong || c.TrangThai == "HoatDong"));

                if (!coChuyenMon)
                {
                    ModelState.AddModelError(nameof(phanCong.MaGiaSu),
                        $"Gia sư {giaSu.HoTen} chưa có chuyên môn môn '{lop.MonHoc?.TenMonHoc}' đang hoạt động.");
                }
            }

            // Chặn phân công trùng: nếu gia sư đang phụ trách lớp này rồi thì không phân công lại
            var daPhanCong = await _context.PhanCongGiaSu.AnyAsync(p =>
                p.MaLop == phanCong.MaLop &&
                p.MaGiaSu == phanCong.MaGiaSu &&
                p.TrangThai == "Đang phụ trách");

            if (daPhanCong)
            {
                ModelState.AddModelError(string.Empty, "Gia sư này đã và đang phụ trách lớp học này.");
            }

            if (!ModelState.IsValid)
            {
                await NapDanhSachLuaChonAsync(phanCong.MaLop, phanCong.MaGiaSu);
                return View(phanCong);
            }

            phanCong.NgayPhanCong = DateTime.Now;
            phanCong.TrangThai = "Đang phụ trách";
            phanCong.GhiChu = phanCong.GhiChu?.Trim();

            _context.PhanCongGiaSu.Add(phanCong);
            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = "Phân công gia sư thành công.";
            return RedirectToAction(nameof(Index));
        }

        // GET: PhanCongGiaSu/Edit/5
        [AdminOnly]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var phanCong = await _context.PhanCongGiaSu
                .Include(p => p.LopHoc)
                .Include(p => p.GiaSu)
                .FirstOrDefaultAsync(p => p.MaPhanCong == id);

            if (phanCong == null) return NotFound();

            await NapDanhSachLuaChonAsync(phanCong.MaLop, phanCong.MaGiaSu);
            return View(phanCong);
        }

        // POST: PhanCongGiaSu/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> Edit(int id, [Bind("MaPhanCong,MaLop,MaGiaSu,TrangThai,GhiChu")] PhanCongGiaSu input)
        {
            if (id != input.MaPhanCong) return NotFound();

            var phanCong = await _context.PhanCongGiaSu.FirstOrDefaultAsync(p => p.MaPhanCong == id);
            if (phanCong == null) return NotFound();

            var lop = await _context.LopHoc
                .Include(l => l.MonHoc)
                .FirstOrDefaultAsync(l => l.MaLop == input.MaLop);

            if (lop == null)
            {
                ModelState.AddModelError(nameof(input.MaLop), "Lớp học không tồn tại.");
            }

            var giaSu = await _context.GiaSu.FirstOrDefaultAsync(g => g.MaGiaSu == input.MaGiaSu);
            if (giaSu == null)
            {
                ModelState.AddModelError(nameof(input.MaGiaSu), "Gia sư không tồn tại.");
            }

            // Kiểm tra chuyên môn
            if (lop != null && giaSu != null)
            {
                var coChuyenMon = await _context.ChuyenMonGiaSu.AnyAsync(c =>
                    c.MaGiaSu == input.MaGiaSu &&
                    c.MaMonHoc == lop.MaMonHoc &&
                    (c.TrangThai == TrangThaiChung.HoatDong || c.TrangThai == "HoatDong"));

                if (!coChuyenMon)
                {
                    ModelState.AddModelError(nameof(input.MaGiaSu),
                        $"Gia sư {giaSu.HoTen} chưa có chuyên môn môn '{lop.MonHoc?.TenMonHoc}' đang hoạt động.");
                }
            }

            // Chặn phân công trùng đang phụ trách
            if (input.TrangThai == "Đang phụ trách")
            {
                var daPhanCong = await _context.PhanCongGiaSu.AnyAsync(p =>
                    p.MaPhanCong != id &&
                    p.MaLop == input.MaLop &&
                    p.MaGiaSu == input.MaGiaSu &&
                    p.TrangThai == "Đang phụ trách");

                if (daPhanCong)
                {
                    ModelState.AddModelError(string.Empty, "Gia sư này đã có phân công đang phụ trách cho lớp học này.");
                }
            }

            if (!ModelState.IsValid)
            {
                await NapDanhSachLuaChonAsync(input.MaLop, input.MaGiaSu);
                return View(input);
            }

            phanCong.MaLop = input.MaLop;
            phanCong.MaGiaSu = input.MaGiaSu;
            phanCong.TrangThai = input.TrangThai.Trim();
            phanCong.GhiChu = input.GhiChu?.Trim();

            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = "Cập nhật phân công gia sư thành công.";
            return RedirectToAction(nameof(Index));
        }

        // GET: PhanCongGiaSu/Delete/5
        [AdminOnly]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var phanCong = await _context.PhanCongGiaSu
                .Include(p => p.LopHoc)
                .Include(p => p.GiaSu)
                .FirstOrDefaultAsync(p => p.MaPhanCong == id);

            if (phanCong == null) return NotFound();

            return View(phanCong);
        }

        // POST: PhanCongGiaSu/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var phanCong = await _context.PhanCongGiaSu.FindAsync(id);
            if (phanCong == null) return NotFound();

            // Chặn xóa nếu đã có lịch học liên quan tới gia sư và lớp này
            var coLichHoc = await _context.LichHoc.AnyAsync(l =>
                l.MaLop == phanCong.MaLop && l.MaGiaSu == phanCong.MaGiaSu);

            if (coLichHoc)
            {
                TempData["Loi"] = "Không thể xóa phân công vì đã có buổi học trong lịch học của gia sư cho lớp này. Hãy chuyển trạng thái sang 'Đã hủy' hoặc 'Đã kết thúc'.";
                return RedirectToAction(nameof(Index));
            }

            _context.PhanCongGiaSu.Remove(phanCong);
            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = "Xóa phân công gia sư thành công.";
            return RedirectToAction(nameof(Index));
        }

        private async Task NapDanhSachLuaChonAsync(int? selectedMaLop = null, int? selectedMaGiaSu = null)
        {
            var lops = await _context.LopHoc
                .AsNoTracking()
                .Include(l => l.MonHoc)
                .Where(l => l.TrangThai != TrangThaiLop.DaHuy && l.TrangThai != TrangThaiLop.DaKetThuc || l.MaLop == selectedMaLop)
                .OrderBy(l => l.TenLop)
                .Select(l => new
                {
                    l.MaLop,
                    TenHienThi = l.TenLop + (l.MonHoc != null ? $" ({l.MonHoc.TenMonHoc})" : "")
                })
                .ToListAsync();

            var giaSus = await _context.GiaSu
                .AsNoTracking()
                .Where(g => g.TrangThai == TrangThaiChung.HoatDong || g.MaGiaSu == selectedMaGiaSu)
                .OrderBy(g => g.HoTen)
                .Select(g => new
                {
                    g.MaGiaSu,
                    TenHienThi = g.HoTen + (!string.IsNullOrEmpty(g.SoDienThoai) ? $" - {g.SoDienThoai}" : "")
                })
                .ToListAsync();

            ViewBag.LopHocList = new SelectList(lops, "MaLop", "TenHienThi", selectedMaLop);
            ViewBag.GiaSuList = new SelectList(giaSus, "MaGiaSu", "TenHienThi", selectedMaGiaSu);
        }
    }
}
