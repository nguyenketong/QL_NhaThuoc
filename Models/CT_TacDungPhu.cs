namespace QL_NhaThuoc.Models
{
    public class CT_TacDungPhu
    {
        public int MaThuoc { get; set; }
        public int MaTacDungPhu { get; set; }
        public string? MucDo { get; set; }
        
        // Navigation properties
        public Thuoc? Thuoc { get; set; }
        public TacDungPhu? TacDungPhu { get; set; }
    }
}
