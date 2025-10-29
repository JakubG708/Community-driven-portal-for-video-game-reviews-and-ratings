using GamesPlatform.Data;
using GamesPlatform.DTOs;
using GamesPlatform.Enums;
using GamesPlatform.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GamesPlatform.Services.Games
{
    public class GamesService : IGamesService
    {
        private readonly IDbContextFactory<ApplicationDbContext> dbFactory;

        public GamesService(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            this.dbFactory = dbFactory;
        }

        public async Task AddGameAsync(GameDTO gameDTO)
        {
            using var db = await dbFactory.CreateDbContextAsync();
            var game = new Game
            {
                Title = gameDTO.Title,
                Tag = gameDTO.Tag,
                ReleaseYear = DateTime.SpecifyKind(gameDTO.ReleaseYear, DateTimeKind.Utc),
                Developer = gameDTO.Developer,
                Publisher = gameDTO.Publisher,
                Description = gameDTO.Description,
                ImageUrl = gameDTO.ImageUrl,
                ThumbNailUrl = gameDTO.ThumbNailUrl,
                Platforms = new List<Platform>()
            };

            db.Games.Add(game);
            await db.SaveChangesAsync();
        }

        public async Task EditGameAsync(int id, EditGameDTO gameDTO)
        {
            using var db = await dbFactory.CreateDbContextAsync();
            var game = await db.Games.Include(g => g.Platforms).FirstOrDefaultAsync(g => g.GameId == id);

            if (game == null) throw new Exception("Game not found");

            game.Title = gameDTO.Title;
            game.Tag = gameDTO.Tag;
            game.ReleaseYear = DateTime.SpecifyKind(gameDTO.ReleaseYear, DateTimeKind.Utc);
            game.Developer = gameDTO.Developer;
            game.Publisher = gameDTO.Publisher;
            game.Description = gameDTO.Description;
            game.ImageUrl = gameDTO.ImageUrl;
            game.ThumbNailUrl = gameDTO.ThumbNailUrl;

            game.Platforms.Clear();
            var selectedPlatforms = await db.Platforms
                                           .Where(p => gameDTO.SelectedPlatformIds.Contains(p.PlatformId))
                                           .ToListAsync();
            foreach (var p in selectedPlatforms)
            {
                game.Platforms.Add(p);
            }

            await db.SaveChangesAsync();
        }

        public async Task<GameDTO> GetGameByIdAsync(int id)
        {
            using var db = await dbFactory.CreateDbContextAsync();

            var game = await db.Games
                               .Include(x => x.Reviews)
                                   .ThenInclude(r => r.User)
                               .Include(x => x.Ratings)
                               .Include(x => x.Platforms)
                               .FirstOrDefaultAsync(x => x.GameId == id);

            if (game == null) throw new Exception("Game not found");

            var gameDTO = new GameDTO
            {
                GameId = game.GameId,
                Title = game.Title,
                Tag = game.Tag,
                ReleaseYear = game.ReleaseYear,
                Developer = game.Developer,
                Publisher = game.Publisher,
                Description = game.Description,
                ImageUrl = game.ImageUrl,
                ThumbNailUrl = game.ThumbNailUrl,
                AvgGameplayRating = game.Ratings.Count > 0 ? (int)game.Ratings.Average(x => x.Gameplay) : 0,
                AvgGraphicsRating = game.Ratings.Count > 0 ? (int)game.Ratings.Average(x => x.Graphics) : 0,
                AvgOptimizationRating = game.Ratings.Count > 0 ? (int)game.Ratings.Average(x => x.Optimization) : 0,
                AvgStoryRating = game.Ratings.Count > 0 ? (int)game.Ratings.Average(x => x.Story) : 0,
                Reviews = game.Reviews.ToList(),
                Platforms = game.Platforms.ToList()
            };

            return gameDTO;
        }

        public async Task<ICollection<Game>> GetGamesAsync()
        {
            using var db =await dbFactory.CreateDbContextAsync();

            var games = await db.Games.ToListAsync<Game>();

            return games;
        }
    }
}
