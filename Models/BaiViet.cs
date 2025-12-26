namespace QL_NhaThuoc.Models
{
    public class BaiViet
    {
        public int MaBaiViet { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string? MoTaNgan { get; set; }
        public string? NoiDung { get; set; }
        public string? HinhAnh { get; set; }
        public DateTime? NgayDang { get; set; }
        public int? LuotXem { get; set; }
        public bool? IsNoiBat { get; set; }
        public bool? IsActive { get; set; }
    }
}
