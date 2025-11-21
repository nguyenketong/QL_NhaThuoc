namespace QL_NhaThuoc.Models
{
    public class CartItem
    {
        public int CartItemId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public int Quantity { get; set; }
        public DateTime AddedDate { get; set; }
    }
}
