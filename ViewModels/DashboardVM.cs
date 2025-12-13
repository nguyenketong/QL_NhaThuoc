namespace QL_NhaThuoc.ViewModels
{
    /// <summary>
    /// ViewModel cho trang dashboard/tổng quan
    /// </summary>
    public class DashboardVM
    {
        // Thống kê tổng quan
        public int TongSoThuoc { get; set; }
        public int TongSoNhomThuoc { get; set; }
        public int TongSoDonHang { get; set; }
        public int TongSoNguoiDung { get; set; }

        // Đơn hàng theo trạng thái
        public int DonHangChoXuLy { get; set; }
        public int DonHangDangGiao { get; set; }
        public int DonHangHoanThanh { get; set; }
        public int DonHangDaHuy { get; set; }

        // Doanh thu
        public decimal DoanhThuThang { get; set; }
        public decimal DoanhThuNam { get; set; }

        // Thuốc bán chạy
        public List<ThuocBanChayVM> ThuocBanChays { get; set; } = new();

        // Đơn hàng gần đây
        public List<DonHangGanDayVM> DonHangGanDays { get; set; } = new();
    }

    public class ThuocBanChayVM
    {
        public int MaThuoc { get; set; }
        public string TenThuoc { get; set; } = string.Empty;
        public int SoLuongBan { get; set; }
        public decimal? DoanhThu { get; set; }
    }

    public class DonHangGanDayVM
    {
        public int MaDonHang { get; set; }
        public string? HoTen { get; set; }
        public DateTime NgayDatHang { get; set; }
        public decimal? TongTien { get; set; }
        public string? TrangThai { get; set; }
    }
}
