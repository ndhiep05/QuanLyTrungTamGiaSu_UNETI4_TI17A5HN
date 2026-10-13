
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Data;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.ViewModels;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Filters;

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly AppDbContext _context;

        public TaiKhoanController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult DangNhap()
        {
            return View(new DangNhapViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangNhap(DangNhapViewModel model)
        {
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
            Console.WriteLine(
             $"TrangThai=[{taiKhoan.TrangThai}], " +
             $"VaiTro=[{taiKhoan.VaiTro}]");
            if (taiKhoan.TrangThai.Trim() != "HoatDong")
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Tài khoản hiện không hoạt động.");

                return View(model);
            }

            HttpContext.Session.SetString("VaiTro", taiKhoan.VaiTro);
            HttpContext.Session.SetString("HoTen", taiKhoan.HoTen);
            HttpContext.Session.SetInt32("MaTaiKhoan", taiKhoan.MaTaiKhoan);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult DangXuat()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }



        [HttpGet]
        [AdminOnly]
        public async Task<IActionResult> Index(string? tuKhoa, int page = 1)
        {
            const int kichThuocTrang = 10;

            if (page < 1)
            {
                page = 1;
            }

            var truyVan = _context.TaiKhoan.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();

                truyVan = truyVan.Where(t =>
                    t.TenDangNhap.Contains(tuKhoa) ||
                    t.HoTen.Contains(tuKhoa) ||
                    t.Email.Contains(tuKhoa));
            }

            var tongSoBanGhi = await truyVan.CountAsync();

            var tongSoTrang = (int)Math.Ceiling(
                (double)tongSoBanGhi / kichThuocTrang);

            if (tongSoTrang > 0 && page > tongSoTrang)
            {
                page = tongSoTrang;
            }

            var danhSach = await truyVan
                .OrderBy(t => t.MaTaiKhoan)
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


