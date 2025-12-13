namespace QL_NhaThuoc.Models
{
    public class NguoiDung
    {
        public int MaNguoiDung { get; set; }
        public string? HoTen { get; set; }
        public string SoDienThoai { get; set; } = string.Empty;
        public string? DiaChi { get; set; }
        public string? OTP { get; set; }
        public DateTime? OTP_Expire { get; set; }
        public DateTime? NgayTao { get; set; }
        
        // Phân quyền: "User" hoặc "Admin"
        public string VaiTro { get; set; } = "User";
        
        public ICollection<DonHang>? DonHangs { get; set; }
    }
}
