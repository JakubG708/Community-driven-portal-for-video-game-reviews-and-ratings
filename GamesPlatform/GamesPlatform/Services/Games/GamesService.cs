using GamesPlatform.Data;
using GamesPlatform.DTOs;
using GamesPlatform.Enums;
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

        public async Task<GameDTO> GetGameByIdAsync(int id)
        {
            using var db = dbFactory.CreateDbContext();

            var game = await db.Games.Include(x => x.Reviews).Include(x => x.Ratings).Include(x=>x.Platforms).FirstOrDefaultAsync(x=>x.GameId == id);
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
            using var db = dbFactory.CreateDbContext();

            var games = await db.Games.ToListAsync<Game>();

            return games;
        }
    }
}
