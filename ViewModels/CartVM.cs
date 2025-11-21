using QL_NhaThuoc.Models;

namespace QL_NhaThuoc.ViewModels
{
    public class CartVM
    {
        public List<CartItem>? CartItems { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
