using System.ComponentModel.DataAnnotations;

namespace QL_NhaThuoc.ViewModels
{
    /// <summary>
    /// ViewModel cho thông tin profile người dùng
    /// </summary>
    public class NguoiDungProfileVM
    {
        public int MaNguoiDung { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2 đến 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [RegularExpression(@"^(0[3|5|7|8|9])+([0-9]{8})$", ErrorMessage = "Số điện thoại phải có 10 số và bắt đầu bằng 03, 05, 07, 08 hoặc 09")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Địa chỉ không quá 500 ký tự")]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [Display(Name = "Ngày tạo tài khoản")]
        public DateTime? NgayTao { get; set; }

        // Thống kê
        [Display(Name = "Tổng đơn hàng")]
        public int TongDonHang { get; set; }
        
        [Display(Name = "Tổng chi tiêu")]
        public decimal TongChiTieu { get; set; }
    }
}
