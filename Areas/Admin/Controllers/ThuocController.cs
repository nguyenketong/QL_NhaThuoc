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

        // GET: Admin/Thuoc - EF + LINQ với tìm kiếm
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.THUOC
                .Include(t => t.NhomThuoc)
                .Include(t => t.ThuongHieu)
                .Include(t => t.NuocSanXuat)
                .AsQueryable();

            // Lọc theo từ khóa tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => 
                    t.TenThuoc.Contains(search) ||
                    (t.MoTa != null && t.MoTa.Contains(search)) ||
                    (t.NhomThuoc != null && t.NhomThuoc.TenNhomThuoc.Contains(search)) ||
                    (t.ThuongHieu != null && t.ThuongHieu.TenThuongHieu.Contains(search)));
            }

            var danhSach = await query
                .OrderByDescending(t => t.MaThuoc)
                .ToListAsync();

            ViewBag.Search = search;
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
            // Bỏ qua validation cho navigation properties và các trường nullable
            ModelState.Remove("NhomThuoc");
            ModelState.Remove("NuocSanXuat");
            ModelState.Remove("ThuongHieu");
            ModelState.Remove("CT_ThanhPhans");
            ModelState.Remove("CT_TacDungPhus");
            ModelState.Remove("CT_DoiTuongs");
            ModelState.Remove("ChiTietDonHangs");
            ModelState.Remove("MaThuongHieu");
            ModelState.Remove("MaNuocSX");
            ModelState.Remove("GiaGoc");
            ModelState.Remove("PhanTramGiam");
            ModelState.Remove("SoLuongTon");
            ModelState.Remove("SoLuongDaBan");
            ModelState.Remove("NgayBatDauKM");
            ModelState.Remove("NgayKetThucKM");
            ModelState.Remove("IsHot");
            ModelState.Remove("IsNew");
            ModelState.Remove("IsActive");
            ModelState.Remove("GiaBan");

            if (ModelState.IsValid)
            {
                // Gán ngày tạo
                thuoc.NgayTao = DateTime.Now;
                
                // Xử lý checkbox - nếu không tick thì giá trị là null, cần set false
                if (!thuoc.IsHot.HasValue) thuoc.IsHot = false;
                if (!thuoc.IsNew.HasValue) thuoc.IsNew = false;
                if (!thuoc.IsActive.HasValue) thuoc.IsActive = true; // Mặc định đang kinh doanh
                
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

                // Lưu thành phần (loại bỏ trùng lặp)
                if (ThanhPhanIds != null)
                {
                    var addedThanhPhans = new HashSet<int>();
                    for (int i = 0; i < ThanhPhanIds.Length; i++)
                    {
                        if (ThanhPhanIds[i] > 0 && !addedThanhPhans.Contains(ThanhPhanIds[i]))
                        {
                            addedThanhPhans.Add(ThanhPhanIds[i]);
                            _context.CT_THANH_PHAN.Add(new CT_ThanhPhan
                            {
                                MaThuoc = thuoc.MaThuoc,
                                MaThanhPhan = ThanhPhanIds[i],
                                HamLuong = HamLuongs != null && i < HamLuongs.Length ? HamLuongs[i] : null
                            });
                        }
                    }
                }

                // Lưu tác dụng phụ (loại bỏ trùng lặp)
                if (TacDungPhuIds != null)
                {
                    var addedTacDungPhus = new HashSet<int>();
                    for (int i = 0; i < TacDungPhuIds.Length; i++)
                    {
                        if (TacDungPhuIds[i] > 0 && !addedTacDungPhus.Contains(TacDungPhuIds[i]))
                        {
                            addedTacDungPhus.Add(TacDungPhuIds[i]);
                            _context.CT_TAC_DUNG_PHU.Add(new CT_TacDungPhu
                            {
                                MaThuoc = thuoc.MaThuoc,
                                MaTacDungPhu = TacDungPhuIds[i],
                                MucDo = MucDos != null && i < MucDos.Length ? MucDos[i] : null
                            });
                        }
                    }
                }

                // Lưu đối tượng sử dụng (loại bỏ trùng lặp)
                if (DoiTuongIds != null)
                {
                    foreach (var doiTuongId in DoiTuongIds.Distinct())
                    {
                        if (doiTuongId > 0)
                        {
                            _context.CT_DOI_TUONG.Add(new CT_DoiTuong
                            {
                                MaThuoc = thuoc.MaThuoc,
                                MaDoiTuong = doiTuongId
                            });
                        }
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

            // Bỏ qua validation cho navigation properties và các trường nullable
            ModelState.Remove("NhomThuoc");
            ModelState.Remove("NuocSanXuat");
            ModelState.Remove("ThuongHieu");
            ModelState.Remove("CT_ThanhPhans");
            ModelState.Remove("CT_TacDungPhus");
            ModelState.Remove("CT_DoiTuongs");
            ModelState.Remove("ChiTietDonHangs");
            ModelState.Remove("MaThuongHieu");
            ModelState.Remove("MaNuocSX");
            ModelState.Remove("GiaGoc");
            ModelState.Remove("PhanTramGiam");
            ModelState.Remove("SoLuongTon");
            ModelState.Remove("SoLuongDaBan");
            ModelState.Remove("NgayBatDauKM");
            ModelState.Remove("NgayKetThucKM");
            ModelState.Remove("IsHot");
            ModelState.Remove("IsNew");
            ModelState.Remove("IsActive");
            ModelState.Remove("GiaBan");

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy thuốc cũ từ DB để giữ lại hình ảnh nếu không upload mới
                    var existingThuoc = await _context.THUOC.AsNoTracking().FirstOrDefaultAsync(t => t.MaThuoc == id);
                    if (existingThuoc == null)
                        return NotFound();
                    
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
                    else if (string.IsNullOrEmpty(thuoc.HinhAnh))
                    {
                        // Giữ lại hình ảnh cũ nếu không upload mới
                        thuoc.HinhAnh = existingThuoc.HinhAnh;
                    }
                    
                    // Giữ lại NgayTao từ bản ghi cũ
                    thuoc.NgayTao = existingThuoc.NgayTao;
                    
                    // Xử lý checkbox - nếu không tick thì giá trị là null, cần set false
                    if (!thuoc.IsHot.HasValue) thuoc.IsHot = false;
                    if (!thuoc.IsNew.HasValue) thuoc.IsNew = false;
                    if (!thuoc.IsActive.HasValue) thuoc.IsActive = false;

                    // Xóa dữ liệu cũ TRƯỚC khi update thuốc
                    var oldThanhPhans = await _context.CT_THANH_PHAN.AsNoTracking().Where(ct => ct.MaThuoc == id).ToListAsync();
                    var oldTacDungPhus = await _context.CT_TAC_DUNG_PHU.AsNoTracking().Where(ct => ct.MaThuoc == id).ToListAsync();
                    var oldDoiTuongs = await _context.CT_DOI_TUONG.AsNoTracking().Where(ct => ct.MaThuoc == id).ToListAsync();

                    // Xóa bằng ExecuteDelete để tránh tracking conflict
                    await _context.CT_THANH_PHAN.Where(ct => ct.MaThuoc == id).ExecuteDeleteAsync();
                    await _context.CT_TAC_DUNG_PHU.Where(ct => ct.MaThuoc == id).ExecuteDeleteAsync();
                    await _context.CT_DOI_TUONG.Where(ct => ct.MaThuoc == id).ExecuteDeleteAsync();

                    // Update thuốc
                    _context.Update(thuoc);
                    await _context.SaveChangesAsync();

                    // Thêm thành phần mới (loại bỏ trùng lặp)
                    if (ThanhPhanIds != null)
                    {
                        var addedThanhPhans = new HashSet<int>();
                        for (int i = 0; i < ThanhPhanIds.Length; i++)
                        {
                            if (ThanhPhanIds[i] > 0 && !addedThanhPhans.Contains(ThanhPhanIds[i]))
                            {
                                addedThanhPhans.Add(ThanhPhanIds[i]);
                                _context.CT_THANH_PHAN.Add(new CT_ThanhPhan
                                {
                                    MaThuoc = id,
                                    MaThanhPhan = ThanhPhanIds[i],
                                    HamLuong = HamLuongs != null && i < HamLuongs.Length ? HamLuongs[i] : null
                                });
                            }
                        }
                    }

                    // Thêm tác dụng phụ mới (loại bỏ trùng lặp)
                    if (TacDungPhuIds != null)
                    {
                        var addedTacDungPhus = new HashSet<int>();
                        for (int i = 0; i < TacDungPhuIds.Length; i++)
                        {
                            if (TacDungPhuIds[i] > 0 && !addedTacDungPhus.Contains(TacDungPhuIds[i]))
                            {
                                addedTacDungPhus.Add(TacDungPhuIds[i]);
                                _context.CT_TAC_DUNG_PHU.Add(new CT_TacDungPhu
                                {
                                    MaThuoc = id,
                                    MaTacDungPhu = TacDungPhuIds[i],
                                    MucDo = MucDos != null && i < MucDos.Length ? MucDos[i] : null
                                });
                            }
                        }
                    }

                    // Thêm đối tượng sử dụng mới (loại bỏ trùng lặp)
                    if (DoiTuongIds != null)
                    {
                        foreach (var doiTuongId in DoiTuongIds.Distinct())
                        {
                            if (doiTuongId > 0)
                            {
                                _context.CT_DOI_TUONG.Add(new CT_DoiTuong
                                {
                                    MaThuoc = id,
                                    MaDoiTuong = doiTuongId
                                });
                            }
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
            ViewBag.SelectedDoiTuongs = DoiTuongIds?.ToList() ?? new List<int>();
            return View(thuoc);
        }

        // POST: Admin/Thuoc/Delete/5 - EF + LINQ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var thuoc = await _context.THUOC.FindAsync(id);
            if (thuoc == null)
            {
                TempData["LoiThongBao"] = "Không tìm thấy thuốc cần xóa!";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra thuốc có trong đơn hàng không
            var coTrongDonHang = await _context.CHI_TIET_DON_HANG.AnyAsync(ct => ct.MaThuoc == id);
            if (coTrongDonHang)
            {
                TempData["LoiThongBao"] = $"Không thể xóa thuốc \"{thuoc.TenThuoc}\" vì đã có trong đơn hàng. Bạn có thể đánh dấu \"Ngừng kinh doanh\" thay vì xóa.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Xóa các bảng liên quan trước (nếu cần)
                await _context.CT_THANH_PHAN.Where(ct => ct.MaThuoc == id).ExecuteDeleteAsync();
                await _context.CT_TAC_DUNG_PHU.Where(ct => ct.MaThuoc == id).ExecuteDeleteAsync();
                await _context.CT_DOI_TUONG.Where(ct => ct.MaThuoc == id).ExecuteDeleteAsync();

                _context.THUOC.Remove(thuoc);
                await _context.SaveChangesAsync();
                TempData["ThongBao"] = $"Đã xóa thuốc \"{thuoc.TenThuoc}\" thành công!";
            }
            catch (Exception ex)
            {
                TempData["LoiThongBao"] = $"Lỗi khi xóa thuốc: {ex.Message}";
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
