using GamesPlatform.Data;
using GamesPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace GamesPlatform.Services.Platforms
{
    public class PlatformService : IPlatformService
    {
        private readonly IDbContextFactory<ApplicationDbContext> dbFactory;

        public PlatformService(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            this.dbFactory = dbFactory;
        }
        public async Task<ICollection<Platform>> GetPlatformsAsync()
        {
            using var db = await dbFactory.CreateDbContextAsync();
            return await db.Platforms.ToListAsync();
        }
    }
}
