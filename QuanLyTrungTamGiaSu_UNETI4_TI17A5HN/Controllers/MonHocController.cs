using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Data;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Filters;
namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Controllers
{
    public class MonHocController : Controller
    {
        private readonly AppDbContext _context;

        public MonHocController(AppDbContext context)
        {
            _context = context;
        }

        // GET: MonHoc
        public async Task<IActionResult> Index()
        {
            var danhSach = await _context.MonHoc
                .AsNoTracking()
                .OrderBy(m => m.TenMonHoc)
                .ToListAsync();

            return View(danhSach);
        }

        // GET: MonHoc/Details/5
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
        public IActionResult Create()
        {
            return View();
        }

        // POST: MonHoc/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MonHoc monHoc)
        {
            if (!ModelState.IsValid)
                return View(monHoc);

            _context.MonHoc.Add(monHoc);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: MonHoc/Edit/5
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
        public async Task<IActionResult> Edit(
            int id, MonHoc monHoc)
        {
            if (id != monHoc.MaMonHoc)
                return NotFound();

            if (!ModelState.IsValid)
                return View(monHoc);

            try
            {
                _context.Update(monHoc);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.MonHoc
                    .AnyAsync(m => m.MaMonHoc == id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: MonHoc/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

        // POST: MonHoc/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var monHoc = await _context.MonHoc.FindAsync(id);

            if (monHoc != null)
            {
                _context.MonHoc.Remove(monHoc);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        [AdminOnly]
public async Task<IActionResult> Index(string? tuKhoa)
        {
            var truyVan = _context.MonHoc.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();

                truyVan = truyVan.Where(m =>
                    m.TenMonHoc.Contains(tuKhoa) ||
                    (m.CapDo != null && m.CapDo.Contains(tuKhoa)) ||
                    (m.MoTa != null && m.MoTa.Contains(tuKhoa)));
            }

            ViewBag.TuKhoa = tuKhoa;

            var danhSach = await truyVan
                .OrderBy(m => m.TenMonHoc)
                .ToListAsync();

            return View(danhSach);
        }



    }
}