using System.ComponentModel.DataAnnotations.Schema;

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
        
        // Trường bổ sung cho quản lý sản phẩm (nullable để tương thích DB cũ)
        public int? SoLuongTon { get; set; }
        public int? SoLuongDaBan { get; set; }
        public DateTime? NgayTao { get; set; }
        
        // Trường khuyến mãi
        public decimal? GiaGoc { get; set; }
        public int? PhanTramGiam { get; set; }
        public DateTime? NgayBatDauKM { get; set; }
        public DateTime? NgayKetThucKM { get; set; }
        
        // Đánh dấu sản phẩm (nullable)
        public bool? IsHot { get; set; }
        public bool? IsNew { get; set; }
        public bool? IsActive { get; set; }
        
        // Computed property - Kiểm tra đang khuyến mãi
        [NotMapped]
        public bool DangKhuyenMai => PhanTramGiam.HasValue && PhanTramGiam > 0 
            && (!NgayBatDauKM.HasValue || NgayBatDauKM <= DateTime.Now)
            && (!NgayKetThucKM.HasValue || NgayKetThucKM >= DateTime.Now);
        
        // Computed property - Giá sau khuyến mãi
        [NotMapped]
        public decimal GiaSauKM => DangKhuyenMai && GiaGoc.HasValue 
            ? GiaGoc.Value * (100 - PhanTramGiam!.Value) / 100 
            : GiaBan ?? 0;
        
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
