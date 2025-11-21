using QL_NhaThuoc.Models;

namespace QL_NhaThuoc.ViewModels
{
    public class ProductVM
    {
        public Product? Product { get; set; }
        public List<Product>? RelatedProducts { get; set; }
        public List<Ingredient>? Ingredients { get; set; }
        public List<SideEffect>? SideEffects { get; set; }
    }
}
