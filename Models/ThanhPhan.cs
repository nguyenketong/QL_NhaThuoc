using System.ComponentModel.DataAnnotations;

namespace QL_NhaThuoc.Models
{
    public class ThanhPhan
    {
        public int MaThanhPhan { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập tên thành phần")]
        [StringLength(100, ErrorMessage = "Tên thành phần không quá 100 ký tự")]
        [Display(Name = "Tên thành phần")]
        public string TenThanhPhan { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Mô tả không quá 500 ký tự")]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }
        
        public ICollection<CT_ThanhPhan>? CT_ThanhPhans { get; set; }
    }
}
