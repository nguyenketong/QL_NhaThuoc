namespace QL_NhaThuoc.Models
{
    public class DonHang
    {
        public int MaDonHang { get; set; }
        public int MaNguoiDung { get; set; }
        public DateTime NgayDatHang { get; set; }
        public decimal? TongTien { get; set; }
        public string? TrangThai { get; set; }
        public string? PhuongThucThanhToan { get; set; }
        public string? DiaChiGiaoHang { get; set; }
        
        // Navigation properties
        public NguoiDung? NguoiDung { get; set; }
        public ICollection<ChiTietDonHang>? ChiTietDonHangs { get; set; }
    }
}
