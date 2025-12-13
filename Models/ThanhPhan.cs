namespace QL_NhaThuoc.Models
{
    public class ThanhPhan
    {
        public int MaThanhPhan { get; set; }
        public string TenThanhPhan { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        
        public ICollection<CT_ThanhPhan>? CT_ThanhPhans { get; set; }
    }
}
