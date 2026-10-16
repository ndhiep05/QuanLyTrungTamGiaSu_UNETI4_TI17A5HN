// Họ và tên: Nguyễn Đình Hiệp
// Mã sinh viên: 23103100230
// Nội dung thực hiện: Controller quản lý Thanh toán Học phí (MODULE 5 - SV1).

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Data;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.ViewModels;

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Controllers;

public class ThanhToanHocPhiController : Controller
{
    private readonly AppDbContext _context;

    public ThanhToanHocPhiController(AppDbContext context)
    {
        _context = context;
    }

    // GET: ThanhToanHocPhi
    public async Task<IActionResult> Index(string? tuKhoa, int page = 1)
    {
        var vaiTro = HttpContext.Session.GetString("VaiTro");
        var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");

        if (string.IsNullOrEmpty(vaiTro))
        {
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        var query = _context.ThanhToanHocPhi
            .Include(t => t.DangKyLop)
                .ThenInclude(d => d!.HocVien)
            .Include(t => t.DangKyLop)
                .ThenInclude(d => d!.LopHoc)
            .AsQueryable();

        // Học viên chỉ xem thanh toán của chính mình
        if (vaiTro == VaiTro.HocVien)
        {
            var hocVien = await _context.HocVien.FirstOrDefaultAsync(h => h.MaTaiKhoan == maTaiKhoan);
            if (hocVien != null)
            {
                query = query.Where(t => t.DangKyLop != null && t.DangKyLop.MaHocVien == hocVien.MaHocVien);
            }
            else
            {
                query = query.Where(t => false);
            }
        }

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            tuKhoa = tuKhoa.Trim();
            query = query.Where(t =>
                (t.DangKyLop != null && t.DangKyLop.HocVien != null && t.DangKyLop.HocVien.HoTen.Contains(tuKhoa)) ||
                (t.DangKyLop != null && t.DangKyLop.LopHoc != null && t.DangKyLop.LopHoc.TenLop.Contains(tuKhoa)) ||
                t.PhuongThuc.Contains(tuKhoa));
        }

        query = query.OrderByDescending(t => t.NgayThanhToan);

        int pageSize = 15;
        int totalItems = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        ViewBag.TuKhoa = tuKhoa;
        ViewBag.PhanTrang = new PhanTrangViewModel
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };

