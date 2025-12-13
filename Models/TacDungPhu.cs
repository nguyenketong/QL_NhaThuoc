namespace QL_NhaThuoc.Models
{
    public class TacDungPhu
    {
        public int MaTacDungPhu { get; set; }
        public string TenTacDungPhu { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        
        public ICollection<CT_TacDungPhu>? CT_TacDungPhus { get; set; }
    }
}
