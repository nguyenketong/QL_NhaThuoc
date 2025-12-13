using System.ComponentModel.DataAnnotations;

namespace QL_NhaThuoc.ViewModels
{
    public class PhoneLoginVM
    {
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
