using Microsoft.AspNetCore.Identity;

namespace GamesPlatform.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public User? User { get; set; }
        public Admin? Admin { get; set; }
    }
}
