// Họ và tên: SV4
// Mã sinh viên: SV4
// Nội dung thực hiện: Nhóm nghiệp vụ SV4 - Điểm danh học viên theo buổi học (2.3)
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
    public class DiemDanhController : Controller
    {
        private const int PageSize = 10;
        private readonly AppDbContext _context;

        public DiemDanhController(AppDbContext context)
        {
            _context = context;
        }

        // GET: DiemDanh
        public async Task<IActionResult> Index(string? tuKhoa, int page = 1)
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");

            var query = _context.DiemDanh
                .Include(d => d.HocVien)
                .Include(d => d.LichHoc)
                    .ThenInclude(l => l!.LopHoc)
                .Include(d => d.LichHoc)
                    .ThenInclude(l => l!.GiaSu)
                .AsQueryable();

            // Phân quyền dữ liệu theo vai trò
            if (vaiTro == VaiTro.GiaSu)
            {
                var giaSu = await _context.GiaSu.FirstOrDefaultAsync(g => g.MaTaiKhoan == maTaiKhoan);
                if (giaSu != null)
                {
                    query = query.Where(d => d.LichHoc != null && d.LichHoc.MaGiaSu == giaSu.MaGiaSu);
                }
                else
                {
                    query = query.Where(d => false);
                }
            }
            else if (vaiTro == VaiTro.HocVien)
            {
                var hocVien = await _context.HocVien.FirstOrDefaultAsync(h => h.MaTaiKhoan == maTaiKhoan);
                if (hocVien != null)
                {
                    query = query.Where(d => d.MaHocVien == hocVien.MaHocVien);

                    // Tính tỷ lệ chuyên cần cho học viên
                    var tongBuoiCuaHocVien = await _context.DiemDanh
                        .CountAsync(d => d.MaHocVien == hocVien.MaHocVien);

                    var coMatCuaHocVien = await _context.DiemDanh
                        .CountAsync(d => d.MaHocVien == hocVien.MaHocVien &&
                                         d.TrangThaiDiemDanh == TrangThaiDiemDanh.CoMat);

                    if (tongBuoiCuaHocVien > 0)
                    {
                        ViewBag.TyLeChuyenCan = Math.Round((double)coMatCuaHocVien / tongBuoiCuaHocVien * 100, 1);
                    }
                }
                else
                {
                    query = query.Where(d => false);
                }
            }

            // Tìm kiếm theo từ khóa
            tuKhoa = tuKhoa?.Trim();
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                query = query.Where(d =>
                    (d.HocVien != null && d.HocVien.HoTen.Contains(tuKhoa)) ||
                    (d.LichHoc != null && d.LichHoc.LopHoc != null && d.LichHoc.LopHoc.TenLop.Contains(tuKhoa)));
            }

            var tongSoBanGhi = await query.CountAsync();
            var tongSoTrang = (int)Math.Ceiling((double)tongSoBanGhi / PageSize);
            page = Math.Clamp(page, 1, Math.Max(tongSoTrang, 1));

            var danhSach = await query
                .OrderByDescending(d => d.ThoiGianGhiNhan)
                .ThenByDescending(d => d.MaDiemDanh)
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

        // GET: DiemDanh/TheoBuoi?maLichHoc=5
        public async Task<IActionResult> TheoBuoi(int maLichHoc)
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");

            var lichHoc = await _context.LichHoc
                .Include(l => l.LopHoc)
                .Include(l => l.GiaSu)
                .FirstOrDefaultAsync(l => l.MaLichHoc == maLichHoc);

            if (lichHoc == null) return NotFound();

            // Nếu là Gia sư, chỉ được điểm danh buổi học mình phụ trách
            if (vaiTro == VaiTro.GiaSu && (lichHoc.GiaSu == null || lichHoc.GiaSu.MaTaiKhoan != maTaiKhoan))
            {
                return RedirectToAction("KhongCoQuyen", "Home");
            }

            // Nếu là Học viên, không có quyền vào form điểm danh
            if (vaiTro == VaiTro.HocVien)
            {
                return RedirectToAction("KhongCoQuyen", "Home");
            }

            // Lấy danh sách học viên đã đăng ký hợp lệ vào lớp của buổi học này
            var hocViens = await _context.DangKyLop
                .Include(d => d.HocVien)
                .Where(d => d.MaLop == lichHoc.MaLop &&
                            d.TrangThai != TrangThaiDangKy.DaHuy &&
                            d.HocVien != null)
                .Select(d => d.HocVien!)
                .Distinct()
                .OrderBy(h => h.HoTen)
                .ToListAsync();

            // Lấy danh sách điểm danh hiện tại của buổi học
            var diemDanhHienTai = await _context.DiemDanh
                .Where(d => d.MaLichHoc == maLichHoc)
                .ToDictionaryAsync(d => d.MaHocVien);

            var viewModel = new DiemDanhTheoBuoiViewModel
            {
                MaLichHoc = lichHoc.MaLichHoc,
                TenLop = lichHoc.LopHoc?.TenLop ?? "Chưa rõ",
                TenGiaSu = lichHoc.GiaSu?.HoTen ?? "Chưa rõ",
                ThoiGianBatDau = lichHoc.ThoiGianBatDau,
                ThoiGianKetThuc = lichHoc.ThoiGianKetThuc,
                DanhSach = hocViens.Select(hv =>
                {
                    diemDanhHienTai.TryGetValue(hv.MaHocVien, out var dd);
                    return new DiemDanhDong
                    {
                        MaHocVien = hv.MaHocVien,
                        HoTen = hv.HoTen,
                        TrangThaiDiemDanh = dd?.TrangThaiDiemDanh ?? TrangThaiDiemDanh.CoMat,
                        GhiChu = dd?.GhiChu
                    };
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: DiemDanh/TheoBuoi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TheoBuoi(DiemDanhTheoBuoiViewModel model)
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");

            var lichHoc = await _context.LichHoc
                .Include(l => l.GiaSu)
                .FirstOrDefaultAsync(l => l.MaLichHoc == model.MaLichHoc);

            if (lichHoc == null) return NotFound();

            if (vaiTro == VaiTro.GiaSu && (lichHoc.GiaSu == null || lichHoc.GiaSu.MaTaiKhoan != maTaiKhoan))
            {
                return RedirectToAction("KhongCoQuyen", "Home");
            }

            if (vaiTro == VaiTro.HocVien)
            {
                return RedirectToAction("KhongCoQuyen", "Home");
            }

            var thoiGianGhiNhan = DateTime.Now;
            var diemDanhHienTai = await _context.DiemDanh
                .Where(d => d.MaLichHoc == model.MaLichHoc)
                .ToDictionaryAsync(d => d.MaHocVien);

            foreach (var dong in model.DanhSach)
            {
                if (diemDanhHienTai.TryGetValue(dong.MaHocVien, out var dd))
                {
                    dd.TrangThaiDiemDanh = dong.TrangThaiDiemDanh;
                    dd.GhiChu = dong.GhiChu?.Trim();
                    dd.ThoiGianGhiNhan = thoiGianGhiNhan;
                }
                else
                {
                    _context.DiemDanh.Add(new DiemDanh
                    {
                        MaLichHoc = model.MaLichHoc,
                        MaHocVien = dong.MaHocVien,
                        TrangThaiDiemDanh = dong.TrangThaiDiemDanh,
                        GhiChu = dong.GhiChu?.Trim(),
                        ThoiGianGhiNhan = thoiGianGhiNhan
                    });
                }
            }

            // Khi điểm danh xong, nếu buổi học đang ở trạng thái 'Đã lên lịch', chuyển sang 'Đã hoàn thành'
            if (lichHoc.TrangThai == TrangThaiLich.DaLenLich)
            {
                lichHoc.TrangThai = TrangThaiLich.DaHoanThanh;
            }

            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = "Lưu điểm danh buổi học thành công.";
            return RedirectToAction("Index", "LichHoc");
        }

        // GET: DiemDanh/Create
        [AdminOnly]
        public async Task<IActionResult> Create()
        {
            await NapDanhSachLuaChonAsync();
            return View(new DiemDanh
            {
                ThoiGianGhiNhan = DateTime.Now,
                TrangThaiDiemDanh = TrangThaiDiemDanh.CoMat
            });
        }

        // POST: DiemDanh/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> Create([Bind("MaLichHoc,MaHocVien,TrangThaiDiemDanh,GhiChu")] DiemDanh input)
        {
            var daCo = await _context.DiemDanh.AnyAsync(d =>
                d.MaLichHoc == input.MaLichHoc &&
                d.MaHocVien == input.MaHocVien);

            if (daCo)
            {
                ModelState.AddModelError(string.Empty, "Học viên này đã có bản ghi điểm danh trong buổi học đã chọn.");
            }

            if (!ModelState.IsValid)
            {
                await NapDanhSachLuaChonAsync(input.MaLichHoc, input.MaHocVien);
                return View(input);
            }

            input.ThoiGianGhiNhan = DateTime.Now;
            input.GhiChu = input.GhiChu?.Trim();

            _context.DiemDanh.Add(input);
            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = "Thêm bản ghi điểm danh thành công.";
            return RedirectToAction(nameof(Index));
        }

        // GET: DiemDanh/Edit/5
        [AdminOnly]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var diemDanh = await _context.DiemDanh
                .Include(d => d.LichHoc)
                    .ThenInclude(l => l!.LopHoc)
                .Include(d => d.HocVien)
                .FirstOrDefaultAsync(d => d.MaDiemDanh == id);

            if (diemDanh == null) return NotFound();

            await NapDanhSachLuaChonAsync(diemDanh.MaLichHoc, diemDanh.MaHocVien);
            return View(diemDanh);
        }

        // POST: DiemDanh/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> Edit(int id, [Bind("MaDiemDanh,MaLichHoc,MaHocVien,TrangThaiDiemDanh,GhiChu")] DiemDanh input)
        {
            if (id != input.MaDiemDanh) return NotFound();

            var diemDanh = await _context.DiemDanh.FindAsync(id);
            if (diemDanh == null) return NotFound();

            var daCo = await _context.DiemDanh.AnyAsync(d =>
                d.MaDiemDanh != id &&
                d.MaLichHoc == input.MaLichHoc &&
                d.MaHocVien == input.MaHocVien);

            if (daCo)
            {
                ModelState.AddModelError(string.Empty, "Học viên này đã có bản ghi điểm danh trong buổi học này.");
            }

            if (!ModelState.IsValid)
            {
                await NapDanhSachLuaChonAsync(input.MaLichHoc, input.MaHocVien);
                return View(input);
            }

            diemDanh.MaLichHoc = input.MaLichHoc;
            diemDanh.MaHocVien = input.MaHocVien;
            diemDanh.TrangThaiDiemDanh = input.TrangThaiDiemDanh;
            diemDanh.GhiChu = input.GhiChu?.Trim();
            diemDanh.ThoiGianGhiNhan = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = "Cập nhật điểm danh thành công.";
            return RedirectToAction(nameof(Index));
        }

        // GET: DiemDanh/Delete/5
        [AdminOnly]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var diemDanh = await _context.DiemDanh
                .Include(d => d.LichHoc)
                    .ThenInclude(l => l!.LopHoc)
                .Include(d => d.HocVien)
                .FirstOrDefaultAsync(d => d.MaDiemDanh == id);

            if (diemDanh == null) return NotFound();

            return View(diemDanh);
        }

        // POST: DiemDanh/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var diemDanh = await _context.DiemDanh.FindAsync(id);
            if (diemDanh == null) return NotFound();

            _context.DiemDanh.Remove(diemDanh);
            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = "Xóa điểm danh thành công.";
            return RedirectToAction(nameof(Index));
        }

        private async Task NapDanhSachLuaChonAsync(int? selectedMaLichHoc = null, int? selectedMaHocVien = null)
        {
            var lichHocs = await _context.LichHoc
                .AsNoTracking()
                .Include(l => l.LopHoc)
                .OrderByDescending(l => l.ThoiGianBatDau)
                .Select(l => new
                {
                    l.MaLichHoc,
                    TenHienThi = $"{l.LopHoc!.TenLop} ({l.ThoiGianBatDau:dd/MM/yyyy HH:mm})"
                })
                .ToListAsync();

            var hocViens = await _context.HocVien
                .AsNoTracking()
                .Where(h => h.TrangThai == TrangThaiChung.HoatDong || h.MaHocVien == selectedMaHocVien)
                .OrderBy(h => h.HoTen)
                .Select(h => new
                {
                    h.MaHocVien,
                    TenHienThi = h.HoTen + (!string.IsNullOrEmpty(h.SoDienThoai) ? $" - {h.SoDienThoai}" : "")
                })
                .ToListAsync();

            ViewBag.LichHocList = new SelectList(lichHocs, "MaLichHoc", "TenHienThi", selectedMaLichHoc);
            ViewBag.HocVienList = new SelectList(hocViens, "MaHocVien", "TenHienThi", selectedMaHocVien);
        }
    }
}
