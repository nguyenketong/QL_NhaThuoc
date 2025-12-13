using Microsoft.EntityFrameworkCore;
using QL_NhaThuoc.Data;
using QL_NhaThuoc.Models;

namespace QL_NhaThuoc.Services
{
    public class OtpServiceVietnamese
    {
        private readonly QL_NhaThuocDbContext _context;
        private readonly ISmsService _smsService;
        private readonly ILogger<OtpServiceVietnamese> _logger;

        public OtpServiceVietnamese(QL_NhaThuocDbContext context, ISmsService smsService, ILogger<OtpServiceVietnamese> logger)
        {
            _context = context;
            _smsService = smsService;
            _logger = logger;
        }

        public async Task<(bool Success, string Message)> GenerateAndSendOtpAsync(string phoneNumber)
        {
            try
            {
                // Tim nguoi dung theo so dien thoai
                var nguoiDung = await _context.NGUOI_DUNG
                    .FirstOrDefaultAsync(u => u.SoDienThoai == phoneNumber);

                // Neu chua co tai khoan, tu dong tao moi
                if (nguoiDung == null)
                {
                    nguoiDung = new NguoiDung
                    {
                        SoDienThoai = phoneNumber,
                        HoTen = phoneNumber, // Dung so dien thoai lam ten nguoi dung
                        NgayTao = DateTime.Now
                    };
                    _context.NGUOI_DUNG.Add(nguoiDung);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation($"Auto-created new user for phone: {phoneNumber}");
                }

                // Tao ma OTP 6 chu so
                var otp = GenerateOtp();

                // Thoi gian het han: 5 phut
                var otpExpiry = DateTime.Now.AddMinutes(5);

                // Cap nhat OTP vao database (ghi de OTP cu)
                nguoiDung.OTP = otp;
                nguoiDung.OTP_Expire = otpExpiry;

                await _context.SaveChangesAsync();

                // Gui OTP qua SMS
                var smsSent = await _smsService.SendOtpAsync(phoneNumber, otp);
                if (!smsSent)
                {
                    return (false, "Khong the gui SMS. Vui long thu lai");
                }

                _logger.LogInformation($"OTP generated for {phoneNumber}: {otp}, expires at {otpExpiry}");

                return (true, "Ma OTP da duoc gui den so dien thoai cua ban");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating OTP for {phoneNumber}");
                return (false, "Da xay ra loi. Vui long thu lai");
            }
        }

        public async Task<(bool Success, string Message, NguoiDung? NguoiDung)> VerifyOtpAsync(string phoneNumber, string otp)
        {
            try
            {
                var nguoiDung = await _context.NGUOI_DUNG
                    .FirstOrDefaultAsync(u => u.SoDienThoai == phoneNumber);

                if (nguoiDung == null)
                {
                    return (false, "So dien thoai khong ton tai", null);
                }

                // Kiem tra OTP co ton tai khong
                if (string.IsNullOrEmpty(nguoiDung.OTP))
                {
                    return (false, "Vui long yeu cau ma OTP moi", null);
                }

                // Kiem tra OTP da het han chua
                if (nguoiDung.OTP_Expire == null || nguoiDung.OTP_Expire < DateTime.Now)
                {
                    return (false, "Ma OTP da het han. Vui long yeu cau ma moi", null);
                }

                // Kiem tra OTP co dung khong
                if (nguoiDung.OTP != otp)
                {
                    return (false, "Ma OTP khong chinh xac", null);
                }

                // OTP hop le - giu lai ca OTP va OTP_Expire trong database de xem lich su
                // Khong xoa gi ca, chi log thanh cong

                _logger.LogInformation($"OTP verified successfully for {phoneNumber}");

                return (true, "Dang nhap thanh cong", nguoiDung);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error verifying OTP for {phoneNumber}");
                return (false, "Da xay ra loi. Vui long thu lai", null);
            }
        }

        private string GenerateOtp()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}
