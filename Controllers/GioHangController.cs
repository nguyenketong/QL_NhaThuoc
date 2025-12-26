using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QL_NhaThuoc.Data;
using QL_NhaThuoc.ViewModels;
using System.Data;
using System.Text.Json;

namespace QL_NhaThuoc.Controllers
{
    public class GioHangController : Controller
    {
        private readonly QL_NhaThuocDbContext _context;
        private readonly string _connectionString;
        private const string GioHangCookieKey = "GioHang";

        public GioHangController(QL_NhaThuocDbContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        // GET: GioHang - Session based
        public async Task<IActionResult> Index()
        {
            var gioHang = LayGioHang();

            // Cập nhật thông tin sản phẩm từ database
            if (gioHang.Any())
            {
                foreach (var item in gioHang)
                {
                    var thuoc = await _context.THUOC
                        .Where(t => t.MaThuoc == item.MaThuoc)
                        .Select(t => new { t.SoLuongTon, t.HinhAnh, t.IsActive })
                        .FirstOrDefaultAsync();

                    if (thuoc != null)
                    {
                        item.SoLuongTon = thuoc.SoLuongTon ?? 0;
                        item.HinhAnh = thuoc.HinhAnh;
                        item.NgungKinhDoanh = thuoc.IsActive == false;
                        // Nếu sản phẩm không khả dụng, bỏ chọn
                        if (item.KhongKhaDung)
                        {
                            item.DuocChon = false;
                        }
                    }
                    else
                    {
                        // Sản phẩm đã bị xóa khỏi database
                        item.NgungKinhDoanh = true;
                        item.DuocChon = false;
                    }
                }
            }

            return View(gioHang);
        }

        // POST: GioHang/ThemAjax - AJAX + EF
        [HttpPost]
        public async Task<IActionResult> ThemAjax(int maThuoc, int soLuong = 1)
        {
            // Lấy thông tin thuốc từ database - EF
            var thuoc = await _context.THUOC.FindAsync(maThuoc);
            if (thuoc == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại!" });
            }

            // Kiểm tra ngừng kinh doanh
            if (thuoc.IsActive == false)
            {
                return Json(new { success = false, message = "Sản phẩm đã ngừng kinh doanh!" });
            }

            // Kiểm tra hết hàng
            var soLuongTon = thuoc.SoLuongTon ?? 0;
            if (soLuongTon <= 0)
            {
                return Json(new { success = false, message = "Sản phẩm đã hết hàng!" });
            }

            var gioHang = LayGioHang();
            var item = gioHang.FirstOrDefault(x => x.MaThuoc == maThuoc);

            // Kiểm tra số lượng tồn kho
            var soLuongTrongGio = item?.SoLuong ?? 0;
            if (soLuongTrongGio + soLuong > soLuongTon)
            {
                return Json(new { success = false, message = $"Chỉ còn {soLuongTon} sản phẩm trong kho!" });
            }

            // Tính giá: kiểm tra khuyến mãi còn hiệu lực không
            var giaBan = thuoc.GiaBan ?? 0;
            var dangKhuyenMai = thuoc.PhanTramGiam.HasValue && thuoc.PhanTramGiam > 0
                && (!thuoc.NgayBatDauKM.HasValue || thuoc.NgayBatDauKM <= DateTime.Now)
                && (!thuoc.NgayKetThucKM.HasValue || thuoc.NgayKetThucKM >= DateTime.Now);
            
            // Nếu hết khuyến mãi, dùng giá gốc
            if (!dangKhuyenMai && thuoc.GiaGoc.HasValue && thuoc.GiaGoc > 0)
            {
                giaBan = thuoc.GiaGoc.Value;
            }

            if (item != null)
            {
                item.SoLuong += soLuong;
                item.GiaBan = giaBan; // Cập nhật giá mới nhất
            }
            else
            {
                gioHang.Add(new GioHangItem
                {
                    MaThuoc = maThuoc,
                    TenThuoc = thuoc.TenThuoc ?? "",
                    HinhAnh = thuoc.HinhAnh ?? "",
                    GiaBan = giaBan,
                    SoLuong = soLuong
                });
            }

            LuuGioHang(gioHang);
            return Json(new { success = true, soLuong = gioHang.Sum(x => x.SoLuong), message = "Đã thêm vào giỏ hàng!" });
        }

        // POST: GioHang/CapNhat
        [HttpPost]
        public async Task<IActionResult> CapNhat(int maThuoc, int soLuong)
        {
            var gioHang = LayGioHang();
            var item = gioHang.FirstOrDefault(x => x.MaThuoc == maThuoc);

            if (item != null)
            {
                if (soLuong <= 0)
                {
                    gioHang.Remove(item);
                }
                else
                {
                    // Kiểm tra số lượng tồn kho
                    var thuoc = await _context.THUOC.FindAsync(maThuoc);
                    var soLuongTon = thuoc?.SoLuongTon ?? 0;
                    
                    if (soLuong > soLuongTon)
                    {
                        TempData["LoiThongBao"] = $"Chỉ còn {soLuongTon} sản phẩm trong kho!";
                        item.SoLuong = soLuongTon;
                    }
                    else
                    {
                        item.SoLuong = soLuong;
                    }
                }
            }

            LuuGioHang(gioHang);
            return RedirectToAction(nameof(Index));
        }

        // POST: GioHang/CapNhatSoLuong - AJAX
        [HttpPost]
        public async Task<IActionResult> CapNhatSoLuong(int maThuoc, int soLuong)
        {
            var gioHang = LayGioHang();
            var item = gioHang.FirstOrDefault(x => x.MaThuoc == maThuoc);

            if (item != null)
            {
                if (soLuong <= 0)
                {
                    gioHang.Remove(item);
                }
                else
                {
                    // Kiểm tra số lượng tồn kho
                    var thuoc = await _context.THUOC.FindAsync(maThuoc);
                    var soLuongTon = thuoc?.SoLuongTon ?? 0;
                    
                    if (soLuong > soLuongTon)
                    {
                        LuuGioHang(gioHang);
                        return Json(new { success = false, message = $"Chỉ còn {soLuongTon} sản phẩm trong kho!" });
                    }
                    item.SoLuong = soLuong;
                }
            }

            LuuGioHang(gioHang);
            return Json(new { success = true, tongTien = gioHang.Sum(x => x.ThanhTien) });
        }

        // POST: GioHang/CapNhatChon - AJAX cập nhật trạng thái chọn sản phẩm
        [HttpPost]
        public IActionResult CapNhatChon(int maThuoc, bool duocChon)
        {
            var gioHang = LayGioHang();
            var item = gioHang.FirstOrDefault(x => x.MaThuoc == maThuoc);

            if (item != null && !item.KhongKhaDung)
            {
                item.DuocChon = duocChon;
                LuuGioHang(gioHang);
            }

            var tongTienChon = gioHang.Where(x => x.DuocChon && !x.KhongKhaDung).Sum(x => x.ThanhTien);
            return Json(new { success = true, tongTien = tongTienChon });
        }

        // POST: GioHang/ChonTatCa - AJAX chọn/bỏ chọn tất cả
        [HttpPost]
        public IActionResult ChonTatCa(bool chon)
        {
            var gioHang = LayGioHang();
            foreach (var item in gioHang.Where(x => !x.KhongKhaDung))
            {
                item.DuocChon = chon;
            }
            LuuGioHang(gioHang);

            var tongTienChon = gioHang.Where(x => x.DuocChon && !x.KhongKhaDung).Sum(x => x.ThanhTien);
            return Json(new { success = true, tongTien = tongTienChon });
        }

        // GET: GioHang/Xoa - Cho phép GET để dùng link
        public IActionResult Xoa(int maThuoc)
        {
            var gioHang = LayGioHang();
            var item = gioHang.FirstOrDefault(x => x.MaThuoc == maThuoc);

            if (item != null)
                gioHang.Remove(item);

            LuuGioHang(gioHang);
            return RedirectToAction(nameof(Index));
        }

        // GET: GioHang/XoaTatCa
        public IActionResult XoaTatCa()
        {
            XoaGioHangCookie();
            TempData["ThongBao"] = "Đã xóa toàn bộ giỏ hàng!";
            return RedirectToAction(nameof(Index));
        }

        // GET: GioHang/LaySoLuong - AJAX
        [HttpGet]
        public IActionResult LaySoLuong()
        {
            var gioHang = LayGioHang();
            return Json(new { soLuong = gioHang.Sum(x => x.SoLuong) });
        }

        // GET: GioHang/ThanhToan
        public async Task<IActionResult> ThanhToan()
        {
            var maNguoiDung = GetCurrentUserId();
            if (!maNguoiDung.HasValue)
            {
                TempData["LoiThongBao"] = "Vui lòng đăng nhập để thanh toán!";
                return RedirectToAction("PhoneLogin", "User");
            }

            var gioHang = LayGioHang();
            
            // Cập nhật thông tin sản phẩm và lọc chỉ lấy sản phẩm được chọn + khả dụng
            foreach (var item in gioHang)
            {
                var thuoc = await _context.THUOC
                    .Where(t => t.MaThuoc == item.MaThuoc)
                    .Select(t => new { t.SoLuongTon, t.IsActive })
                    .FirstOrDefaultAsync();

                if (thuoc != null)
                {
                    item.SoLuongTon = thuoc.SoLuongTon ?? 0;
                    item.NgungKinhDoanh = thuoc.IsActive == false;
                }
                else
                {
                    item.NgungKinhDoanh = true;
                }
            }

            // Chỉ lấy sản phẩm được chọn và khả dụng
            var gioHangThanhToan = gioHang.Where(x => x.DuocChon && !x.KhongKhaDung).ToList();
            
            if (!gioHangThanhToan.Any())
            {
                TempData["LoiThongBao"] = "Vui lòng chọn sản phẩm để thanh toán!";
                return RedirectToAction(nameof(Index));
            }

            var nguoiDung = await _context.NGUOI_DUNG.FindAsync(maNguoiDung.Value);
            ViewBag.NguoiDung = nguoiDung;
            
            return View(gioHangThanhToan);
        }

        // POST: GioHang/DatHang - Stored Procedure (transaction phức tạp)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DatHang(string diaChiGiaoHang, string phuongThucThanhToan, string hinhThucNhanHang)
        {
            var maNguoiDung = GetCurrentUserId();
            if (!maNguoiDung.HasValue)
                return RedirectToAction("PhoneLogin", "User");

            var gioHang = LayGioHang();
            
            // Cập nhật và lọc sản phẩm được chọn + khả dụng
            foreach (var item in gioHang)
            {
                var thuoc = await _context.THUOC
                    .Where(t => t.MaThuoc == item.MaThuoc)
                    .Select(t => new { t.SoLuongTon, t.IsActive })
                    .FirstOrDefaultAsync();

                if (thuoc != null)
                {
                    item.SoLuongTon = thuoc.SoLuongTon ?? 0;
                    item.NgungKinhDoanh = thuoc.IsActive == false;
                }
                else
                {
                    item.NgungKinhDoanh = true;
                }
            }

            var gioHangDatHang = gioHang.Where(x => x.DuocChon && !x.KhongKhaDung).ToList();
            if (!gioHangDatHang.Any())
            {
                TempData["LoiThongBao"] = "Vui lòng chọn sản phẩm để thanh toán!";
                return RedirectToAction(nameof(Index));
            }

            // Xử lý địa chỉ giao hàng
            string diaChiFinal;
            if (hinhThucNhanHang == "Nhận tại nhà thuốc")
            {
                diaChiFinal = "Nhận tại nhà thuốc: 123 Đường ABC, Phường XYZ, Quận 1, TP.HCM";
            }
            else if (!string.IsNullOrEmpty(diaChiGiaoHang))
            {
                diaChiFinal = diaChiGiaoHang;
            }
            else
            {
                TempData["LoiThongBao"] = "Vui lòng nhập địa chỉ giao hàng!";
                return RedirectToAction(nameof(ThanhToan));
            }

            var tongTien = gioHangDatHang.Sum(x => x.ThanhTien);

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                // 1. Tạo đơn hàng - sp_DonHang_TaoDonHang
                int maDonHang;
                using (var cmd = new SqlCommand("sp_DonHang_TaoDonHang", connection, transaction))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung.Value);
                    cmd.Parameters.AddWithValue("@DiaChiGiaoHang", diaChiFinal);
                    cmd.Parameters.AddWithValue("@PhuongThucThanhToan", phuongThucThanhToan);
                    cmd.Parameters.AddWithValue("@TongTien", tongTien);

                    var outputParam = new SqlParameter("@MaDonHangMoi", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(outputParam);

                    await cmd.ExecuteNonQueryAsync();
                    maDonHang = (int)outputParam.Value;
                }

                // 2. Thêm chi tiết đơn hàng - sp_DonHang_ThemChiTiet
                foreach (var item in gioHangDatHang)
                {
                    using var cmd = new SqlCommand("sp_DonHang_ThemChiTiet", connection, transaction);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MaDonHang", maDonHang);
                    cmd.Parameters.AddWithValue("@MaThuoc", item.MaThuoc);
                    cmd.Parameters.AddWithValue("@SoLuong", item.SoLuong);
                    cmd.Parameters.AddWithValue("@DonGia", item.GiaBan);
                    cmd.Parameters.AddWithValue("@ThanhTien", item.ThanhTien);

                    await cmd.ExecuteNonQueryAsync();
                }

                transaction.Commit();

                // Chỉ xóa sản phẩm đã đặt hàng, giữ lại sản phẩm chưa chọn
                var sanPhamConLai = gioHang.Where(x => !x.DuocChon || x.KhongKhaDung).ToList();
                if (sanPhamConLai.Any())
                {
                    LuuGioHang(sanPhamConLai);
                }
                else
                {
                    XoaGioHangCookie();
                };

                TempData["ThongBao"] = "Đặt hàng thành công!";
                
                // Nếu chọn chuyển khoản, redirect đến trang QR
                if (phuongThucThanhToan == "Chuyển khoản")
                {
                    return RedirectToAction("ThanhToanQR", "DonHang", new { id = maDonHang });
                }
                
                return RedirectToAction("ChiTiet", "DonHang", new { id = maDonHang });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                TempData["LoiThongBao"] = "Đặt hàng thất bại: " + ex.Message;
                return RedirectToAction(nameof(ThanhToan));
            }
        }

        #region Helper Methods - Cookie based (giữ lại khi restart app)
        private List<GioHangItem> LayGioHang()
        {
            var json = Request.Cookies[GioHangCookieKey];
            if (string.IsNullOrEmpty(json))
                return new List<GioHangItem>();
            
            try
            {
                return JsonSerializer.Deserialize<List<GioHangItem>>(json) ?? new List<GioHangItem>();
            }
            catch
            {
                return new List<GioHangItem>();
            }
        }

        private void LuuGioHang(List<GioHangItem> gioHang)
        {
            var json = JsonSerializer.Serialize(gioHang);
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(30), // Giữ 30 ngày
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            };
            Response.Cookies.Append(GioHangCookieKey, json, cookieOptions);
        }

        private void XoaGioHangCookie()
        {
            Response.Cookies.Delete(GioHangCookieKey);
        }

        private int? GetCurrentUserId()
        {
            if (Request.Cookies.TryGetValue("UserId", out var userIdStr) && int.TryParse(userIdStr, out var userId))
                return userId;
            return null;
        }
        #endregion
    }
}
