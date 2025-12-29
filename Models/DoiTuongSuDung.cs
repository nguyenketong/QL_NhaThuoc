using System.ComponentModel.DataAnnotations;

namespace QL_NhaThuoc.Models
{
    public class DoiTuongSuDung
    {
        public int MaDoiTuong { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập tên đối tượng")]
        [StringLength(100, ErrorMessage = "Tên đối tượng không quá 100 ký tự")]
        [Display(Name = "Tên đối tượng")]
        public string TenDoiTuong { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Mô tả không quá 500 ký tự")]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }
        
        public ICollection<CT_DoiTuong>? CT_DoiTuongs { get; set; }
    }
}
