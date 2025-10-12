using GamesPlatform.Data;
using GamesPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace GamesPlatform.Services.Games
{
    public class GamesService : IGamesService
    {
        private readonly IDbContextFactory<ApplicationDbContext> dbFactory;

        public GamesService(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            this.dbFactory = dbFactory;
        }
        public async Task<ICollection<Game>> GetGamesAsync()
        {
            using var db = dbFactory.CreateDbContext();

            var games = await db.Games.ToListAsync<Game>();

            return games;
        }
    }
}
