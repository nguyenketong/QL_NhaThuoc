using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_NhaThuoc.Data;

namespace QL_NhaThuoc.Controllers
{
    public class NhomThuocController : Controller
    {
        private readonly QL_NhaThuocDbContext _context;

        public NhomThuocController(QL_NhaThuocDbContext context)
        {
            _context = context;
        }

        // GET: NhomThuoc/DanhSach - EF + LINQ
        public async Task<IActionResult> DanhSach()
        {
            // Chỉ lấy các nhóm cấp cha (MaDanhMucCha = null)
            var nhomCha = await _context.NHOM_THUOC
                .Where(n => n.MaDanhMucCha == null)
                .Include(n => n.DanhMucCon)
                .Include(n => n.Thuocs)
                .OrderBy(n => n.TenNhomThuoc)
                .ToListAsync();

            // Tính số lượng sản phẩm cho mỗi nhóm cha (bao gồm cả sản phẩm của nhóm con)
            foreach (var nhom in nhomCha)
            {
                var soLuongTrucTiep = nhom.Thuocs?.Count ?? 0;
                var soLuongTuCon = 0;
                
                if (nhom.DanhMucCon != null)
                {
                    foreach (var con in nhom.DanhMucCon)
                    {
                        soLuongTuCon += await _context.THUOC.CountAsync(t => t.MaNhomThuoc == con.MaNhomThuoc);
                    }
                }
                
                // Lưu tổng số lượng vào ViewData
                ViewData[$"SoLuong_{nhom.MaNhomThuoc}"] = soLuongTrucTiep + soLuongTuCon;
            }

            return View(nhomCha);
        }

        // GET: NhomThuoc/ChiTiet/5 - EF + LINQ với phân trang
        // Nếu là danh mục cha, hiển thị tất cả sản phẩm của danh mục cha + các danh mục con
        public async Task<IActionResult> ChiTiet(int id, int page = 1, string? sapXep = null)
        {
            // Load nhóm thuốc hiện tại
            var nhomThuoc = await _context.NHOM_THUOC
                .FirstOrDefaultAsync(n => n.MaNhomThuoc == id);

            if (nhomThuoc == null)
            {
                TempData["Error"] = "Không tìm thấy nhóm thuốc này!";
                return RedirectToAction("DanhSach");
            }

            // Load danh mục con riêng
            var danhMucCon = await _context.NHOM_THUOC
                .Where(n => n.MaDanhMucCha == id)
                .ToListAsync();

            // Lấy danh sách ID của nhóm hiện tại và tất cả danh mục con
            var nhomIds = new List<int> { id };
            nhomIds.AddRange(danhMucCon.Select(n => n.MaNhomThuoc));

            // Load tất cả sản phẩm vào memory trước, sau đó lọc
            var allThuocs = await _context.THUOC
                .Include(t => t.ThuongHieu)
                .ToListAsync();

            // Lọc theo nhóm trong memory
            var filteredThuocs = allThuocs.Where(t => nhomIds.Contains(t.MaNhomThuoc)).ToList();

            // Sắp xếp trong memory
            var sortedThuocs = sapXep switch
            {
                "gia-tang" => filteredThuocs.OrderBy(t => t.GiaBan).ToList(),
                "gia-giam" => filteredThuocs.OrderByDescending(t => t.GiaBan).ToList(),
                "ten-az" => filteredThuocs.OrderBy(t => t.TenThuoc).ToList(),
                "ten-za" => filteredThuocs.OrderByDescending(t => t.TenThuoc).ToList(),
                "moi-nhat" => filteredThuocs.OrderByDescending(t => t.NgayTao).ToList(),
                _ => filteredThuocs.OrderByDescending(t => t.MaThuoc).ToList()
            };

            // Phân trang trong memory
            int pageSize = 12;
            int totalItems = sortedThuocs.Count;
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
            var thuocs = sortedThuocs
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            nhomThuoc.Thuocs = thuocs;
            nhomThuoc.DanhMucCon = danhMucCon;

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.SapXep = sapXep;
            ViewBag.DanhMucCon = danhMucCon;

            return View(nhomThuoc);
        }
    }
}
