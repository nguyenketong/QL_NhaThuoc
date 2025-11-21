namespace QL_NhaThuoc.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string? ImageUrl { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public int BrandId { get; set; }
        public Brand? Brand { get; set; }
        public int CountryId { get; set; }
        public Country? Country { get; set; }
        public int UsageObjectId { get; set; }
        public UsageObject? UsageObject { get; set; }
        public ICollection<ProductIngredient>? ProductIngredients { get; set; }
        public ICollection<ProductSideEffect>? ProductSideEffects { get; set; }
    }
}
