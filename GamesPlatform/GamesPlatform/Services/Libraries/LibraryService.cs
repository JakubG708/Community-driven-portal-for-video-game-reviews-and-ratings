using GamesPlatform.Data;
using GamesPlatform.Enums;
using GamesPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace GamesPlatform.Services.Libraries
{
    public class LibraryService : ILIbraryService
    {
        private readonly IDbContextFactory<ApplicationDbContext> dbFactory;

        public LibraryService(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            this.dbFactory = dbFactory;
        }
        public async Task AddGameToLibraryAsync(string userId, int gameId, Status status)
        {
            using var db = await dbFactory.CreateDbContextAsync();
            var game = await db.Games.FirstOrDefaultAsync(x => x.GameId == gameId);
            if (game == null)
            {
                throw new Exception("Game not found");
            }

            var library = await db.Libraries.Where(x=> x.UserId == userId).Include(x => x.LibGames).FirstOrDefaultAsync();

            if (library.LibGames.FirstOrDefault(x => x.GameId == game.GameId) != null)
            {
                throw new Exception("Game already in library");
            }

            var libGame = new LibGame
            {
                LibraryId = library.LibraryId,
                GameId = game.GameId,
                Status = status,
                AddedAt = DateTime.UtcNow,

            };

            db.Add(libGame);
            await db.SaveChangesAsync();
        }

        public async Task<ICollection<LibGame>> GetUserLibraryAsync(string userId)
        {
            using var db = await dbFactory.CreateDbContextAsync();
            var library = await db.Libraries.Where(x => x.UserId == userId).Include(x => x.LibGames).FirstOrDefaultAsync();

            if (library == null)
            {
                throw new InvalidOperationException($"Library not found for user with ID: {userId}");
            }

            return library.LibGames ?? new List<LibGame>();
        }

        public async Task RemoveGameFromLibraryAsync(string userId, int gameId)
        {
            using var db = await dbFactory.CreateDbContextAsync();
            var library = await db.Libraries.Where(x => x.UserId == userId).Include(x => x.LibGames).FirstOrDefaultAsync();
            
            if (library == null)
            {
                throw new InvalidOperationException($"Library not found for user with ID: {userId}");
            }

            var libGame = library.LibGames.FirstOrDefault(x => x.GameId == gameId);
            if (libGame == null)
            {
                throw new InvalidOperationException($"Game with ID {gameId} not found in user's library.");
            }

            db.LibGames.Remove(libGame);
            await db.SaveChangesAsync();
        }
    }
}
