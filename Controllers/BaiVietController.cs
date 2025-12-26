using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_NhaThuoc.Data;

namespace QL_NhaThuoc.Controllers
{
    public class BaiVietController : Controller
    {
        private readonly QL_NhaThuocDbContext _context;
        private const int PageSize = 10; // 5 dòng x 2 cột = 10 bài/trang

        public BaiVietController(QL_NhaThuocDbContext context)
        {
            _context = context;
        }

        // GET: BaiViet/GocSucKhoe
        public async Task<IActionResult> GocSucKhoe(int page = 1)
        {
            // Lấy tất cả bài viết active
            var allBaiViets = await _context.BAI_VIET
                .Where(b => b.IsActive == true)
                .OrderByDescending(b => b.NgayDang)
                .ToListAsync();

            // Tổng số bài viết
            var totalItems = allBaiViets.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            // Phân trang trong memory
            var baiViets = allBaiViets
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(baiViets);
        }

        // GET: BaiViet/ChiTiet/5
        public async Task<IActionResult> ChiTiet(int id)
        {
            var baiViet = await _context.BAI_VIET.FindAsync(id);
            if (baiViet == null || baiViet.IsActive != true)
                return NotFound();

            // Tăng lượt xem
            baiViet.LuotXem = (baiViet.LuotXem ?? 0) + 1;
            await _context.SaveChangesAsync();

            // Bài viết liên quan
            ViewBag.BaiVietLienQuan = await _context.BAI_VIET
                .Where(b => b.IsActive == true && b.MaBaiViet != id)
                .OrderByDescending(b => b.NgayDang)
                .Take(4)
                .ToListAsync();

            return View(baiViet);
        }
    }
}
