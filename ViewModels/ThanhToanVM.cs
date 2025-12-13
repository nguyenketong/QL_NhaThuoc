using System.ComponentModel.DataAnnotations;

namespace QL_NhaThuoc.ViewModels
{
    /// <summary>
    /// ViewModel cho form thanh toán
    /// </summary>
    public class ThanhToanVM
    {
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        [StringLength(255)]
        [Display(Name = "Địa chỉ giao hàng")]
        public string DiaChiGiaoHang { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        [Display(Name = "Phương thức thanh toán")]
        public string PhuongThucThanhToan { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        // Thông tin người nhận (nếu khác người đặt)
        [StringLength(150)]
        [Display(Name = "Tên người nhận")]
        public string? TenNguoiNhan { get; set; }

        [Phone]
        [StringLength(20)]
        [Display(Name = "Số điện thoại người nhận")]
        public string? SoDienThoaiNguoiNhan { get; set; }
    }
}
