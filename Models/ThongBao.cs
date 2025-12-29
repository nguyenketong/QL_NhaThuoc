using System.ComponentModel.DataAnnotations;

namespace QL_NhaThuoc.Models
{
    public class ThongBao
    {
        public int MaThongBao { get; set; }
        
        [Required(ErrorMessage = "Thiếu mã người dùng")]
        [Display(Name = "Người nhận")]
        public int MaNguoiDung { get; set; }
        
        [Display(Name = "Đơn hàng")]
        public int? MaDonHang { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
        [StringLength(200, ErrorMessage = "Tiêu đề không quá 200 ký tự")]
        [Display(Name = "Tiêu đề")]
        public string TieuDe { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Vui lòng nhập nội dung")]
        [StringLength(1000, ErrorMessage = "Nội dung không quá 1000 ký tự")]
        [Display(Name = "Nội dung")]
        public string NoiDung { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Vui lòng chọn loại thông báo")]
        [RegularExpression(@"^(DonHang|KhuyenMai|HeThong)$", ErrorMessage = "Loại thông báo không hợp lệ")]
        [Display(Name = "Loại thông báo")]
        public string LoaiThongBao { get; set; } = "HeThong";
        
        [Display(Name = "Đã đọc")]
        public bool DaDoc { get; set; } = false;
        
        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;
        
        [StringLength(500, ErrorMessage = "Đường dẫn không quá 500 ký tự")]
        [Display(Name = "Đường dẫn")]
        public string? DuongDan { get; set; }

        // Navigation
        public NguoiDung? NguoiDung { get; set; }
        public DonHang? DonHang { get; set; }
    }
}
