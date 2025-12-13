namespace QL_NhaThuoc.Models
{
    public class Thuoc
    {
        public int MaThuoc { get; set; }
        public string TenThuoc { get; set; } = string.Empty;
        public int MaNhomThuoc { get; set; }
        public int? MaNuocSX { get; set; }
        public int? MaThuongHieu { get; set; }
        public decimal? GiaBan { get; set; }
        public string? DonViTinh { get; set; }
        public string? MoTa { get; set; }
        public string? HinhAnh { get; set; }
        
        // Navigation properties
        public NhomThuoc? NhomThuoc { get; set; }
        public NuocSanXuat? NuocSanXuat { get; set; }
        public ThuongHieu? ThuongHieu { get; set; }
        
        public ICollection<CT_ThanhPhan>? CT_ThanhPhans { get; set; }
        public ICollection<CT_DoiTuong>? CT_DoiTuongs { get; set; }
        public ICollection<CT_TacDungPhu>? CT_TacDungPhus { get; set; }
        public ICollection<ChiTietDonHang>? ChiTietDonHangs { get; set; }
    }
}
