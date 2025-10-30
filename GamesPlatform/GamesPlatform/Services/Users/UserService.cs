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

        public async Task<ICollection<IdentityUser>> GetUsersAsync(string? query = null, string? filterBy = null)
        {
            using var db = await dbFactory.CreateDbContextAsync();
            var usersQuery = db.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var qLower = query.ToLowerInvariant();
                var by = (filterBy ?? "username").ToLowerInvariant();

                if (by == "email")
                {
                    usersQuery = usersQuery.Where(u => u.Email != null && u.Email.ToLower().Contains(qLower));
                }
                else
                {
                    usersQuery = usersQuery.Where(u => u.UserName != null && u.UserName.ToLower().Contains(qLower));
                }
            }

            var users = await usersQuery.ToListAsync();
            return users;
        }
    }
}
