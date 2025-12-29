using System.ComponentModel.DataAnnotations;

namespace QL_NhaThuoc.Models
{
    public class TacDungPhu
    {
        public int MaTacDungPhu { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập tên tác dụng phụ")]
        [StringLength(100, ErrorMessage = "Tên tác dụng phụ không quá 100 ký tự")]
        [Display(Name = "Tên tác dụng phụ")]
        public string TenTacDungPhu { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Mô tả không quá 500 ký tự")]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }
        
        public ICollection<CT_TacDungPhu>? CT_TacDungPhus { get; set; }
    }
}
