namespace QL_NhaThuoc.Models
{
    public class NhomThuoc
    {
        public int MaNhomThuoc { get; set; }
        public string TenNhomThuoc { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        
        public ICollection<Thuoc>? Thuocs { get; set; }
    }
}
