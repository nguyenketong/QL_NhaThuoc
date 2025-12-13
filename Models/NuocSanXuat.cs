namespace QL_NhaThuoc.Models
{
    public class NuocSanXuat
    {
        public int MaNuocSX { get; set; }
        public string TenNuocSX { get; set; } = string.Empty;
        
        public ICollection<Thuoc>? Thuocs { get; set; }
    }
}
