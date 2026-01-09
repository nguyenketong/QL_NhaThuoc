using System.Security.Cryptography;

namespace QL_NhaThuoc.Services
{
    /// <summary>
    /// Service hash mật khẩu sử dụng PBKDF2 (Password-Based Key Derivation Function 2)
    /// - Thuật toán được khuyến nghị bởi NIST
    /// - Tự động tạo salt ngẫu nhiên
    /// - Chống brute-force attack
    /// </summary>
    public class PasswordService
    {
        // Cấu hình bảo mật
        private const int SaltSize = 16;        // 128 bit
        private const int HashSize = 32;        // 256 bit
        private const int Iterations = 100000;  // Số vòng lặp (OWASP khuyến nghị >= 100,000)

        /// <summary>
        /// Hash mật khẩu với salt ngẫu nhiên
        /// </summary>
        /// <param name="password">Mật khẩu gốc</param>
        /// <returns>Chuỗi hash dạng: {iterations}.{salt}.{hash}</returns>
        public string HashPassword(string password)
        {
            // Tạo salt ngẫu nhiên
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

            // Hash mật khẩu với PBKDF2
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize
            );

            // Kết hợp: iterations.salt.hash (để có thể thay đổi iterations sau này)
            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        /// <summary>
        /// Xác thực mật khẩu
        /// </summary>
        /// <param name="password">Mật khẩu người dùng nhập</param>
        /// <param name="hashedPassword">Mật khẩu đã hash trong database</param>
        /// <returns>True nếu khớp</returns>
        public bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                // Tách các phần
                var parts = hashedPassword.Split('.');
                if (parts.Length != 3)
                    return false;

                var iterations = int.Parse(parts[0]);
                var salt = Convert.FromBase64String(parts[1]);
                var storedHash = Convert.FromBase64String(parts[2]);

                // Hash mật khẩu nhập vào với cùng salt và iterations
                byte[] computedHash = Rfc2898DeriveBytes.Pbkdf2(
                    password,
                    salt,
                    iterations,
                    HashAlgorithmName.SHA256,
                    storedHash.Length
                );

                // So sánh an toàn (constant-time comparison để chống timing attack)
                return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
            }
            catch
            {
                return false;
            }
        }
    }
}
