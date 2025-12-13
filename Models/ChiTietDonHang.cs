namespace QL_NhaThuoc.Models
{
    public class ChiTietDonHang
    {
        public int MaChiTiet { get; set; }
        public int MaDonHang { get; set; }
        public int MaThuoc { get; set; }
        public int SoLuong { get; set; }
        public decimal? DonGia { get; set; }
        public decimal? ThanhTien { get; set; }
        
        // Navigation properties
        public DonHang? DonHang { get; set; }
        public Thuoc? Thuoc { get; set; }
    }
}
