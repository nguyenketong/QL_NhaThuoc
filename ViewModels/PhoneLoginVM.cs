using System.ComponentModel.DataAnnotations;

namespace QL_NhaThuoc.ViewModels
{
    public class PhoneLoginVM
    {
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [RegularExpression(@"^(0[3|5|7|8|9])+([0-9]{8})$", ErrorMessage = "Số điện thoại phải có 10 số và bắt đầu bằng 03, 05, 07, 08 hoặc 09")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
