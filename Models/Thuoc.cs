using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_NhaThuoc.Models
{
    public class Thuoc
    {
        public int MaThuoc { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập tên thuốc")]
        [StringLength(200, ErrorMessage = "Tên thuốc không quá 200 ký tự")]
        [Display(Name = "Tên thuốc")]
        public string TenThuoc { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Vui lòng chọn nhóm thuốc")]
        [Display(Name = "Nhóm thuốc")]
        public int MaNhomThuoc { get; set; }
        
        [Display(Name = "Nước sản xuất")]
        public int? MaNuocSX { get; set; }
        
        [Display(Name = "Thương hiệu")]
        public int? MaThuongHieu { get; set; }
        
        [Range(0, 100000000, ErrorMessage = "Giá bán phải từ 0 đến 100 triệu")]
        [Display(Name = "Giá bán")]
        public decimal? GiaBan { get; set; }
        
        [StringLength(50, ErrorMessage = "Đơn vị tính không quá 50 ký tự")]
        [Display(Name = "Đơn vị tính")]
        public string? DonViTinh { get; set; }
        
        [StringLength(2000, ErrorMessage = "Mô tả không quá 2000 ký tự")]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }
        
        [StringLength(500, ErrorMessage = "Đường dẫn hình ảnh không quá 500 ký tự")]
        [Display(Name = "Hình ảnh")]
        public string? HinhAnh { get; set; }
        
        [Range(0, 1000000, ErrorMessage = "Số lượng tồn phải từ 0 đến 1 triệu")]
        [Display(Name = "Số lượng tồn")]
        public int? SoLuongTon { get; set; }
        
        [Display(Name = "Số lượng đã bán")]
        public int? SoLuongDaBan { get; set; }
        
        [Display(Name = "Ngày tạo")]
        public DateTime? NgayTao { get; set; }
        
        // Trường khuyến mãi
        [Range(0, 100000000, ErrorMessage = "Giá gốc phải từ 0 đến 100 triệu")]
        [Display(Name = "Giá gốc")]
        public decimal? GiaGoc { get; set; }
        
        [Range(0, 100, ErrorMessage = "Phần trăm giảm phải từ 0 đến 100")]
        [Display(Name = "Phần trăm giảm")]
        public int? PhanTramGiam { get; set; }

        [Display(Name = "Ngày bắt đầu KM")]
        public DateTime? NgayBatDauKM { get; set; }
        
        [Display(Name = "Ngày kết thúc KM")]
        public DateTime? NgayKetThucKM { get; set; }
        
        // Đánh dấu sản phẩm
        [Display(Name = "Sản phẩm HOT")]
        public bool? IsHot { get; set; }
        
        [Display(Name = "Sản phẩm mới")]
        public bool? IsNew { get; set; }
        
        [Display(Name = "Đang kinh doanh")]
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
