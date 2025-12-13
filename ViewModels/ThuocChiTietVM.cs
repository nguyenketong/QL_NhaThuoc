using QL_NhaThuoc.Models;

namespace QL_NhaThuoc.ViewModels
{
    /// <summary>
    /// ViewModel cho trang chi tiết thuốc với đầy đủ thông tin
    /// </summary>
    public class ThuocChiTietVM
    {
        // Thông tin cơ bản
        public int MaThuoc { get; set; }
        public string TenThuoc { get; set; } = string.Empty;
        public decimal? GiaBan { get; set; }
        public string? DonViTinh { get; set; }
        public string? MoTa { get; set; }
        public string? HinhAnh { get; set; }

        // Thông tin nhóm và thương hiệu
        public string? TenNhomThuoc { get; set; }
        public string? TenThuongHieu { get; set; }
        public string? TenNuocSX { get; set; }

        // Thành phần
        public List<ThanhPhanInfo> ThanhPhans { get; set; } = new();

        // Đối tượng sử dụng
        public List<string> DoiTuongSuDungs { get; set; } = new();

        // Tác dụng phụ
        public List<TacDungPhuInfo> TacDungPhus { get; set; } = new();

        // Thuốc liên quan (cùng nhóm)
        public List<ThuocLienQuanVM> ThuocLienQuans { get; set; } = new();
    }

    public class ThanhPhanInfo
    {
        public string TenThanhPhan { get; set; } = string.Empty;
        public string? HamLuong { get; set; }
        public string? GhiChu { get; set; }
    }

    public class TacDungPhuInfo
    {
        public string TenTacDungPhu { get; set; } = string.Empty;
        public string? MucDo { get; set; }
        public string? MoTa { get; set; }
    }

    public class ThuocLienQuanVM
    {
        public int MaThuoc { get; set; }
        public string TenThuoc { get; set; } = string.Empty;
        public decimal? GiaBan { get; set; }
        public string? HinhAnh { get; set; }
    }
}
