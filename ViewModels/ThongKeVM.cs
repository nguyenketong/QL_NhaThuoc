namespace QL_NhaThuoc.ViewModels
{
    /// <summary>
    /// ViewModel cho các báo cáo thống kê
    /// </summary>
    public class ThongKeVM
    {
        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }

        // Thống kê doanh thu
        public decimal TongDoanhThu { get; set; }
        public int TongDonHang { get; set; }
        public decimal GiaTriTrungBinh { get; set; }

        // Doanh thu theo ngày
        public List<DoanhThuTheoNgayVM> DoanhThuTheoNgays { get; set; } = new();

        // Top thuốc bán chạy
        public List<TopThuocVM> TopThuocs { get; set; } = new();

        // Thống kê theo nhóm thuốc
        public List<ThongKeNhomThuocVM> ThongKeNhomThuocs { get; set; } = new();
    }

    public class DoanhThuTheoNgayVM
    {
        public DateTime Ngay { get; set; }
        public decimal DoanhThu { get; set; }
        public int SoDonHang { get; set; }
    }

    public class TopThuocVM
    {
        public int MaThuoc { get; set; }
        public string TenThuoc { get; set; } = string.Empty;
        public int SoLuongBan { get; set; }
        public decimal DoanhThu { get; set; }
    }

    public class ThongKeNhomThuocVM
    {
        public int MaNhomThuoc { get; set; }
        public string TenNhomThuoc { get; set; } = string.Empty;
        public int SoLuongBan { get; set; }
        public decimal DoanhThu { get; set; }
        public decimal TyLe { get; set; }
    }
}
