namespace QL_NhaThuoc.Models
{
    public class ThuongHieu
    {
        public int MaThuongHieu { get; set; }
        public string TenThuongHieu { get; set; } = string.Empty;
        public string? DiaChi { get; set; }
        public string? QuocGia { get; set; }
        
        public ICollection<Thuoc>? Thuocs { get; set; }
    }
}
