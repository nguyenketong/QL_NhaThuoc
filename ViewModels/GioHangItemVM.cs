namespace QL_NhaThuoc.ViewModels
{
    public class GioHangItem
    {
        public int MaThuoc { get; set; }
        public string TenThuoc { get; set; } = "";
        public string? HinhAnh { get; set; }
        public decimal GiaBan { get; set; }
        public int SoLuong { get; set; }
        public int SoLuongTon { get; set; } // Số lượng còn trong kho
        public bool NgungKinhDoanh { get; set; } // Sản phẩm ngừng kinh doanh
        public bool DuocChon { get; set; } = true; // Mặc định được chọn
        public decimal ThanhTien => GiaBan * SoLuong;
        
        // Kiểm tra sắp hết hàng (≤5)
        public bool SapHetHang => !NgungKinhDoanh && SoLuongTon > 0 && SoLuongTon <= 5;
        public bool HetHang => SoLuongTon <= 0;
        public bool KhongKhaDung => HetHang || NgungKinhDoanh; // Không thể mua
    }
}
