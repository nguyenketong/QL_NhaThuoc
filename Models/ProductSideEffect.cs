namespace QL_NhaThuoc.Models
{
    public class ProductSideEffect
    {
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public int SideEffectId { get; set; }
        public SideEffect? SideEffect { get; set; }
    }
}
