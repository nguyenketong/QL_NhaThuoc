using System.ComponentModel.DataAnnotations;

namespace QL_NhaThuoc.Models
{
    public class ThuongHieu
    {
        public int MaThuongHieu { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập tên thương hiệu")]
        [StringLength(100, ErrorMessage = "Tên thương hiệu không quá 100 ký tự")]
        [Display(Name = "Tên thương hiệu")]
        public string TenThuongHieu { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Địa chỉ không quá 500 ký tự")]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }
        
        [StringLength(100, ErrorMessage = "Quốc gia không quá 100 ký tự")]
        [Display(Name = "Quốc gia")]
        public string? QuocGia { get; set; }
        
        [StringLength(500, ErrorMessage = "Đường dẫn hình ảnh không quá 500 ký tự")]
        [Display(Name = "Hình ảnh")]
        public string? HinhAnh { get; set; }
        
        public ICollection<Thuoc>? Thuocs { get; set; }
    }
}
