// Họ và tên: Nguyễn Đức Hùng
// Mã sinh viên: 23103100212
// Nội dung thực hiện: CRUD, tìm kiếm, lọc, sắp xếp và phân trang gia sư (SV2).
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Data;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Filters;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.ViewModels;

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Controllers;

[AdminOnly]
public class GiaSuController : Controller
{
    private const int PageSize = 10;
    private readonly AppDbContext _context;

    public GiaSuController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? tuKhoa,
        int? monHocId,
        string? trinhDo,
        int? kinhNghiem,
        string? trangThai,
        string? sapXep,
        int page = 1)
    {
        var truyVan = _context.GiaSu
            .AsNoTracking()
            .Include(g => g.ChuyenMons)
                .ThenInclude(c => c.MonHoc)
            .AsQueryable();

        tuKhoa = tuKhoa?.Trim();
        trinhDo = trinhDo?.Trim();
        trangThai = trangThai?.Trim();

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            truyVan = truyVan.Where(g =>
                g.HoTen.Contains(tuKhoa) ||
                g.SoDienThoai.Contains(tuKhoa) ||
                (g.Email != null && g.Email.Contains(tuKhoa)) ||
                g.ChuyenMons.Any(c => c.MonHoc != null && c.MonHoc.TenMonHoc.Contains(tuKhoa)));
        }

        if (monHocId.HasValue && monHocId.Value > 0)
        {
            truyVan = truyVan.Where(g => g.ChuyenMons.Any(c => c.MaMonHoc == monHocId.Value));
        }

        if (!string.IsNullOrWhiteSpace(trinhDo))
        {
            truyVan = truyVan.Where(g => g.TrinhDo == trinhDo);
        }

        if (kinhNghiem.HasValue && kinhNghiem.Value >= 0)
        {
            truyVan = truyVan.Where(g => g.KinhNghiem >= kinhNghiem.Value);
        }

        if (!string.IsNullOrWhiteSpace(trangThai))
        {
            truyVan = truyVan.Where(g => g.TrangThai == trangThai);
        }

        truyVan = (sapXep ?? "ten_tang").ToLowerInvariant() switch
        {
            "ten_giam" => truyVan.OrderByDescending(g => g.HoTen).ThenBy(g => g.MaGiaSu),
            "kn_giam" => truyVan.OrderByDescending(g => g.KinhNghiem).ThenBy(g => g.HoTen),
            "kn_tang" => truyVan.OrderBy(g => g.KinhNghiem).ThenBy(g => g.HoTen),
            _ => truyVan.OrderBy(g => g.HoTen).ThenBy(g => g.MaGiaSu)
        };

        var tongSoBanGhi = await truyVan.CountAsync();
        var tongSoTrang = (int)Math.Ceiling((double)tongSoBanGhi / PageSize);
        page = Math.Clamp(page, 1, Math.Max(tongSoTrang, 1));

        var danhSach = await truyVan
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        ViewBag.TuKhoa = tuKhoa;
        ViewBag.TrinhDo = trinhDo;
        ViewBag.KinhNghiem = kinhNghiem;
        ViewBag.TrangThai = trangThai;
        ViewBag.SapXep = string.IsNullOrWhiteSpace(sapXep) ? "ten_tang" : sapXep;
        ViewBag.MonHocList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
            await _context.MonHoc.AsNoTracking().OrderBy(m => m.TenMonHoc).ToListAsync(),
            "MaMonHoc",
            "TenMonHoc",
            monHocId);
        ViewBag.PhanTrang = new PhanTrangViewModel
        {
            Page = page,
            PageSize = PageSize,
            TotalItems = tongSoBanGhi
        };

        return View(danhSach);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var giaSu = await _context.GiaSu
            .AsNoTracking()
            .Include(g => g.ChuyenMons)
                .ThenInclude(c => c.MonHoc)
            .FirstOrDefaultAsync(g => g.MaGiaSu == id);

        return giaSu == null ? NotFound() : View(giaSu);
    }

    [HttpGet]
    public IActionResult Create() => View(new GiaSu());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("HoTen,SoDienThoai,Email,DiaChi,TrinhDo,KinhNghiem,TrangThai")] GiaSu giaSu)
    {
        ValidateTrangThai(giaSu);
        if (!ModelState.IsValid) return View(giaSu);

        Normalize(giaSu);
        _context.GiaSu.Add(giaSu);
        await _context.SaveChangesAsync();

        TempData["ThanhCong"] = "Thêm gia sư thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var giaSu = await _context.GiaSu.FindAsync(id);
        return giaSu == null ? NotFound() : View(giaSu);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("MaGiaSu,HoTen,SoDienThoai,Email,DiaChi,TrinhDo,KinhNghiem,TrangThai")] GiaSu input)
    {
        if (id != input.MaGiaSu) return NotFound();
        ValidateTrangThai(input);
        if (!ModelState.IsValid) return View(input);

        var giaSu = await _context.GiaSu.FirstOrDefaultAsync(g => g.MaGiaSu == id);
        if (giaSu == null) return NotFound();

        giaSu.HoTen = input.HoTen.Trim();
        giaSu.SoDienThoai = input.SoDienThoai.Trim();
        giaSu.Email = input.Email?.Trim();
        giaSu.DiaChi = input.DiaChi?.Trim();
        giaSu.TrinhDo = input.TrinhDo?.Trim();
        giaSu.KinhNghiem = input.KinhNghiem;
        giaSu.TrangThai = input.TrangThai.Trim();

        try
        {
            await _context.SaveChangesAsync();
            TempData["ThanhCong"] = "Cập nhật gia sư thành công.";
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.GiaSu.AnyAsync(g => g.MaGiaSu == id)) return NotFound();
            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var giaSu = await _context.GiaSu
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.MaGiaSu == id);
        if (giaSu == null) return NotFound();

        ViewBag.CoLichSuPhanCong = await _context.PhanCongGiaSu.AnyAsync(p => p.MaGiaSu == id);
        ViewBag.CoLichHoc = await _context.LichHoc.AnyAsync(l => l.MaGiaSu == id);
        return View(giaSu);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var giaSu = await _context.GiaSu.FirstOrDefaultAsync(g => g.MaGiaSu == id);
        if (giaSu == null) return NotFound();

        var coLichSuPhanCong = await _context.PhanCongGiaSu.AnyAsync(p => p.MaGiaSu == id);
        var coLichHoc = await _context.LichHoc.AnyAsync(l => l.MaGiaSu == id);
        if (coLichSuPhanCong || coLichHoc)
        {
            TempData["Loi"] = "Không thể xóa gia sư đã có lịch sử phân công hoặc lịch học. Hãy chuyển trạng thái sang Khóa.";
            return RedirectToAction(nameof(Index));
        }

        // Quan hệ được cấu hình Restrict trong DbContext, nên xóa các chuyên môn trước.
        var chuyenMons = await _context.ChuyenMonGiaSu
            .Where(c => c.MaGiaSu == id)
            .ToListAsync();
        _context.ChuyenMonGiaSu.RemoveRange(chuyenMons);
        _context.GiaSu.Remove(giaSu);

        try
        {
            await _context.SaveChangesAsync();
            TempData["ThanhCong"] = "Xóa gia sư thành công.";
        }
        catch (DbUpdateException)
        {
            TempData["Loi"] = "Không thể xóa gia sư vì dữ liệu khác trong hệ thống đang tham chiếu đến gia sư này.";
        }

        return RedirectToAction(nameof(Index));
    }

    private static void Normalize(GiaSu giaSu)
    {
        giaSu.HoTen = giaSu.HoTen.Trim();
        giaSu.SoDienThoai = giaSu.SoDienThoai.Trim();
        giaSu.Email = giaSu.Email?.Trim();
        giaSu.DiaChi = giaSu.DiaChi?.Trim();
        giaSu.TrinhDo = giaSu.TrinhDo?.Trim();
        giaSu.TrangThai = giaSu.TrangThai.Trim();
    }

    private void ValidateTrangThai(GiaSu giaSu)
    {
        giaSu.TrangThai = giaSu.TrangThai?.Trim() ?? string.Empty;
        var trangThaiHopLe = new[] { TrangThaiChung.HoatDong, TrangThaiChung.Khoa };
        if (!trangThaiHopLe.Contains(giaSu.TrangThai, StringComparer.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(giaSu.TrangThai), "Vui lòng chọn trạng thái hợp lệ.");
        }
    }
}
