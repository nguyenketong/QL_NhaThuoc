namespace QL_NhaThuoc.Models
{
    public class NhomThuoc
    {
        public int MaNhomThuoc { get; set; }
        public string TenNhomThuoc { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        
        // Danh mục cha (null = danh mục gốc)
        public int? MaDanhMucCha { get; set; }
        
        // Navigation properties
        public NhomThuoc? DanhMucCha { get; set; }
        public ICollection<NhomThuoc>? DanhMucCon { get; set; }
        public ICollection<Thuoc>? Thuocs { get; set; }
    }
}
