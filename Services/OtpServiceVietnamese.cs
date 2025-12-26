using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QL_NhaThuoc.Data;
using QL_NhaThuoc.Models;
using System.Data;

namespace QL_NhaThuoc.Services
{
    public class OtpServiceVietnamese
    {
        private readonly QL_NhaThuocDbContext _context;
        private readonly ISmsService _smsService;
        private readonly ILogger<OtpServiceVietnamese> _logger;
        private readonly string _connectionString;

        public OtpServiceVietnamese(QL_NhaThuocDbContext context, ISmsService smsService, 
            ILogger<OtpServiceVietnamese> logger, IConfiguration configuration)
        {
            _context = context;
            _smsService = smsService;
            _logger = logger;
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<(bool Success, string Message)> GenerateAndSendOtpAsync(string phoneNumber)
        {
            try
            {
                // Tìm người dùng theo số điện thoại - Stored Procedure
                NguoiDung? nguoiDung = null;
                
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using var cmd = new SqlCommand("sp_NguoiDung_TimTheoDienThoai", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SoDienThoai", phoneNumber);

                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        nguoiDung = new NguoiDung
                        {
                            MaNguoiDung = reader.GetInt32(reader.GetOrdinal("MaNguoiDung")),
                            SoDienThoai = reader.IsDBNull(reader.GetOrdinal("SoDienThoai")) ? null : reader.GetString(reader.GetOrdinal("SoDienThoai")),
                            HoTen = reader.IsDBNull(reader.GetOrdinal("HoTen")) ? null : reader.GetString(reader.GetOrdinal("HoTen")),
                            DiaChi = reader.IsDBNull(reader.GetOrdinal("DiaChi")) ? null : reader.GetString(reader.GetOrdinal("DiaChi")),
                            VaiTro = reader.IsDBNull(reader.GetOrdinal("VaiTro")) ? null : reader.GetString(reader.GetOrdinal("VaiTro")),
                            OTP = reader.IsDBNull(reader.GetOrdinal("OTP")) ? null : reader.GetString(reader.GetOrdinal("OTP")),
                            OTP_Expire = reader.IsDBNull(reader.GetOrdinal("OTP_Expire")) ? null : reader.GetDateTime(reader.GetOrdinal("OTP_Expire")),
                            NgayTao = reader.IsDBNull(reader.GetOrdinal("NgayTao")) ? null : reader.GetDateTime(reader.GetOrdinal("NgayTao"))
                        };
                    }
                }

                // Nếu chưa có tài khoản, tự động tạo mới
                if (nguoiDung == null)
                {
                    nguoiDung = new NguoiDung
                    {
                        SoDienThoai = phoneNumber,
                        HoTen = phoneNumber,
                        VaiTro = "KhachHang",
                        NgayTao = DateTime.Now
                    };
                    _context.NGUOI_DUNG.Add(nguoiDung);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation($"Auto-created new user for phone: {phoneNumber}");
                }

                // Tạo mã OTP 6 chữ số
                var otp = GenerateOtp();
                var otpExpiry = DateTime.Now.AddMinutes(5);

                // Cập nhật OTP vào database - Stored Procedure
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using var cmd = new SqlCommand("sp_NguoiDung_CapNhatOTP", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MaNguoiDung", nguoiDung.MaNguoiDung);
                    cmd.Parameters.AddWithValue("@OTP", otp);
                    cmd.Parameters.AddWithValue("@OTP_Expire", otpExpiry);
                    await cmd.ExecuteNonQueryAsync();
                }

                // Gửi OTP qua SMS
                var smsSent = await _smsService.SendOtpAsync(phoneNumber, otp);
                if (!smsSent)
                {
                    return (false, "Không thể gửi SMS. Vui lòng thử lại");
                }

                _logger.LogInformation($"OTP generated for {phoneNumber}: {otp}, expires at {otpExpiry}");

                return (true, "Mã OTP đã được gửi đến số điện thoại của bạn");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating OTP for {phoneNumber}");
                return (false, "Đã xảy ra lỗi. Vui lòng thử lại");
            }
        }

        public async Task<(bool Success, string Message, NguoiDung? NguoiDung)> VerifyOtpAsync(string phoneNumber, string otp)
        {
            try
            {
                // Tìm người dùng theo số điện thoại - Stored Procedure
                NguoiDung? nguoiDung = null;
                
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using var cmd = new SqlCommand("sp_NguoiDung_TimTheoDienThoai", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SoDienThoai", phoneNumber);

                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        nguoiDung = new NguoiDung
                        {
                            MaNguoiDung = reader.GetInt32(reader.GetOrdinal("MaNguoiDung")),
                            SoDienThoai = reader.IsDBNull(reader.GetOrdinal("SoDienThoai")) ? null : reader.GetString(reader.GetOrdinal("SoDienThoai")),
                            HoTen = reader.IsDBNull(reader.GetOrdinal("HoTen")) ? null : reader.GetString(reader.GetOrdinal("HoTen")),
                            DiaChi = reader.IsDBNull(reader.GetOrdinal("DiaChi")) ? null : reader.GetString(reader.GetOrdinal("DiaChi")),
                            VaiTro = reader.IsDBNull(reader.GetOrdinal("VaiTro")) ? null : reader.GetString(reader.GetOrdinal("VaiTro")),
                            OTP = reader.IsDBNull(reader.GetOrdinal("OTP")) ? null : reader.GetString(reader.GetOrdinal("OTP")),
                            OTP_Expire = reader.IsDBNull(reader.GetOrdinal("OTP_Expire")) ? null : reader.GetDateTime(reader.GetOrdinal("OTP_Expire")),
                            NgayTao = reader.IsDBNull(reader.GetOrdinal("NgayTao")) ? null : reader.GetDateTime(reader.GetOrdinal("NgayTao"))
                        };
                    }
                }

                if (nguoiDung == null)
                {
                    return (false, "Số điện thoại không tồn tại", null);
                }

                if (string.IsNullOrEmpty(nguoiDung.OTP))
                {
                    return (false, "Vui lòng yêu cầu mã OTP mới", null);
                }

                if (nguoiDung.OTP_Expire == null || nguoiDung.OTP_Expire < DateTime.Now)
                {
                    return (false, "Mã OTP đã hết hạn. Vui lòng yêu cầu mã mới", null);
                }

                if (nguoiDung.OTP != otp)
                {
                    return (false, "Mã OTP không chính xác", null);
                }

                _logger.LogInformation($"OTP verified successfully for {phoneNumber}");

                return (true, "Đăng nhập thành công", nguoiDung);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error verifying OTP for {phoneNumber}");
                return (false, "Đã xảy ra lỗi. Vui lòng thử lại", null);
            }
        }

        private string GenerateOtp()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}
