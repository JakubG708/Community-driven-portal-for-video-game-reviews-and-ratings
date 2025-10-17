using GamesPlatform.Data;
using GamesPlatform.DTOs;
using GamesPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace GamesPlatform.Services.Ratings
{
    public class RatingService : IRatingService
    {
        private readonly IDbContextFactory<ApplicationDbContext> dbFactory;

        public RatingService(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            this.dbFactory = dbFactory;
        }
        public async Task AddRatingAsync(int gameId, string userId, RatingDTO rating)
        {
            using var db = await dbFactory.CreateDbContextAsync();
            var game = await db.Games.FirstOrDefaultAsync(x => x.GameId == gameId);
            if (game == null)
            {
                throw new Exception("Game not found");
            }
            var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            var newRating = new Rating
            {
                GameId = gameId,
                UserId = userId,
                Gameplay = rating.Gameplay,
                Graphics = rating.Graphics,
                Optimization = rating.Optimization,
                Story = rating.Story,
            };

            db.Ratings.Add(newRating);
            await db.SaveChangesAsync();
        }

        public async Task DeleteRatingAsync(int ratingId)
        {
            using var db = await dbFactory.CreateDbContextAsync();
            var rating = await db.Ratings.FirstOrDefaultAsync(r => r.RatingId == ratingId);
            if (rating == null)
            {
                throw new Exception("Rating not found");
            }

            db.Ratings.Remove(rating);
            await db.SaveChangesAsync();
        }

        public async Task EditRatingAsync(int ratingId, RatingDTO rating)
        {
            using var db = dbFactory.CreateDbContext();
            var existingRating = db.Ratings.FirstOrDefault(r => r.RatingId == ratingId);
            if (existingRating == null)
            {
                throw new Exception("Rating not found");
            }
            existingRating.Gameplay = rating.Gameplay;
            existingRating.Graphics = rating.Graphics;
            existingRating.Optimization = rating.Optimization;
            existingRating.Story = rating.Story;

            db.Ratings.Update(existingRating);
            await db.SaveChangesAsync();
        }

        public async Task<ICollection<Rating>> GetAllRatingsAsync()
        {
            using var db = dbFactory.CreateDbContext();
            return await db.Ratings.ToListAsync();
        }

        public async Task<Rating> GetUserRatingAsync(int gameId, string userId)
        {
            using var db = await dbFactory.CreateDbContextAsync();
            var rating = await db.Ratings.FirstOrDefaultAsync(r => r.GameId == gameId && r.UserId == userId);
            return rating;
        }

        public async Task<ICollection<Rating>> GetUserRatingsAsync(string userId)
        {
            using var db = dbFactory.CreateDbContext();
            var ratings = await db.Ratings.Where(r => r.UserId == userId).ToListAsync();
            return ratings;
        }
    }
}
