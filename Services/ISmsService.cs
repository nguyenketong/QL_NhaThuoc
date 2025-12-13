namespace QL_NhaThuoc.Services
{
    public interface ISmsService
    {
        Task<bool> SendOtpAsync(string phoneNumber, string otp);
    }
}
