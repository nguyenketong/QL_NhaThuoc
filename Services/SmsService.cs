using System.Text;
using System.Text.Json;

namespace QL_NhaThuoc.Services
{
    public class SmsService : ISmsService
    {
        private readonly ILogger<SmsService> _logger;

        public SmsService(ILogger<SmsService> logger)
        {
            _logger = logger;
        }

        // Method chính - hiển thị OTP trên console
        public Task<(bool Success, string? PinId)> SendOtpAsync(string phoneNumber)
        {
            // Tạo OTP 6 số
            var otp = new Random().Next(100000, 999999).ToString();
            
            _logger.LogInformation($"📱 Sending OTP to: {phoneNumber}");
            
            // Hiển thị OTP trên console
            Console.WriteLine($"\n╔══════════════════════════════════════╗");
            Console.WriteLine($"║         📱 MÃ OTP ĐĂNG NHẬP          ║");
            Console.WriteLine($"╠══════════════════════════════════════╣");
            Console.WriteLine($"║  SĐT: {phoneNumber,-30} ║");
            Console.WriteLine($"║  OTP: {otp,-30} ║");
            Console.WriteLine($"║  Hiệu lực: 5 phút                    ║");
            Console.WriteLine($"╚══════════════════════════════════════╝\n");
            
            _logger.LogInformation($"✅ OTP generated: {otp}");
            
            // Trả về OTP như PinId để lưu vào DB
            return Task.FromResult((true, otp))!;
        }

        // Xác minh OTP - so sánh trực tiếp
        public Task<bool> VerifyOtpAsync(string pinId, string pin)
        {
            var verified = pinId == pin;
            _logger.LogInformation($"🔐 Verify OTP: {(verified ? "SUCCESS" : "FAILED")}");
            return Task.FromResult(verified);
        }

        // Legacy method
        public Task<bool> SendOtpAsync(string phoneNumber, string otp)
        {
            throw new NotImplementedException();
        }
    }
}
