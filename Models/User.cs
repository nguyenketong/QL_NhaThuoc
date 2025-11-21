using Microsoft.AspNetCore.Identity;

namespace QL_NhaThuoc.Models
{
    public class User : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public ICollection<Order>? Orders { get; set; }
    }
}
