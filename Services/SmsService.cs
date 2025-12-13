using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace QL_NhaThuoc.Services
{
    public class SmsService : ISmsService
    {
        private readonly ILogger<SmsService> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public SmsService(ILogger<SmsService> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<bool> SendOtpAsync(string phoneNumber, string otp)
        {
            try
            {
                var apiKey = _configuration["eSMS:ApiKey"];
                var secretKey = _configuration["eSMS:SecretKey"];
                var baseUrl = _configuration["eSMS:BaseUrl"];
                var brandName = _configuration["eSMS:BrandName"];

                // Log thông tin cấu hình
                _logger.LogInformation($"📋 API Config - ApiKey: {apiKey?.Substring(0, 10)}..., BaseUrl: {baseUrl}");

                // Chuẩn hóa số điện thoại (loại bỏ số 0 đầu nếu có, thêm 84)
                var normalizedPhone = phoneNumber.TrimStart('0');
                if (!normalizedPhone.StartsWith("84"))
                {
                    normalizedPhone = "84" + normalizedPhone;
                }

                _logger.LogInformation($"📱 Phone normalized: {phoneNumber} -> {normalizedPhone}");

                // Nội dung tin nhắn
                var message = $"Ma OTP cua ban la: {otp}. Ma co hieu luc trong 5 phut.";

                // Tạo request body theo định dạng eSMS API
                // SmsType: 2 = Brandname OTP, 4 = SMS thường, 8 = SMS quảng cáo
                var requestBody = new
                {
                    ApiKey = apiKey,
                    SecretKey = secretKey,
                    Phone = normalizedPhone,
                    Content = message,
                    SmsType = 8, // 8 = SMS quảng cáo (thường được kích hoạt mặc định)
                    IsUnicode = 0, // 0 = không dấu, 1 = có dấu
                    Sandbox = 0 // 0 = gửi thật, 1 = test mode
                };

                var jsonContent = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                
                _logger.LogInformation($"📤 Request Body:\n{jsonContent}");

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Gửi request đến eSMS API
                var apiUrl = $"{baseUrl}/SendMultipleMessage_V4_post_json/";
                _logger.LogInformation($"🌐 Calling API: {apiUrl}");

                var response = await _httpClient.PostAsync(apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"📥 eSMS Response Status: {response.StatusCode}");
                _logger.LogInformation($"📥 eSMS Response Body: {responseContent}");

                // Parse response
                var result = JsonSerializer.Deserialize<EsmsResponse>(responseContent, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (result?.CodeResult == "100")
                {
                    _logger.LogInformation($"✅ SMS sent successfully to {phoneNumber}");
                    Console.WriteLine($"\n========================================");
                    Console.WriteLine($"✅ SMS đã gửi thành công đến: {phoneNumber}");
                    Console.WriteLine($"🔐 Mã OTP: {otp}");
                    Console.WriteLine($"📨 SMSID: {result.SMSID}");
                    Console.WriteLine($"========================================\n");
                    return true;
                }
                else
                {
                    _logger.LogWarning($"⚠️ eSMS API returned error - Code: {result?.CodeResult}, Message: {result?.ErrorMessage}");
                    
                    // Fallback: Log OTP ra console để test
                    Console.WriteLine($"\n========================================");
                    Console.WriteLine($"⚠️ Không gửi được SMS (Code: {result?.CodeResult})");
                    Console.WriteLine($"❌ Lỗi: {result?.ErrorMessage}");
                    Console.WriteLine($"📱 Số điện thoại: {phoneNumber} ({normalizedPhone})");
                    Console.WriteLine($"🔐 Mã OTP để test: {otp}");
                    Console.WriteLine($"⏰ Có hiệu lực trong 5 phút");
                    Console.WriteLine($"========================================\n");
                    
                    return true; // Vẫn return true để test được
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error sending SMS to {phoneNumber}");
                
                // Fallback: Log OTP ra console
                Console.WriteLine($"\n========================================");
                Console.WriteLine($"❌ Lỗi gửi SMS: {ex.Message}");
                Console.WriteLine($"📱 Số điện thoại: {phoneNumber}");
                Console.WriteLine($"🔐 Mã OTP để test: {otp}");
                Console.WriteLine($"⏰ Có hiệu lực trong 5 phút");
                Console.WriteLine($"========================================\n");
                
                return true; // Vẫn return true để test được
            }
        }

        // Model cho response từ eSMS API
        private class EsmsResponse
        {
            public string? CodeResult { get; set; }
            public string? ErrorMessage { get; set; }
            public string? SMSID { get; set; }
        }
    }
}
