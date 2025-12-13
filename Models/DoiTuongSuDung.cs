namespace QL_NhaThuoc.Models
{
    public class DoiTuongSuDung
    {
        public int MaDoiTuong { get; set; }
        public string TenDoiTuong { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        
        public ICollection<CT_DoiTuong>? CT_DoiTuongs { get; set; }
    }
}
