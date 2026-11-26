// Họ và tên: SV4
// Mã sinh viên: SV4
// Nội dung thực hiện: Nhóm nghiệp vụ SV4 - Lập lịch học, kiểm tra trùng lịch (2.2), hoàn thành hoặc hủy buổi học (2.4)
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Data;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Filters;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.ViewModels;

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Controllers
{
    [PhanQuyen(VaiTro.Admin, VaiTro.GiaSu, VaiTro.HocVien)]
    public class LichHocController : Controller
    {
        private const int PageSize = 10;
        private readonly AppDbContext _context;

        public LichHocController(AppDbContext context)
        {
            _context = context;
        }

        // GET: LichHoc
        public async Task<IActionResult> Index(string? tuKhoa, int page = 1)
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");

            var query = _context.LichHoc
                .Include(l => l.LopHoc)
                    .ThenInclude(lop => lop!.MonHoc)
                .Include(l => l.GiaSu)
                .AsQueryable();

            // Phân quyền theo vai trò
            if (vaiTro == VaiTro.GiaSu)
            {
                var giaSu = await _context.GiaSu.FirstOrDefaultAsync(g => g.MaTaiKhoan == maTaiKhoan);
                if (giaSu != null)
                {
                    query = query.Where(l => l.MaGiaSu == giaSu.MaGiaSu);
                }
                else
                {
                    query = query.Where(l => false);
                }
            }
            else if (vaiTro == VaiTro.HocVien)
            {
                var hocVien = await _context.HocVien.FirstOrDefaultAsync(h => h.MaTaiKhoan == maTaiKhoan);
                if (hocVien != null)
                {
                    // Lấy các lớp mà học viên đã đăng ký hợp lệ (Đã xác nhận hoặc Hoàn thành)
                    var lopIds = await _context.DangKyLop
                        .Where(d => d.MaHocVien == hocVien.MaHocVien &&
                                    d.TrangThai != TrangThaiDangKy.DaHuy)
                        .Select(d => d.MaLop)
                        .ToListAsync();

                    query = query.Where(l => lopIds.Contains(l.MaLop));
                }
                else
                {
                    query = query.Where(l => false);
                }
            }

            // Tìm kiếm theo từ khóa
            tuKhoa = tuKhoa?.Trim();
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                query = query.Where(l =>
                    (l.LopHoc != null && l.LopHoc.TenLop.Contains(tuKhoa)) ||
                    (l.GiaSu != null && l.GiaSu.HoTen.Contains(tuKhoa)) ||
                    (l.PhongHocHoacHinhThuc != null && l.PhongHocHoacHinhThuc.Contains(tuKhoa)));
            }

            var tongSoBanGhi = await query.CountAsync();
            var tongSoTrang = (int)Math.Ceiling((double)tongSoBanGhi / PageSize);
            page = Math.Clamp(page, 1, Math.Max(tongSoTrang, 1));

            var danhSach = await query
                .OrderByDescending(l => l.ThoiGianBatDau)
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

        // GET: LichHoc/Create
        [AdminOnly]
        public async Task<IActionResult> Create()
        {
            await NapDanhSachLuaChonAsync();
            var batDau = DateTime.Today.AddDays(1).AddHours(8);
            return View(new LichHoc
            {
                ThoiGianBatDau = batDau,
                ThoiGianKetThuc = batDau.AddHours(2),
                TrangThai = TrangThaiLich.DaLenLich
            });
        }

        // POST: LichHoc/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> Create([Bind("MaLop,MaGiaSu,ThoiGianBatDau,ThoiGianKetThuc,PhongHocHoacHinhThuc,GhiChu")] LichHoc input)
        {
            await KiemTraLichHocHopLeAsync(input, excludeId: null);

            if (!ModelState.IsValid)
            {
                await NapDanhSachLuaChonAsync(input.MaLop, input.MaGiaSu);
                return View(input);
            }

            input.TrangThai = TrangThaiLich.DaLenLich;
            input.PhongHocHoacHinhThuc = input.PhongHocHoacHinhThuc?.Trim();
            input.GhiChu = input.GhiChu?.Trim();

            _context.LichHoc.Add(input);
            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = "Lập lịch học thành công.";
            return RedirectToAction(nameof(Index));
        }

        // GET: LichHoc/Edit/5
        [AdminOnly]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var lichHoc = await _context.LichHoc
                .Include(l => l.LopHoc)
                .Include(l => l.GiaSu)
                .FirstOrDefaultAsync(l => l.MaLichHoc == id);

            if (lichHoc == null) return NotFound();

            await NapDanhSachLuaChonAsync(lichHoc.MaLop, lichHoc.MaGiaSu);
            return View(lichHoc);
        }

        // POST: LichHoc/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> Edit(int id, [Bind("MaLichHoc,MaLop,MaGiaSu,ThoiGianBatDau,ThoiGianKetThuc,PhongHocHoacHinhThuc,TrangThai,GhiChu")] LichHoc input)
        {
            if (id != input.MaLichHoc) return NotFound();

            var lichHoc = await _context.LichHoc.FirstOrDefaultAsync(l => l.MaLichHoc == id);
            if (lichHoc == null) return NotFound();

            await KiemTraLichHocHopLeAsync(input, excludeId: id);

            if (!ModelState.IsValid)
            {
                await NapDanhSachLuaChonAsync(input.MaLop, input.MaGiaSu);
                return View(input);
            }

            lichHoc.MaLop = input.MaLop;
            lichHoc.MaGiaSu = input.MaGiaSu;
            lichHoc.ThoiGianBatDau = input.ThoiGianBatDau;
            lichHoc.ThoiGianKetThuc = input.ThoiGianKetThuc;
            lichHoc.PhongHocHoacHinhThuc = input.PhongHocHoacHinhThuc?.Trim();
            lichHoc.TrangThai = input.TrangThai.Trim();
            lichHoc.GhiChu = input.GhiChu?.Trim();

            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = "Cập nhật lịch học thành công.";
            return RedirectToAction(nameof(Index));
        }

        // GET: LichHoc/Delete/5
        [AdminOnly]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var lichHoc = await _context.LichHoc
                .Include(l => l.LopHoc)
                .Include(l => l.GiaSu)
                .FirstOrDefaultAsync(l => l.MaLichHoc == id);

            if (lichHoc == null) return NotFound();

            return View(lichHoc);
        }

        // POST: LichHoc/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lichHoc = await _context.LichHoc.FindAsync(id);
            if (lichHoc == null) return NotFound();

            // Chặn xóa nếu buổi học đã có dữ liệu điểm danh
            var coDiemDanh = await _context.DiemDanh.AnyAsync(d => d.MaLichHoc == id);
            if (coDiemDanh)
            {
                TempData["Loi"] = "Không thể xóa buổi học đã có dữ liệu điểm danh. Vui lòng chuyển trạng thái sang 'Đã hủy'.";
                return RedirectToAction(nameof(Index));
            }

            _context.LichHoc.Remove(lichHoc);
            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = "Xóa lịch học thành công.";
            return RedirectToAction(nameof(Index));
        }

        // POST: LichHoc/HoanThanh/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HoanThanh(int id)
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");

            var lichHoc = await _context.LichHoc
                .Include(l => l.GiaSu)
                .FirstOrDefaultAsync(l => l.MaLichHoc == id);

            if (lichHoc == null) return NotFound();

            if (vaiTro == VaiTro.GiaSu && lichHoc.GiaSu?.MaTaiKhoan != maTaiKhoan)
            {
                return RedirectToAction("KhongCoQuyen", "Home");
            }

            lichHoc.TrangThai = TrangThaiLich.DaHoanThanh;
            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = "Buổi học đã được đánh dấu hoàn thành.";
            return RedirectToAction(nameof(Index));
        }

        // POST: LichHoc/Huy/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Huy(int id, string? lyDo)
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");

            var lichHoc = await _context.LichHoc
                .Include(l => l.GiaSu)
                .FirstOrDefaultAsync(l => l.MaLichHoc == id);

            if (lichHoc == null) return NotFound();

            if (vaiTro == VaiTro.GiaSu && lichHoc.GiaSu?.MaTaiKhoan != maTaiKhoan)
            {
                return RedirectToAction("KhongCoQuyen", "Home");
            }

            lichHoc.TrangThai = TrangThaiLich.DaHuy;
            if (!string.IsNullOrWhiteSpace(lyDo))
            {
                lichHoc.GhiChu = string.IsNullOrWhiteSpace(lichHoc.GhiChu)
                    ? $"[Đã hủy: {lyDo.Trim()}]"
                    : $"{lichHoc.GhiChu} | [Đã hủy: {lyDo.Trim()}]";
            }

            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = "Hủy buổi học thành công.";
            return RedirectToAction(nameof(Index));
        }

        private async Task KiemTraLichHocHopLeAsync(LichHoc input, int? excludeId)
        {
            if (input.ThoiGianKetThuc <= input.ThoiGianBatDau)
            {
                ModelState.AddModelError(nameof(input.ThoiGianKetThuc), "Thời gian kết thúc phải diễn ra sau thời gian bắt đầu.");
            }

            var lop = await _context.LopHoc.FindAsync(input.MaLop);
            if (lop == null)
            {
                ModelState.AddModelError(nameof(input.MaLop), "Lớp học không tồn tại.");
            }

            var giaSu = await _context.GiaSu.FindAsync(input.MaGiaSu);
            if (giaSu == null)
            {
                ModelState.AddModelError(nameof(input.MaGiaSu), "Gia sư không tồn tại.");
            }

            // Kiểm tra phân công gia sư phụ trách lớp học
            if (lop != null && giaSu != null)
            {
                var duocPhanCong = await _context.PhanCongGiaSu.AnyAsync(p =>
                    p.MaLop == input.MaLop &&
                    p.MaGiaSu == input.MaGiaSu &&
                    p.TrangThai == "Đang phụ trách");

                if (!duocPhanCong)
                {
                    ModelState.AddModelError(nameof(input.MaGiaSu),
                        $"Gia sư {giaSu.HoTen} hiện không được phân công phụ trách lớp '{lop.TenLop}'.");
                }
            }

            // Kiểm tra trùng lịch / xung đột thời gian (giao nhau giữa 2 khoảng thời gian):
            // (StartA < EndB && EndA > StartB)
            if (input.ThoiGianBatDau < input.ThoiGianKetThuc)
            {
                // 1. Trùng lịch của Gia sư
                var trungGiaSu = await _context.LichHoc
                    .Include(l => l.LopHoc)
                    .AnyAsync(l =>
                        l.MaLichHoc != excludeId &&
                        l.MaGiaSu == input.MaGiaSu &&
                        l.TrangThai != TrangThaiLich.DaHuy &&
                        l.ThoiGianBatDau < input.ThoiGianKetThuc &&
                        l.ThoiGianKetThuc > input.ThoiGianBatDau);

                if (trungGiaSu)
                {
                    ModelState.AddModelError(nameof(input.MaGiaSu),
                        "Gia sư đã có lịch dạy khác trong khoảng thời gian này.");
                }

                // 2. Trùng lịch của Lớp học
                var trungLop = await _context.LichHoc
                    .AnyAsync(l =>
                        l.MaLichHoc != excludeId &&
                        l.MaLop == input.MaLop &&
                        l.TrangThai != TrangThaiLich.DaHuy &&
                        l.ThoiGianBatDau < input.ThoiGianKetThuc &&
                        l.ThoiGianKetThuc > input.ThoiGianBatDau);

                if (trungLop)
                {
                    ModelState.AddModelError(nameof(input.MaLop),
                        "Lớp học này đã có buổi học khác trong cùng khoảng thời gian này.");
                }

                // 3. Trùng phòng học / hình thức (nếu có nhập)
                if (!string.IsNullOrWhiteSpace(input.PhongHocHoacHinhThuc))
                {
                    var phong = input.PhongHocHoacHinhThuc.Trim();
                    var trungPhong = await _context.LichHoc
                        .Include(l => l.LopHoc)
                        .AnyAsync(l =>
                            l.MaLichHoc != excludeId &&
                            l.PhongHocHoacHinhThuc != null &&
                            l.PhongHocHoacHinhThuc.ToLower() == phong.ToLower() &&
                            l.TrangThai != TrangThaiLich.DaHuy &&
                            l.ThoiGianBatDau < input.ThoiGianKetThuc &&
                            l.ThoiGianKetThuc > input.ThoiGianBatDau);

                    if (trungPhong)
                    {
                        ModelState.AddModelError(nameof(input.PhongHocHoacHinhThuc),
                            $"Phòng học/Hình thức '{phong}' đã có lớp học khác sử dụng trong khoảng thời gian này.");
                    }
                }
            }
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
