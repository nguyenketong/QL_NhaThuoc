using System.ComponentModel.DataAnnotations;

namespace QL_NhaThuoc.ViewModels
{
    /// <summary>
    /// ViewModel cho bộ lọc tìm kiếm thuốc
    /// </summary>
    public class ThuocFilterVM
    {
        [Display(Name = "Nhóm thuốc")]
        public int? MaNhomThuoc { get; set; }

        [Display(Name = "Thương hiệu")]
        public int? MaThuongHieu { get; set; }

        [Display(Name = "Nước sản xuất")]
        public int? MaNuocSX { get; set; }

        [Display(Name = "Giá từ")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal? GiaMin { get; set; }

        [Display(Name = "Giá đến")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal? GiaMax { get; set; }

        [Display(Name = "Từ khóa tìm kiếm")]
        [StringLength(200)]
        public string? TuKhoa { get; set; }

        [Display(Name = "Sắp xếp theo")]
        public string? SapXep { get; set; } // "TenAZ", "TenZA", "GiaTang", "GiaGiam"
    }
}
