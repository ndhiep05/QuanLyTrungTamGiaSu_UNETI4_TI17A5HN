using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Data;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.ViewModels;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Filters;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Services;

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ExcelExportService _excelService;

        public TaiKhoanController(AppDbContext context, ExcelExportService? excelService = null)
        {
            _context = context;
            _excelService = excelService ?? new ExcelExportService();
        }

        [HttpGet]
        public IActionResult DangNhap(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new DangNhapViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangNhap(DangNhapViewModel model, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var tenDangNhap = model.TenDangNhap.Trim();

            var taiKhoan = await _context.TaiKhoan
                .FirstOrDefaultAsync(t => t.TenDangNhap == tenDangNhap);

            if (taiKhoan == null || taiKhoan.MatKhau != model.MatKhau)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Tên đăng nhập hoặc mật khẩu không đúng.");

                return View(model);
            }

            // Kiểm tra trạng thái tài khoản (hỗ trợ cả HoatDong và Hoạt động)
            var trangThai = taiKhoan.TrangThai?.Trim() ?? "";
            if (!trangThai.Equals("HoatDong", StringComparison.OrdinalIgnoreCase) &&
                !trangThai.Equals(TrangThaiChung.HoatDong, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Tài khoản hiện đang bị khóa hoặc đã ngưng hoạt động.");

                return View(model);
            }

            HttpContext.Session.SetString("VaiTro", taiKhoan.VaiTro);
            HttpContext.Session.SetString("HoTen", taiKhoan.HoTen);
            HttpContext.Session.SetInt32("MaTaiKhoan", taiKhoan.MaTaiKhoan);
            HttpContext.Session.SetString("TenDangNhap", taiKhoan.TenDangNhap);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult DangXuat()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // Action chuyển tiếp hỗ trợ tương thích nếu có link gọi Login hoặc AccessDenied
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            return RedirectToAction(nameof(DangNhap), new { returnUrl });
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return RedirectToAction("KhongCoQuyen", "Home");
        }

        [HttpGet]
        [AdminOnly]
        public async Task<IActionResult> Index(
            string? tuKhoa,
            string? vaiTro,
            string? trangThai,
            string? sapXep,
            int page = 1)
        {
            const int kichThuocTrang = 10;
            var truyVan = _context.TaiKhoan.AsNoTracking();

            tuKhoa = tuKhoa?.Trim();
            vaiTro = vaiTro?.Trim();
            trangThai = trangThai?.Trim();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                truyVan = truyVan.Where(t =>
                    t.TenDangNhap.Contains(tuKhoa) ||
                    t.HoTen.Contains(tuKhoa) ||
                    t.Email.Contains(tuKhoa));
            }

            if (!string.IsNullOrWhiteSpace(vaiTro))
            {
                truyVan = truyVan.Where(t => t.VaiTro == vaiTro);
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                if (trangThai.Equals("HoatDong", StringComparison.OrdinalIgnoreCase) || trangThai.Equals("Hoạt động", StringComparison.OrdinalIgnoreCase))
                {
                    truyVan = truyVan.Where(t => t.TrangThai == "HoatDong" || t.TrangThai == "Hoạt động");
                }
                else if (trangThai.Equals("Khoa", StringComparison.OrdinalIgnoreCase) || trangThai.Equals("Khóa", StringComparison.OrdinalIgnoreCase))
                {
                    truyVan = truyVan.Where(t => t.TrangThai == "Khoa" || t.TrangThai == "Khóa");
                }
                else
                {
                    truyVan = truyVan.Where(t => t.TrangThai == trangThai);
                }
            }

            truyVan = (sapXep ?? "id_giam").ToLowerInvariant() switch
            {
                "id_tang" => truyVan.OrderBy(t => t.MaTaiKhoan),
                "ten_tang" => truyVan.OrderBy(t => t.TenDangNhap).ThenBy(t => t.MaTaiKhoan),
                "ten_giam" => truyVan.OrderByDescending(t => t.TenDangNhap).ThenBy(t => t.MaTaiKhoan),
                "hoten_tang" => truyVan.OrderBy(t => t.HoTen).ThenBy(t => t.MaTaiKhoan),
                "hoten_giam" => truyVan.OrderByDescending(t => t.HoTen).ThenBy(t => t.MaTaiKhoan),
                _ => truyVan.OrderByDescending(t => t.MaTaiKhoan)
            };

            var tongSoBanGhi = await truyVan.CountAsync();
            var tongSoTrang = (int)Math.Ceiling((double)tongSoBanGhi / kichThuocTrang);
            page = Math.Clamp(page, 1, Math.Max(tongSoTrang, 1));

            var danhSach = await truyVan
                .Skip((page - 1) * kichThuocTrang)
                .Take(kichThuocTrang)
                .ToListAsync();

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.VaiTro = vaiTro;
            ViewBag.TrangThai = trangThai;
            ViewBag.SapXep = string.IsNullOrWhiteSpace(sapXep) ? "id_giam" : sapXep;

            ViewBag.PhanTrang = new PhanTrangViewModel
            {
                Page = page,
                PageSize = kichThuocTrang,
                TotalItems = tongSoBanGhi
            };

            return View(danhSach);
        }

        // GET: TaiKhoan/XuatExcel
        [HttpGet]
        [AdminOnly]
        public async Task<IActionResult> XuatExcel(
            string? tuKhoa,
            string? vaiTro,
            string? trangThai,
            string? sapXep)
        {
            var truyVan = _context.TaiKhoan.AsNoTracking();

            tuKhoa = tuKhoa?.Trim();
            vaiTro = vaiTro?.Trim();
            trangThai = trangThai?.Trim();

            var boLocList = new List<string>();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                truyVan = truyVan.Where(t =>
                    t.TenDangNhap.Contains(tuKhoa) ||
                    t.HoTen.Contains(tuKhoa) ||
                    t.Email.Contains(tuKhoa));
                boLocList.Add($"Từ khóa: \"{tuKhoa}\"");
            }

            if (!string.IsNullOrWhiteSpace(vaiTro))
            {
                truyVan = truyVan.Where(t => t.VaiTro == vaiTro);
                var tenVaiTro = vaiTro switch
                {
                    "Admin" => "Quản trị viên",
                    "GiaSu" => "Gia sư",
                    "HocVien" => "Học viên",
                    _ => vaiTro
                };
                boLocList.Add($"Vai trò: {tenVaiTro}");
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                if (trangThai.Equals("HoatDong", StringComparison.OrdinalIgnoreCase) || trangThai.Equals("Hoạt động", StringComparison.OrdinalIgnoreCase))
                {
                    truyVan = truyVan.Where(t => t.TrangThai == "HoatDong" || t.TrangThai == "Hoạt động");
                    boLocList.Add("Trạng thái: Hoạt động");
                }
                else if (trangThai.Equals("Khoa", StringComparison.OrdinalIgnoreCase) || trangThai.Equals("Khóa", StringComparison.OrdinalIgnoreCase))
                {
                    truyVan = truyVan.Where(t => t.TrangThai == "Khoa" || t.TrangThai == "Khóa");
                    boLocList.Add("Trạng thái: Khóa");
                }
                else
                {
                    truyVan = truyVan.Where(t => t.TrangThai == trangThai);
                    boLocList.Add($"Trạng thái: {trangThai}");
                }
            }

            truyVan = (sapXep ?? "id_giam").ToLowerInvariant() switch
            {
                "id_tang" => truyVan.OrderBy(t => t.MaTaiKhoan),
                "ten_tang" => truyVan.OrderBy(t => t.TenDangNhap).ThenBy(t => t.MaTaiKhoan),
                "ten_giam" => truyVan.OrderByDescending(t => t.TenDangNhap).ThenBy(t => t.MaTaiKhoan),
                "hoten_tang" => truyVan.OrderBy(t => t.HoTen).ThenBy(t => t.MaTaiKhoan),
                "hoten_giam" => truyVan.OrderByDescending(t => t.HoTen).ThenBy(t => t.MaTaiKhoan),
                _ => truyVan.OrderByDescending(t => t.MaTaiKhoan)
            };

            var danhSach = await truyVan.ToListAsync();
            var nguoiXuat = HttpContext.Session.GetString("HoTen") ?? "Quản trị viên";
            var thongTinBoLoc = boLocList.Any() ? string.Join(", ", boLocList) : "Tất cả";

            var fileBytes = _excelService.XuatDanhSachTaiKhoan(danhSach, nguoiXuat, thongTinBoLoc);
            var fileName = $"DanhSachTaiKhoan_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet]
        [AdminOnly]
        public IActionResult Create()
        {
            var taiKhoan = new TaiKhoan
            {
                VaiTro = "HocVien",
                TrangThai = "HoatDong"
            };

            return View(taiKhoan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> Create(TaiKhoan taiKhoan)
        {
            // Không nhận dữ liệu điều hướng từ form.
            ModelState.Remove(nameof(TaiKhoan.GiaSu));
            ModelState.Remove(nameof(TaiKhoan.HocVien));

            // Kiểm tra tên đăng nhập đã tồn tại chưa.
            var trungTenDangNhap = await _context.TaiKhoan
                .AnyAsync(t => t.TenDangNhap == taiKhoan.TenDangNhap.Trim());

            if (trungTenDangNhap)
            {
                ModelState.AddModelError(
                    nameof(taiKhoan.TenDangNhap),
                    "Tên đăng nhập đã tồn tại.");
            }

            // Kiểm tra email đã tồn tại chưa.
            var trungEmail = await _context.TaiKhoan
                .AnyAsync(t => t.Email == taiKhoan.Email.Trim());

            if (trungEmail)
            {
                ModelState.AddModelError(
                    nameof(taiKhoan.Email),
                    "Email đã được sử dụng.");
            }

            // Chỉ cho phép các vai trò hợp lệ.
            var vaiTroHopLe = new[] { "Admin", "GiaSu", "HocVien" };

            if (!vaiTroHopLe.Contains(taiKhoan.VaiTro))
            {
                ModelState.AddModelError(
                    nameof(taiKhoan.VaiTro),
                    "Vai trò không hợp lệ.");
            }

            // Chỉ cho phép các trạng thái hợp lệ.
            var trangThaiHopLe = new[] { "HoatDong", "Khoa" };

            if (!trangThaiHopLe.Contains(taiKhoan.TrangThai))
            {
                ModelState.AddModelError(
                    nameof(taiKhoan.TrangThai),
                    "Trạng thái không hợp lệ.");
            }

            if (!ModelState.IsValid)
            {
                taiKhoan.TenDangNhap = taiKhoan.TenDangNhap?.Trim() ?? "";
                taiKhoan.HoTen = taiKhoan.HoTen?.Trim() ?? "";
                taiKhoan.Email = taiKhoan.Email?.Trim() ?? "";

                return View(taiKhoan);
            }

            taiKhoan.TenDangNhap = taiKhoan.TenDangNhap.Trim();
            taiKhoan.HoTen = taiKhoan.HoTen.Trim();
            taiKhoan.Email = taiKhoan.Email.Trim();

            _context.TaiKhoan.Add(taiKhoan);
            await _context.SaveChangesAsync();

            TempData["ThongBao"] = "Thêm tài khoản thành công.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [AdminOnly]
        public async Task<IActionResult> Edit(int id)
        {
            var taiKhoan = await _context.TaiKhoan
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.MaTaiKhoan == id);

            if (taiKhoan == null)
            {
                return NotFound();
            }

            return View(taiKhoan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> Edit(int id, TaiKhoan taiKhoan)
        {
            if (id != taiKhoan.MaTaiKhoan)
                return BadRequest();

            ModelState.Remove(nameof(TaiKhoan.GiaSu));
            ModelState.Remove(nameof(TaiKhoan.HocVien));

            var taiKhoanTrongDb = await _context.TaiKhoan
                .FirstOrDefaultAsync(t => t.MaTaiKhoan == id);

            if (taiKhoanTrongDb == null)
                return NotFound();

            var tenDangNhap = taiKhoan.TenDangNhap?.Trim() ?? "";
            var hoTen = taiKhoan.HoTen?.Trim() ?? "";
            var email = taiKhoan.Email?.Trim() ?? "";

            if (await _context.TaiKhoan.AnyAsync(t =>
                t.MaTaiKhoan != id && t.TenDangNhap == tenDangNhap))
            {
                ModelState.AddModelError(
                    nameof(taiKhoan.TenDangNhap),
                    "Tên đăng nhập đã tồn tại.");
            }

            if (await _context.TaiKhoan.AnyAsync(t =>
                t.MaTaiKhoan != id && t.Email == email))
            {
                ModelState.AddModelError(
                    nameof(taiKhoan.Email),
                    "Email đã được sử dụng.");
            }

            var vaiTroHopLe = new[] { "Admin", "GiaSu", "HocVien" };
            var trangThaiHopLe = new[] { "HoatDong", "Khoa" };

            if (!vaiTroHopLe.Contains(taiKhoan.VaiTro))
            {
                ModelState.AddModelError(
                    nameof(taiKhoan.VaiTro),
                    "Vai trò không hợp lệ.");
            }

            if (!trangThaiHopLe.Contains(taiKhoan.TrangThai))
            {
                ModelState.AddModelError(
                    nameof(taiKhoan.TrangThai),
                    "Trạng thái không hợp lệ.");
            }

            if (!ModelState.IsValid)
                return View(taiKhoan);

            taiKhoanTrongDb.TenDangNhap = tenDangNhap;
            taiKhoanTrongDb.HoTen = hoTen;
            taiKhoanTrongDb.Email = email;
            taiKhoanTrongDb.VaiTro = taiKhoan.VaiTro;
            taiKhoanTrongDb.TrangThai = taiKhoan.TrangThai;

            // Có mật khẩu được gửi lên thì cập nhật; nếu rỗng thì giữ mật khẩu cũ.
            if (!string.IsNullOrWhiteSpace(taiKhoan.MatKhau))
            {
                taiKhoanTrongDb.MatKhau = taiKhoan.MatKhau;
            }

            await _context.SaveChangesAsync();

            TempData["ThongBao"] = "Cập nhật tài khoản thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [AdminOnly]
        public async Task<IActionResult> Delete(int id)
        {
            var taiKhoan = await _context.TaiKhoan
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.MaTaiKhoan == id);

            if (taiKhoan == null)
            {
                return NotFound();
            }

            // Không cho xóa chính tài khoản đang đăng nhập.
            var maDangNhap = HttpContext.Session.GetInt32("MaTaiKhoan");

            if (maDangNhap == id)
            {
                TempData["ThongBao"] =
                    "Bạn không thể xóa tài khoản đang đăng nhập.";

                return RedirectToAction(nameof(Index));
            }

            return View(taiKhoan);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var maDangNhap = HttpContext.Session.GetInt32("MaTaiKhoan");

            if (maDangNhap == id)
            {
                TempData["ThongBao"] =
                    "Bạn không thể xóa tài khoản đang đăng nhập.";

                return RedirectToAction(nameof(Index));
            }

            var taiKhoan = await _context.TaiKhoan
                .FirstOrDefaultAsync(t => t.MaTaiKhoan == id);

            if (taiKhoan == null)
            {
                return NotFound();
            }

            // Chưa xóa tự động vì tài khoản có thể đang được liên kết
            // với hồ sơ GiaSu, HocVien hoặc các bảng nghiệp vụ.
            var coHoSoGiaSu = await _context.GiaSu
                .AnyAsync(g => g.MaTaiKhoan == id);

            var coHoSoHocVien = await _context.HocVien
                .AnyAsync(h => h.MaTaiKhoan == id);

            if (coHoSoGiaSu || coHoSoHocVien)
            {
                TempData["ThongBao"] =
                    "Tài khoản đang liên kết với hồ sơ gia sư hoặc học viên. " +
                    "Hãy khóa tài khoản thay vì xóa.";

                return RedirectToAction(nameof(Index));
            }

            _context.TaiKhoan.Remove(taiKhoan);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["ThongBao"] =
                    "Không thể xóa tài khoản vì còn dữ liệu nghiệp vụ liên quan.";

                return RedirectToAction(nameof(Index));
            }

            TempData["ThongBao"] = "Xóa tài khoản thành công.";

            return RedirectToAction(nameof(Index));
        }
    }
}
