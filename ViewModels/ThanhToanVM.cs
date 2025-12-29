using System.ComponentModel.DataAnnotations;

namespace QL_NhaThuoc.ViewModels
{
    /// <summary>
    /// ViewModel cho form thanh toán
    /// </summary>
    public class ThanhToanVM
    {
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Địa chỉ phải từ 10 đến 500 ký tự")]
        [Display(Name = "Địa chỉ giao hàng")]
        public string DiaChiGiaoHang { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        [RegularExpression(@"^(COD|Banking|MoMo|ZaloPay)$", ErrorMessage = "Phương thức thanh toán không hợp lệ")]
        [Display(Name = "Phương thức thanh toán")]
        public string PhuongThucThanhToan { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Ghi chú không quá 500 ký tự")]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        // Thông tin người nhận (nếu khác người đặt)
        [StringLength(100, ErrorMessage = "Tên người nhận không quá 100 ký tự")]
        [Display(Name = "Tên người nhận")]
        public string? TenNguoiNhan { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [RegularExpression(@"^(0[3|5|7|8|9])+([0-9]{8})$", ErrorMessage = "Số điện thoại phải có 10 số")]
        [Display(Name = "Số điện thoại người nhận")]
        public string? SoDienThoaiNguoiNhan { get; set; }
    }
}
