namespace QL_NhaThuoc.Models
{
    public class ThongBao
    {
        public int MaThongBao { get; set; }
        public int MaNguoiDung { get; set; }
        public int? MaDonHang { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public string LoaiThongBao { get; set; } = "HeThong"; // DonHang, KhuyenMai, HeThong
        public bool DaDoc { get; set; } = false;
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public string? DuongDan { get; set; }

        // Navigation
        public NguoiDung? NguoiDung { get; set; }
        public DonHang? DonHang { get; set; }
    }
}
