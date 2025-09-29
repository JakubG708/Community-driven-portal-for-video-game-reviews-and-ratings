using GamesPlatform.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace GamesPlatform.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
    }
}
