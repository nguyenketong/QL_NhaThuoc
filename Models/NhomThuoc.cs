using System.ComponentModel.DataAnnotations;

namespace QL_NhaThuoc.Models
{
    public class NhomThuoc
    {
        public int MaNhomThuoc { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập tên nhóm thuốc")]
        [StringLength(100, ErrorMessage = "Tên nhóm thuốc không quá 100 ký tự")]
        [Display(Name = "Tên nhóm thuốc")]
        public string TenNhomThuoc { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Mô tả không quá 500 ký tự")]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }
        
        [Display(Name = "Danh mục cha")]
        public int? MaDanhMucCha { get; set; }
        
        // Navigation properties
        public NhomThuoc? DanhMucCha { get; set; }
        public ICollection<NhomThuoc>? DanhMucCon { get; set; }
        public ICollection<Thuoc>? Thuocs { get; set; }
    }
}
