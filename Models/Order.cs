namespace QL_NhaThuoc.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public User? User { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";
        public string? ShippingAddress { get; set; }
        public string? PhoneNumber { get; set; }
        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}
