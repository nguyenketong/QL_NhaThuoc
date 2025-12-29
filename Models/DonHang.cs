using System.ComponentModel.DataAnnotations;

namespace QL_NhaThuoc.Models
{
    public class DonHang
    {
        public int MaDonHang { get; set; }
        
        [Required(ErrorMessage = "Thiếu thông tin người dùng")]
        [Display(Name = "Người đặt")]
        public int MaNguoiDung { get; set; }
        
        [Required(ErrorMessage = "Thiếu ngày đặt hàng")]
        [Display(Name = "Ngày đặt hàng")]
        public DateTime NgayDatHang { get; set; }
        
        [Range(0, 1000000000, ErrorMessage = "Tổng tiền phải từ 0 đến 1 tỷ")]
        [Display(Name = "Tổng tiền")]
        public decimal? TongTien { get; set; }
        
        [StringLength(50, ErrorMessage = "Trạng thái không quá 50 ký tự")]
        [Display(Name = "Trạng thái")]
        public string? TrangThai { get; set; }
        
        [StringLength(50, ErrorMessage = "Phương thức thanh toán không quá 50 ký tự")]
        [Display(Name = "Phương thức thanh toán")]
        public string? PhuongThucThanhToan { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        [StringLength(500, ErrorMessage = "Địa chỉ giao hàng không quá 500 ký tự")]
        [Display(Name = "Địa chỉ giao hàng")]
        public string? DiaChiGiaoHang { get; set; }
        
        [Display(Name = "Đã thanh toán")]
        public bool DaThanhToan { get; set; } = false;
        
        // Navigation properties
        public NguoiDung? NguoiDung { get; set; }
        public ICollection<ChiTietDonHang>? ChiTietDonHangs { get; set; }
    }
}
