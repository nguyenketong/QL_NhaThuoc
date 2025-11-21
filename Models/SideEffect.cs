namespace QL_NhaThuoc.Models
{
    public class SideEffect
    {
        public int SideEffectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ICollection<ProductSideEffect>? ProductSideEffects { get; set; }
    }
}
