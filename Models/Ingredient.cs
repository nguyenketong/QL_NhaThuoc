namespace QL_NhaThuoc.Models
{
    public class Ingredient
    {
        public int IngredientId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ICollection<ProductIngredient>? ProductIngredients { get; set; }
    }
}
