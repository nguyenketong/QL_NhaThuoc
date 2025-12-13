using QL_NhaThuoc.Models;

namespace QL_NhaThuoc.ViewModels
{
    /// <summary>
    /// ViewModel cho hiển thị đơn hàng với thông tin chi tiết
    /// </summary>
    public class DonHangVM
    {
        public int MaDonHang { get; set; }
        public DateTime NgayDatHang { get; set; }
        public decimal? TongTien { get; set; }
        public string? TrangThai { get; set; }
        public string? PhuongThucThanhToan { get; set; }
        public string? DiaChiGiaoHang { get; set; }

        // Thông tin người dùng
        public string? HoTen { get; set; }
        public string? SoDienThoai { get; set; }

        // Chi tiết đơn hàng
        public List<ChiTietDonHangVM> ChiTietDonHangs { get; set; } = new();

        // Trạng thái hiển thị
        public string TrangThaiHienThi
        {
            get
            {
                return TrangThai switch
                {
                    "Cho xu ly" => "Chờ xử lý",
                    "Dang giao" => "Đang giao",
                    "Hoan thanh" => "Hoàn thành",
                    "Da huy" => "Đã hủy",
                    _ => TrangThai ?? ""
                };
            }
        }

        public string TrangThaiBadgeClass
        {
            get
            {
                return TrangThai switch
                {
                    "Cho xu ly" => "bg-warning",
                    "Dang giao" => "bg-info",
                    "Hoan thanh" => "bg-success",
                    "Da huy" => "bg-danger",
                    _ => "bg-secondary"
                };
            }
        }
    }

    public class ChiTietDonHangVM
    {
        public int MaThuoc { get; set; }
        public string TenThuoc { get; set; } = string.Empty;
        public int SoLuong { get; set; }
        public decimal? DonGia { get; set; }
        public decimal? ThanhTien { get; set; }
        public string? HinhAnh { get; set; }
    }
}
