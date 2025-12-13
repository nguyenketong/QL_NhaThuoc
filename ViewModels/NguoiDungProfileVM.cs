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
        [StringLength(150)]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; } = string.Empty;

        [StringLength(255)]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [Display(Name = "Ngày tạo tài khoản")]
        public DateTime? NgayTao { get; set; }

        // Thống kê
        public int TongDonHang { get; set; }
        public decimal TongChiTieu { get; set; }
    }
}
