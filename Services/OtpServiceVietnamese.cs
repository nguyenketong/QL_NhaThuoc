using Microsoft.Data.SqlClient;
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

        public async Task<(bool Success, string Message, string? PinId)> GenerateAndSendOtpAsync(string phoneNumber)
        {
            try
            {
                // Tìm người dùng theo số điện thoại
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
                        VaiTro = "User",
                        NgayTao = DateTime.Now
                    };
                    _context.NGUOI_DUNG.Add(nguoiDung);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation($"Auto-created new user for phone: {phoneNumber}");
                }

                // Gửi OTP qua Infobip (Infobip tự tạo mã OTP)
                var (success, pinId) = await _smsService.SendOtpAsync(phoneNumber);
                
                if (!success || string.IsNullOrEmpty(pinId))
                {
                    return (false, "Không thể gửi SMS. Vui lòng thử lại", null);
                }

                // Lưu PinId vào database để xác minh sau
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using var cmd = new SqlCommand("sp_NguoiDung_CapNhatOTP", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MaNguoiDung", nguoiDung.MaNguoiDung);
                    cmd.Parameters.AddWithValue("@OTP", pinId); // Lưu PinId thay vì OTP
                    cmd.Parameters.AddWithValue("@OTP_Expire", DateTime.Now.AddMinutes(5));
                    await cmd.ExecuteNonQueryAsync();
                }

                _logger.LogInformation($"OTP sent to {phoneNumber}, PinId: {pinId}");

                return (true, "Mã OTP đã được gửi đến số điện thoại của bạn", pinId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating OTP for {phoneNumber}");
                return (false, "Đã xảy ra lỗi. Vui lòng thử lại", null);
            }
        }

        public async Task<(bool Success, string Message, NguoiDung? NguoiDung)> VerifyOtpAsync(string phoneNumber, string otp)
        {
            try
            {
                // Tìm người dùng theo số điện thoại
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

                // Xác minh OTP qua Infobip API
                var pinId = nguoiDung.OTP; // PinId được lưu trong trường OTP
                var verified = await _smsService.VerifyOtpAsync(pinId, otp);

                if (!verified)
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
    }
}
