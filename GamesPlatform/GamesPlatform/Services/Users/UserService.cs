using GamesPlatform.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GamesPlatform.Services.Users
{
    public class UserService : IUserService
    {
        private readonly IDbContextFactory<ApplicationDbContext> dbFactory;

        public UserService(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            this.dbFactory = dbFactory;
        }

        public async Task<IdentityUser?> GetUserByIdAsync(string id)
        {
            using var db = await dbFactory.CreateDbContextAsync();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
            return user;
        }

        public async Task<ICollection<IdentityUser>> GetUsersAsync()
        {
            using var db = await dbFactory.CreateDbContextAsync();
            var users = await db.Users.ToListAsync<IdentityUser>();
            return users;
        }
    }
}
