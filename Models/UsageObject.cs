namespace QL_NhaThuoc.Models
{
    public class UsageObject
    {
        public int UsageObjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ICollection<Product>? Products { get; set; }
    }
}
