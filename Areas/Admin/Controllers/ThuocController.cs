using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QL_NhaThuoc.Data;
using QL_NhaThuoc.Filters;
using QL_NhaThuoc.Models;

namespace QL_NhaThuoc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminAuthorize]
    public class ThuocController : Controller
    {
        private readonly QL_NhaThuocDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ThuocController(QL_NhaThuocDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Admin/Thuoc - EF + LINQ
        public async Task<IActionResult> Index()
        {
            var danhSach = await _context.THUOC
                .Include(t => t.NhomThuoc)
                .Include(t => t.ThuongHieu)
                .Include(t => t.NuocSanXuat)
                .OrderByDescending(t => t.MaThuoc)
                .ToListAsync();

            return View(danhSach);
        }

        // GET: Admin/Thuoc/Create
        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();
            return View();
        }

        // POST: Admin/Thuoc/Create - EF + LINQ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Thuoc thuoc, IFormFile? hinhAnhFile,
            int[]? ThanhPhanIds, string[]? HamLuongs,
            int[]? TacDungPhuIds, string[]? MucDos,
            int[]? DoiTuongIds)
        {
            if (ModelState.IsValid)
            {
                // Gán ngày tạo
                thuoc.NgayTao = DateTime.Now;
                
                // Xử lý upload hình ảnh
                if (hinhAnhFile != null && hinhAnhFile.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(hinhAnhFile.FileName);
                    var filePath = Path.Combine(_env.WebRootPath, "images", fileName);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await hinhAnhFile.CopyToAsync(stream);
                    }
                    thuoc.HinhAnh = "/images/" + fileName;
                }

                _context.THUOC.Add(thuoc);
                await _context.SaveChangesAsync();

                // Lưu thành phần
                if (ThanhPhanIds != null)
                {
                    for (int i = 0; i < ThanhPhanIds.Length; i++)
                    {
                        if (ThanhPhanIds[i] > 0)
                        {
                            _context.CT_THANH_PHAN.Add(new CT_ThanhPhan
                            {
                                MaThuoc = thuoc.MaThuoc,
                                MaThanhPhan = ThanhPhanIds[i],
                                HamLuong = HamLuongs != null && i < HamLuongs.Length ? HamLuongs[i] : null
                            });
                        }
                    }
                }

                // Lưu tác dụng phụ
                if (TacDungPhuIds != null)
                {
                    for (int i = 0; i < TacDungPhuIds.Length; i++)
                    {
                        if (TacDungPhuIds[i] > 0)
                        {
                            _context.CT_TAC_DUNG_PHU.Add(new CT_TacDungPhu
                            {
                                MaThuoc = thuoc.MaThuoc,
                                MaTacDungPhu = TacDungPhuIds[i],
                                MucDo = MucDos != null && i < MucDos.Length ? MucDos[i] : null
                            });
                        }
                    }
                }

                // Lưu đối tượng sử dụng
                if (DoiTuongIds != null)
                {
                    foreach (var doiTuongId in DoiTuongIds)
                    {
                        _context.CT_DOI_TUONG.Add(new CT_DoiTuong
                        {
                            MaThuoc = thuoc.MaThuoc,
                            MaDoiTuong = doiTuongId
                        });
                    }
                }

                await _context.SaveChangesAsync();

                TempData["ThongBao"] = "Thêm thuốc thành công!";
                return RedirectToAction(nameof(Index));
            }

            await LoadDropdowns();
            return View(thuoc);
        }


        // GET: Admin/Thuoc/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var thuoc = await _context.THUOC
                .Include(t => t.CT_ThanhPhans)
                .Include(t => t.CT_TacDungPhus)
                .Include(t => t.CT_DoiTuongs)
                .FirstOrDefaultAsync(t => t.MaThuoc == id);
                
            if (thuoc == null)
                return NotFound();

            await LoadDropdowns();
            ViewBag.SelectedDoiTuongs = thuoc.CT_DoiTuongs?.Select(ct => ct.MaDoiTuong).ToList() ?? new List<int>();
            return View(thuoc);
        }

        // POST: Admin/Thuoc/Edit/5 - EF + LINQ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Thuoc thuoc, IFormFile? hinhAnhFile,
            int[]? ThanhPhanIds, string[]? HamLuongs,
            int[]? TacDungPhuIds, string[]? MucDos,
            int[]? DoiTuongIds)
        {
            if (id != thuoc.MaThuoc)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Xử lý upload hình ảnh mới
                    if (hinhAnhFile != null && hinhAnhFile.Length > 0)
                    {
                        var fileName = Guid.NewGuid() + Path.GetExtension(hinhAnhFile.FileName);
                        var filePath = Path.Combine(_env.WebRootPath, "images", fileName);
                        
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await hinhAnhFile.CopyToAsync(stream);
                        }
                        thuoc.HinhAnh = "/images/" + fileName;
                    }

                    _context.Update(thuoc);

                    // Xóa dữ liệu cũ
                    var oldThanhPhans = await _context.CT_THANH_PHAN.Where(ct => ct.MaThuoc == id).ToListAsync();
                    _context.CT_THANH_PHAN.RemoveRange(oldThanhPhans);

                    var oldTacDungPhus = await _context.CT_TAC_DUNG_PHU.Where(ct => ct.MaThuoc == id).ToListAsync();
                    _context.CT_TAC_DUNG_PHU.RemoveRange(oldTacDungPhus);

                    var oldDoiTuongs = await _context.CT_DOI_TUONG.Where(ct => ct.MaThuoc == id).ToListAsync();
                    _context.CT_DOI_TUONG.RemoveRange(oldDoiTuongs);

                    // Thêm thành phần mới
                    if (ThanhPhanIds != null)
                    {
                        for (int i = 0; i < ThanhPhanIds.Length; i++)
                        {
                            if (ThanhPhanIds[i] > 0)
                            {
                                _context.CT_THANH_PHAN.Add(new CT_ThanhPhan
                                {
                                    MaThuoc = id,
                                    MaThanhPhan = ThanhPhanIds[i],
                                    HamLuong = HamLuongs != null && i < HamLuongs.Length ? HamLuongs[i] : null
                                });
                            }
                        }
                    }

                    // Thêm tác dụng phụ mới
                    if (TacDungPhuIds != null)
                    {
                        for (int i = 0; i < TacDungPhuIds.Length; i++)
                        {
                            if (TacDungPhuIds[i] > 0)
                            {
                                _context.CT_TAC_DUNG_PHU.Add(new CT_TacDungPhu
                                {
                                    MaThuoc = id,
                                    MaTacDungPhu = TacDungPhuIds[i],
                                    MucDo = MucDos != null && i < MucDos.Length ? MucDos[i] : null
                                });
                            }
                        }
                    }

                    // Thêm đối tượng sử dụng mới
                    if (DoiTuongIds != null)
                    {
                        foreach (var doiTuongId in DoiTuongIds)
                        {
                            _context.CT_DOI_TUONG.Add(new CT_DoiTuong
                            {
                                MaThuoc = id,
                                MaDoiTuong = doiTuongId
                            });
                        }
                    }

                    await _context.SaveChangesAsync();

                    TempData["ThongBao"] = "Cập nhật thuốc thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.THUOC.AnyAsync(t => t.MaThuoc == id))
                        return NotFound();
                    throw;
                }
            }

            await LoadDropdowns();
            return View(thuoc);
        }

        // POST: Admin/Thuoc/Delete/5 - EF + LINQ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var thuoc = await _context.THUOC.FindAsync(id);
            if (thuoc != null)
            {
                _context.THUOC.Remove(thuoc);
                await _context.SaveChangesAsync();
                TempData["ThongBao"] = "Xóa thuốc thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDropdowns()
        {
            ViewBag.NhomThuocs = new SelectList(
                await _context.NHOM_THUOC.OrderBy(n => n.TenNhomThuoc).ToListAsync(),
                "MaNhomThuoc", "TenNhomThuoc");

            ViewBag.ThuongHieus = new SelectList(
                await _context.THUONG_HIEU.OrderBy(th => th.TenThuongHieu).ToListAsync(),
                "MaThuongHieu", "TenThuongHieu");

            ViewBag.NuocSXs = new SelectList(
                await _context.NUOC_SAN_XUAT.OrderBy(n => n.TenNuocSX).ToListAsync(),
                "MaNuocSX", "TenNuocSX");

            // Load thành phần, tác dụng phụ, đối tượng sử dụng
            ViewBag.ThanhPhans = await _context.THANH_PHAN.OrderBy(tp => tp.TenThanhPhan).ToListAsync();
            ViewBag.TacDungPhus = await _context.TAC_DUNG_PHU.OrderBy(tdp => tdp.TenTacDungPhu).ToListAsync();
            ViewBag.DoiTuongs = await _context.DOI_TUONG_SU_DUNG.OrderBy(dt => dt.TenDoiTuong).ToListAsync();
        }
    }
}
