namespace QL_NhaThuoc.Models
{
    public class CT_DoiTuong
    {
        public int MaThuoc { get; set; }
        public int MaDoiTuong { get; set; }
        
        // Navigation properties
        public Thuoc? Thuoc { get; set; }
        public DoiTuongSuDung? DoiTuongSuDung { get; set; }
    }
}
