using System.ComponentModel.DataAnnotations;

namespace QL_NhaThuoc.ViewModels
{
    public class VerifyOtpVM
    {
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mã OTP")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải có 6 chữ số")]
        [Display(Name = "Mã OTP")]
        public string OTP { get; set; } = string.Empty;
    }
}
