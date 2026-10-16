// Họ và tên: Nguyễn Đức Hùng
// Mã sinh viên: 23103100212
// Nội dung thực hiện: CRUD và tìm kiếm chuyên môn gia sư, ngăn bản ghi đang hiệu lực bị trùng (SV2).
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Data;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Filters;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.ViewModels;

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Controllers;

public class ChuyenMonGiaSuController : Controller
{
    private const int PageSize = 10;
    private readonly AppDbContext _context;

    public ChuyenMonGiaSuController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? tuKhoa, int page = 1)
    {
        var vaiTro = HttpContext.Session.GetString("VaiTro");
        if (vaiTro != VaiTro.Admin && vaiTro != VaiTro.GiaSu)
        {
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        var truyVan = _context.ChuyenMonGiaSu
            .AsNoTracking()
            .Include(c => c.GiaSu)
            .Include(c => c.MonHoc)
            .AsQueryable();

        if (vaiTro == VaiTro.GiaSu)
        {
            var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (!maTaiKhoan.HasValue) return RedirectToAction("DangNhap", "TaiKhoan");
            truyVan = truyVan.Where(c => c.GiaSu != null && c.GiaSu.MaTaiKhoan == maTaiKhoan.Value);
        }

        tuKhoa = tuKhoa?.Trim();
        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            truyVan = truyVan.Where(c =>
                (c.GiaSu != null &&
                    (c.GiaSu.HoTen.Contains(tuKhoa) ||
                     c.GiaSu.SoDienThoai.Contains(tuKhoa) ||
                     (c.GiaSu.Email != null && c.GiaSu.Email.Contains(tuKhoa)))) ||
                (c.MonHoc != null && c.MonHoc.TenMonHoc.Contains(tuKhoa)));
        }

        var tongSoBanGhi = await truyVan.CountAsync();
        var tongSoTrang = (int)Math.Ceiling((double)tongSoBanGhi / PageSize);
        page = Math.Clamp(page, 1, Math.Max(tongSoTrang, 1));

        var danhSach = await truyVan
            .OrderBy(c => c.GiaSu!.HoTen)
            .ThenBy(c => c.MonHoc!.TenMonHoc)
            .ThenBy(c => c.ChuyenMonGiaSuId)
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

    [HttpGet]
    [AdminOnly]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var chuyenMon = await _context.ChuyenMonGiaSu
            .AsNoTracking()
            .Include(c => c.GiaSu)
            .Include(c => c.MonHoc)
            .FirstOrDefaultAsync(c => c.ChuyenMonGiaSuId == id);

        return chuyenMon == null ? NotFound() : View(chuyenMon);
    }

    [HttpGet]
    [AdminOnly]
    public async Task<IActionResult> Create()
    {
        await NapDanhSachLuaChonAsync();
        return View(new ChuyenMonGiaSu());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AdminOnly]
    public async Task<IActionResult> Create(
        [Bind("MaGiaSu,MaMonHoc,TrangThai")] ChuyenMonGiaSu chuyenMon)
    {
        await ValidateChuyenMonAsync(chuyenMon, excludeId: null);
        if (!ModelState.IsValid)
        {
            await NapDanhSachLuaChonAsync(chuyenMon.MaGiaSu, chuyenMon.MaMonHoc);
            return View(chuyenMon);
        }

        chuyenMon.TrangThai = chuyenMon.TrangThai.Trim();
        _context.ChuyenMonGiaSu.Add(chuyenMon);
        await _context.SaveChangesAsync();

        TempData["ThanhCong"] = "Thêm chuyên môn gia sư thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [AdminOnly]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var chuyenMon = await _context.ChuyenMonGiaSu.FindAsync(id);
        if (chuyenMon == null) return NotFound();

        await NapDanhSachLuaChonAsync(chuyenMon.MaGiaSu, chuyenMon.MaMonHoc);
        return View(chuyenMon);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AdminOnly]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("ChuyenMonGiaSuId,MaGiaSu,MaMonHoc,TrangThai")] ChuyenMonGiaSu input)
    {
        if (id != input.ChuyenMonGiaSuId) return NotFound();

        await ValidateChuyenMonAsync(input, id);
        if (!ModelState.IsValid)
        {
            await NapDanhSachLuaChonAsync(input.MaGiaSu, input.MaMonHoc);
            return View(input);
        }

        var chuyenMon = await _context.ChuyenMonGiaSu
            .FirstOrDefaultAsync(c => c.ChuyenMonGiaSuId == id);
        if (chuyenMon == null) return NotFound();

        chuyenMon.MaGiaSu = input.MaGiaSu;
        chuyenMon.MaMonHoc = input.MaMonHoc;
        chuyenMon.TrangThai = input.TrangThai.Trim();

        try
        {
            await _context.SaveChangesAsync();
            TempData["ThanhCong"] = "Cập nhật chuyên môn gia sư thành công.";
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.ChuyenMonGiaSu.AnyAsync(c => c.ChuyenMonGiaSuId == id)) return NotFound();
            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [AdminOnly]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var chuyenMon = await _context.ChuyenMonGiaSu
            .AsNoTracking()
            .Include(c => c.GiaSu)
            .Include(c => c.MonHoc)
            .FirstOrDefaultAsync(c => c.ChuyenMonGiaSuId == id);

        return chuyenMon == null ? NotFound() : View(chuyenMon);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [AdminOnly]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var chuyenMon = await _context.ChuyenMonGiaSu.FindAsync(id);
        if (chuyenMon == null) return NotFound();

        _context.ChuyenMonGiaSu.Remove(chuyenMon);
        await _context.SaveChangesAsync();
        TempData["ThanhCong"] = "Xóa chuyên môn gia sư thành công.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateChuyenMonAsync(ChuyenMonGiaSu chuyenMon, int? excludeId)
    {
        if (chuyenMon.MaGiaSu <= 0 ||
            !await _context.GiaSu.AnyAsync(g => g.MaGiaSu == chuyenMon.MaGiaSu))
        {
            ModelState.AddModelError(nameof(chuyenMon.MaGiaSu), "Vui lòng chọn gia sư hợp lệ.");
        }

        if (chuyenMon.MaMonHoc <= 0 ||
            !await _context.MonHoc.AnyAsync(m => m.MaMonHoc == chuyenMon.MaMonHoc))
        {
            ModelState.AddModelError(nameof(chuyenMon.MaMonHoc), "Vui lòng chọn môn học hợp lệ.");
        }

        chuyenMon.TrangThai = chuyenMon.TrangThai?.Trim() ?? string.Empty;
        var trangThaiHopLe = new[] { TrangThaiChung.HoatDong, TrangThaiChung.Khoa };
        if (!trangThaiHopLe.Contains(chuyenMon.TrangThai, StringComparer.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(chuyenMon.TrangThai), "Trạng thái không hợp lệ.");
        }

        if (IsActive(chuyenMon.TrangThai) &&
            await _context.ChuyenMonGiaSu.AnyAsync(c =>
                c.ChuyenMonGiaSuId != excludeId &&
                c.MaGiaSu == chuyenMon.MaGiaSu &&
                c.MaMonHoc == chuyenMon.MaMonHoc &&
                (c.TrangThai == TrangThaiChung.HoatDong || c.TrangThai == "HoatDong")))
        {
            ModelState.AddModelError(string.Empty, "Gia sư đã có chuyên môn đang hoạt động cho môn học này.");
        }
    }

    private async Task NapDanhSachLuaChonAsync(int? giaSuId = null, int? monHocId = null)
    {
        var giaSus = _context.GiaSu.AsNoTracking()
            .Where(g => g.TrangThai == TrangThaiChung.HoatDong || g.MaGiaSu == giaSuId)
            .OrderBy(g => g.HoTen);
        var monHocs = _context.MonHoc.AsNoTracking()
            .Where(m => m.TrangThai == TrangThaiChung.HoatDong || m.MaMonHoc == monHocId)
            .OrderBy(m => m.TenMonHoc);

        ViewBag.GiaSuList = new SelectList(await giaSus.ToListAsync(), "MaGiaSu", "HoTen", giaSuId);
        ViewBag.MonHocList = new SelectList(await monHocs.ToListAsync(), "MaMonHoc", "TenMonHoc", monHocId);
    }

    private static bool IsActive(string? trangThai) =>
        string.Equals(trangThai?.Trim(), TrangThaiChung.HoatDong, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(trangThai?.Trim(), "HoatDong", StringComparison.OrdinalIgnoreCase);
}
