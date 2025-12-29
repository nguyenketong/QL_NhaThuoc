using System.ComponentModel.DataAnnotations;

namespace QL_NhaThuoc.Models
{
    public class BaiViet
    {
        public int MaBaiViet { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
        [StringLength(200, ErrorMessage = "Tiêu đề không quá 200 ký tự")]
        [Display(Name = "Tiêu đề")]
        public string TieuDe { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Mô tả ngắn không quá 500 ký tự")]
        [Display(Name = "Mô tả ngắn")]
        public string? MoTaNgan { get; set; }
        
        [Display(Name = "Nội dung")]
        public string? NoiDung { get; set; }
        
        [StringLength(500, ErrorMessage = "Đường dẫn hình ảnh không quá 500 ký tự")]
        [Display(Name = "Hình ảnh")]
        public string? HinhAnh { get; set; }
        
        [Display(Name = "Ngày đăng")]
        public DateTime? NgayDang { get; set; }
        
        [Range(0, int.MaxValue, ErrorMessage = "Lượt xem không hợp lệ")]
        [Display(Name = "Lượt xem")]
        public int? LuotXem { get; set; }
        
        [Display(Name = "Bài viết nổi bật")]
        public bool? IsNoiBat { get; set; }
        
        [Display(Name = "Đang hoạt động")]
        public bool? IsActive { get; set; }
    }
}
