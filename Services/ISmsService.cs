namespace QL_NhaThuoc.Services
{
    public interface ISmsService
    {
        // Infobip: Gửi OTP (Infobip tự tạo mã)
        Task<(bool Success, string? PinId)> SendOtpAsync(string phoneNumber);
        
        // Infobip: Xác minh OTP
        Task<bool> VerifyOtpAsync(string pinId, string pin);
        
        // Legacy method (không dùng nữa)
        Task<bool> SendOtpAsync(string phoneNumber, string otp);
    }
}
