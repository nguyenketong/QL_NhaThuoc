using System.ComponentModel.DataAnnotations;

namespace QL_NhaThuoc.ViewModels
{
    /// <summary>
    /// ViewModel cho item trong giỏ hàng
    /// </summary>
    public class GioHangItemVM
    {
        public int MaThuoc { get; set; }

        [Required]
        [StringLength(150)]
        public string TenThuoc { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal GiaBan { get; set; }

        [Required]
        [Range(1, 1000, ErrorMessage = "Số lượng phải từ 1 đến 1000")]
        public int SoLuong { get; set; }

        public string? HinhAnh { get; set; }

        public string? DonViTinh { get; set; }

        // Tính toán
        public decimal ThanhTien => GiaBan * SoLuong;
    }
}
