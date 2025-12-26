using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_NhaThuoc.Data;
using QL_NhaThuoc.Filters;

namespace QL_NhaThuoc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminAuthorize]
    public class DonHangController : Controller
    {
        private readonly QL_NhaThuocDbContext _context;

        public DonHangController(QL_NhaThuocDbContext context)
        {
            _context = context;
        }

        // GET: Admin/DonHang - EF + LINQ
        public async Task<IActionResult> Index(string? trangThai)
        {
            var query = _context.DON_HANG
                .Include(dh => dh.NguoiDung)
                .Include(dh => dh.ChiTietDonHangs)
                .AsQueryable();

            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(dh => dh.TrangThai == trangThai);
            }

            var danhSach = await query
                .OrderByDescending(dh => dh.NgayDatHang)
                .Select(dh => new
                {
                    dh.MaDonHang,
                    HoTen = dh.NguoiDung != null ? dh.NguoiDung.HoTen : "",
                    SoDienThoai = dh.NguoiDung != null ? dh.NguoiDung.SoDienThoai : "",
                    dh.NgayDatHang,
                    dh.TongTien,
                    dh.TrangThai,
                    SoSanPham = dh.ChiTietDonHangs != null ? dh.ChiTietDonHangs.Count : 0
                })
                .ToListAsync();

            ViewBag.TrangThaiFilter = trangThai;
            return View(danhSach);
        }

        // GET: Admin/DonHang/Details/5 - EF + LINQ
        public async Task<IActionResult> Details(int id)
        {
            var donHang = await _context.DON_HANG
                .Include(dh => dh.NguoiDung)
                .Include(dh => dh.ChiTietDonHangs!)
                    .ThenInclude(ct => ct.Thuoc)
                .FirstOrDefaultAsync(dh => dh.MaDonHang == id);

            if (donHang == null)
                return NotFound();

            return View(donHang);
        }

        // POST: Admin/DonHang/CapNhatTrangThai - EF + LINQ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatTrangThai(int id, string trangThai)
        {
            var donHang = await _context.DON_HANG
                .Include(dh => dh.ChiTietDonHangs)
                .FirstOrDefaultAsync(dh => dh.MaDonHang == id);
                
            if (donHang != null)
            {
                var trangThaiHienTai = donHang.TrangThai;

                // Quy tắc 1: Đã hủy không thể chuyển về bất kỳ trạng thái nào
                if (trangThaiHienTai == "Da huy")
                {
                    TempData["LoiThongBao"] = "Đơn hàng đã hủy không thể thay đổi trạng thái!";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Quy tắc 2: Hoàn thành không thể chuyển về bất kỳ trạng thái nào
                if (trangThaiHienTai == "Hoan thanh")
                {
                    TempData["LoiThongBao"] = "Đơn hàng đã hoàn thành không thể thay đổi trạng thái!";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Quy tắc 3: Đang giao không được chuyển sang Đã hủy
                if (trangThaiHienTai == "Dang giao" && trangThai == "Da huy")
                {
                    TempData["LoiThongBao"] = "Đơn hàng đang giao không thể hủy!";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Quy tắc 4: Đã xác nhận thanh toán thì không thể hủy
                if (donHang.DaThanhToan && trangThai == "Da huy")
                {
                    TempData["LoiThongBao"] = "Đơn hàng đã thanh toán không thể hủy!";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Quy tắc 5: Chuyển khoản chưa thanh toán không cho chuyển sang Đang giao/Hoàn thành
                if (donHang.PhuongThucThanhToan == "Chuyển khoản" && !donHang.DaThanhToan)
                {
                    if (trangThai == "Dang giao" || trangThai == "Hoan thanh")
                    {
                        TempData["LoiThongBao"] = "Vui lòng xác nhận đã nhận tiền chuyển khoản trước!";
                        return RedirectToAction(nameof(Details), new { id });
                    }
                }

                // Trừ tồn kho khi chuyển từ "Chờ xử lý" sang "Đang giao" hoặc "Hoàn thành"
                if (trangThaiHienTai == "Cho xu ly" && (trangThai == "Dang giao" || trangThai == "Hoan thanh"))
                {
                    if (donHang.ChiTietDonHangs != null)
                    {
                        foreach (var ct in donHang.ChiTietDonHangs)
                        {
                            var thuoc = await _context.THUOC.FindAsync(ct.MaThuoc);
                            if (thuoc != null)
                            {
                                // Trừ số lượng tồn kho
                                thuoc.SoLuongTon = (thuoc.SoLuongTon ?? 0) - ct.SoLuong;
                                // Cộng số lượng đã bán
                                thuoc.SoLuongDaBan = (thuoc.SoLuongDaBan ?? 0) + ct.SoLuong;
                            }
                        }
                    }
                }

                donHang.TrangThai = trangThai;
                
                // Tạo thông báo cho user
                var thongBao = new Models.ThongBao
                {
                    MaNguoiDung = donHang.MaNguoiDung,
                    MaDonHang = donHang.MaDonHang,
                    LoaiThongBao = "DonHang",
                    DuongDan = $"/DonHang/ChiTiet/{donHang.MaDonHang}",
                    NgayTao = DateTime.Now
                };

                switch (trangThai)
                {
                    case "Dang giao":
                        thongBao.TieuDe = $"Đơn hàng #{donHang.MaDonHang} đang giao";
                        thongBao.NoiDung = "Đơn hàng của bạn đã được giao cho đơn vị vận chuyển. Vui lòng chú ý điện thoại!";
                        break;
                    case "Hoan thanh":
                        thongBao.TieuDe = $"Đơn hàng #{donHang.MaDonHang} hoàn thành";
                        thongBao.NoiDung = "Đơn hàng đã giao thành công. Cảm ơn bạn đã mua hàng!";
                        break;
                    case "Da huy":
                        thongBao.TieuDe = $"Đơn hàng #{donHang.MaDonHang} đã hủy";
                        thongBao.NoiDung = "Đơn hàng của bạn đã bị hủy. Liên hệ hotline nếu cần hỗ trợ.";
                        break;
                    default:
                        thongBao.TieuDe = $"Đơn hàng #{donHang.MaDonHang} cập nhật";
                        thongBao.NoiDung = $"Trạng thái đơn hàng đã được cập nhật thành: {trangThai}";
                        break;
                }

                _context.THONG_BAO.Add(thongBao);
                await _context.SaveChangesAsync();
                TempData["ThongBao"] = "Cập nhật trạng thái đơn hàng thành công!";
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Admin/DonHang/XacNhanThanhToan - Xác nhận đã nhận tiền chuyển khoản
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanThanhToan(int id)
        {
            var donHang = await _context.DON_HANG.FindAsync(id);
            if (donHang != null && donHang.PhuongThucThanhToan == "Chuyển khoản")
            {
                donHang.DaThanhToan = true;
                
                // Tạo thông báo cho user
                var thongBao = new Models.ThongBao
                {
                    MaNguoiDung = donHang.MaNguoiDung,
                    MaDonHang = donHang.MaDonHang,
                    TieuDe = $"Đơn hàng #{donHang.MaDonHang} đã xác nhận thanh toán",
                    NoiDung = "Chúng tôi đã nhận được tiền chuyển khoản của bạn. Đơn hàng sẽ sớm được xử lý!",
                    LoaiThongBao = "DonHang",
                    DuongDan = $"/DonHang/ChiTiet/{donHang.MaDonHang}",
                    NgayTao = DateTime.Now
                };

                _context.THONG_BAO.Add(thongBao);
                await _context.SaveChangesAsync();
                TempData["ThongBao"] = "Đã xác nhận thanh toán thành công!";
            }
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