        return View(items);
    }

    // GET: ThanhToanHocPhi/Create
    public async Task<IActionResult> Create(int? maDangKy)
    {
        var vaiTro = HttpContext.Session.GetString("VaiTro");
        if (vaiTro != VaiTro.Admin)
        {
            return RedirectToAction("KhongCoQuyen", "Home");
        }

        await PopulateDangKySelectListAsync(maDangKy);
        return View(new ThanhToanHocPhi { NgayThanhToan = DateTime.Now, SoTien = 0, PhuongThuc = "Tiền mặt" });
    }

    // POST: ThanhToanHocPhi/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ThanhToanHocPhi model)
    {
        var vaiTro = HttpContext.Session.GetString("VaiTro");
        if (vaiTro != VaiTro.Admin)
        {
            return RedirectToAction("KhongCoQuyen", "Home");
        }

        var dangKy = await _context.DangKyLop
            .Include(d => d.LopHoc)
            .Include(d => d.ThanhToans)
            .FirstOrDefaultAsync(d => d.MaDangKy == model.MaDangKy);

        if (dangKy == null || dangKy.LopHoc == null)
        {
            ModelState.AddModelError("MaDangKy", "Vui lòng chọn một lượt đăng ký lớp hợp lệ.");
        }
        else
        {
            // 9.2. Kiểm tra học phí: SoTienConLai = HocPhi - TongSoTienDaThanhToan
            decimal tongDaThanhToan = dangKy.ThanhToans.Sum(t => t.SoTien);
            decimal hocPhi = dangKy.LopHoc.HocPhi;
            decimal soTienConLai = hocPhi - tongDaThanhToan;

            if (model.SoTien <= 0)
            {
                ModelState.AddModelError("SoTien", "Số tiền thanh toán mới phải lớn hơn 0.");
            }
            else if (model.SoTien > soTienConLai)
            {
                ModelState.AddModelError("SoTien", $"Số tiền thanh toán ({model.SoTien:N0} ₫) vượt quá số tiền còn lại ({soTienConLai:N0} ₫) của đăng ký này.");
            }
        }

        if (ModelState.IsValid)
        {
            model.NgayThanhToan = DateTime.Now;
            model.NguoiGhiNhan = HttpContext.Session.GetInt32("MaTaiKhoan");

            _context.ThanhToanHocPhi.Add(model);
            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = "Thêm thanh toán học phí thành công!";
            return RedirectToAction(nameof(Index));
        }

        await PopulateDangKySelectListAsync(model.MaDangKy);
        return View(model);
    }

    // GET: ThanhToanHocPhi/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var vaiTro = HttpContext.Session.GetString("VaiTro");
        if (vaiTro != VaiTro.Admin)
        {
            return RedirectToAction("KhongCoQuyen", "Home");
        }

        var item = await _context.ThanhToanHocPhi.FindAsync(id);
        if (item == null)
        {
            return NotFound();
        }

        await PopulateDangKySelectListAsync(item.MaDangKy);
        return View(item);
    }

    // POST: ThanhToanHocPhi/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ThanhToanHocPhi model)
    {
        var vaiTro = HttpContext.Session.GetString("VaiTro");
        if (vaiTro != VaiTro.Admin)
        {
            return RedirectToAction("KhongCoQuyen", "Home");
        }

        if (id != model.MaThanhToan)
        {
            return NotFound();
        }

        var existing = await _context.ThanhToanHocPhi
            .Include(t => t.DangKyLop)
                .ThenInclude(d => d!.LopHoc)
            .Include(t => t.DangKyLop)
                .ThenInclude(d => d!.ThanhToans)
            .FirstOrDefaultAsync(t => t.MaThanhToan == id);

        if (existing == null)
        {
            return NotFound();
        }

        var dangKy = existing.DangKyLop;
        if (dangKy == null || dangKy.LopHoc == null)
        {
            ModelState.AddModelError("MaDangKy", "Đăng ký lớp không hợp lệ.");
        }
        else
        {
            // Tính số tiền đã đóng của đăng ký này trừ bản ghi hiện tại đang sửa
            decimal tongDaThanhToanKhac = dangKy.ThanhToans
                .Where(t => t.MaThanhToan != id)
                .Sum(t => t.SoTien);

            decimal soTienConLai = dangKy.LopHoc.HocPhi - tongDaThanhToanKhac;

            if (model.SoTien <= 0)
            {
                ModelState.AddModelError("SoTien", "Số tiền thanh toán mới phải lớn hơn 0.");
            }
            else if (model.SoTien > soTienConLai)
            {
                ModelState.AddModelError("SoTien", $"Số tiền thanh toán ({model.SoTien:N0} ₫) vượt quá số tiền còn lại ({soTienConLai:N0} ₫).");
            }
        }

        if (ModelState.IsValid)
        {
            existing.MaDangKy = model.MaDangKy;
            existing.SoTien = model.SoTien;
            existing.PhuongThuc = model.PhuongThuc;
            existing.GhiChu = model.GhiChu;

            await _context.SaveChangesAsync();
            TempData["ThanhCong"] = "Cập nhật thanh toán học phí thành công!";
            return RedirectToAction(nameof(Index));
        }

        await PopulateDangKySelectListAsync(model.MaDangKy);
        return View(model);
    }

    // GET: ThanhToanHocPhi/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var vaiTro = HttpContext.Session.GetString("VaiTro");
        if (vaiTro != VaiTro.Admin)
        {
            return RedirectToAction("KhongCoQuyen", "Home");
        }

        var item = await _context.ThanhToanHocPhi
            .Include(t => t.DangKyLop)
                .ThenInclude(d => d!.HocVien)
            .Include(t => t.DangKyLop)
                .ThenInclude(d => d!.LopHoc)
            .FirstOrDefaultAsync(t => t.MaThanhToan == id);

        if (item == null)
        {
            return NotFound();
        }

        return View(item);
    }

    // POST: ThanhToanHocPhi/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var vaiTro = HttpContext.Session.GetString("VaiTro");
        if (vaiTro != VaiTro.Admin)
        {
            return RedirectToAction("KhongCoQuyen", "Home");
        }

        var item = await _context.ThanhToanHocPhi.FindAsync(id);
        if (item != null)
        {
            _context.ThanhToanHocPhi.Remove(item);
            await _context.SaveChangesAsync();
            TempData["ThanhCong"] = "Xóa bản ghi thanh toán học phí thành công!";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDangKySelectListAsync(int? selectedId = null)
    {
        var list = await _context.DangKyLop
            .Include(d => d.HocVien)
            .Include(d => d.LopHoc)
            .Include(d => d.ThanhToans)
            .Where(d => d.TrangThai != TrangThaiDangKy.DaHuy)
            .ToListAsync();

        var selectList = list.Select(d =>
        {
            decimal hocPhi = d.LopHoc?.HocPhi ?? 0m;
            decimal daThu = d.ThanhToans.Sum(t => t.SoTien);
            decimal conLai = Math.Max(0, hocPhi - daThu);
            return new
            {
                MaDangKy = d.MaDangKy,
                MoTa = $"{d.HocVien?.HoTen ?? "Học viên"} - {d.LopHoc?.TenLop ?? "Lớp"} (Học phí: {hocPhi:N0} ₫ | Còn nợ: {conLai:N0} ₫)"
            };
        });

        ViewBag.DangKyList = new SelectList(selectList, "MaDangKy", "MoTa", selectedId);
    }
}
