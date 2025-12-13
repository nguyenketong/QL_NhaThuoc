namespace QL_NhaThuoc.Models
{
    public class CT_ThanhPhan
    {
        public int MaThuoc { get; set; }
        public int MaThanhPhan { get; set; }
        public string? HamLuong { get; set; }
        public string? GhiChu { get; set; }
        
        // Navigation properties
        public Thuoc? Thuoc { get; set; }
        public ThanhPhan? ThanhPhan { get; set; }
    }
}
