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
            var danhSachNhom = await _context.NHOM_THUOC
                .Select(n => new
                {
                    n.MaNhomThuoc,
                    n.TenNhomThuoc,
                    n.MoTa,
                    SoLuongThuoc = n.Thuocs != null ? n.Thuocs.Count : 0
                })
                .OrderBy(n => n.TenNhomThuoc)
                .ToListAsync();

            return View(danhSachNhom);
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
