using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_NhaThuoc.Models
{
    public class NuocSanXuat
    {
        public int MaNuocSX { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập tên nước sản xuất")]
        [StringLength(100, ErrorMessage = "Tên nước sản xuất không quá 100 ký tự")]
        [Display(Name = "Tên nước sản xuất")]
        public string TenNuocSX { get; set; } = string.Empty;
        
        public ICollection<Thuoc>? Thuocs { get; set; }
        
        [NotMapped]
        public int SoLuongThuoc { get; set; }
    }
}
