using System.ComponentModel.DataAnnotations;

namespace QL_NhaThuoc.Models
{
    public class ChiTietDonHang
    {
        public int MaChiTiet { get; set; }
        
        [Required(ErrorMessage = "Thiếu mã đơn hàng")]
        [Display(Name = "Đơn hàng")]
        public int MaDonHang { get; set; }
        
        [Required(ErrorMessage = "Thiếu mã thuốc")]
        [Display(Name = "Thuốc")]
        public int MaThuoc { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập số lượng")]
        [Range(1, 10000, ErrorMessage = "Số lượng phải từ 1 đến 10,000")]
        [Display(Name = "Số lượng")]
        public int SoLuong { get; set; }
        
        [Range(0, 100000000, ErrorMessage = "Đơn giá phải từ 0 đến 100 triệu")]
        [Display(Name = "Đơn giá")]
        public decimal? DonGia { get; set; }
        
        [Range(0, 1000000000, ErrorMessage = "Thành tiền phải từ 0 đến 1 tỷ")]
        [Display(Name = "Thành tiền")]
        public decimal? ThanhTien { get; set; }
        
        // Navigation properties
        public DonHang? DonHang { get; set; }
        public Thuoc? Thuoc { get; set; }
    }
}
