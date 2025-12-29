using System.ComponentModel.DataAnnotations;

namespace QL_NhaThuoc.Models
{
    public class NguoiDung
    {
        public int MaNguoiDung { get; set; }
        
        [StringLength(100, ErrorMessage = "Họ tên không quá 100 ký tự")]
        [Display(Name = "Họ tên")]
        public string? HoTen { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [RegularExpression(@"^(0[3|5|7|8|9])+([0-9]{8})$", ErrorMessage = "Số điện thoại phải có 10 số và bắt đầu bằng 03, 05, 07, 08 hoặc 09")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Địa chỉ không quá 500 ký tự")]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }
        
        [StringLength(6, ErrorMessage = "Mã OTP có 6 ký tự")]
        public string? OTP { get; set; }
        
        public DateTime? OTP_Expire { get; set; }
        
        [Display(Name = "Ngày tạo")]
        public DateTime? NgayTao { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        [RegularExpression(@"^(User|Admin)$", ErrorMessage = "Vai trò phải là User hoặc Admin")]
        [Display(Name = "Vai trò")]
        public string VaiTro { get; set; } = "User";
        
        public ICollection<DonHang>? DonHangs { get; set; }
    }
}
