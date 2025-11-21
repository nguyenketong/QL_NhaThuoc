using QL_NhaThuoc.Models;

namespace QL_NhaThuoc.ViewModels
{
    public class OrderVM
    {
        public string ShippingAddress { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public List<CartItem>? CartItems { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
