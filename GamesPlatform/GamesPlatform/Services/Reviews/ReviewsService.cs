using GamesPlatform.Data;
using GamesPlatform.DTOs;
using GamesPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace GamesPlatform.Services.Reviews
{
    public class ReviewsService : IReviewsService
    {
        private readonly IDbContextFactory<ApplicationDbContext> dbFactory;

        public ReviewsService(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            this.dbFactory = dbFactory;
        }

        public async Task AddReviewAsync(int gameId, string userId, string content)
        {
            using var db = await dbFactory.CreateDbContextAsync();
            var review = new Review
            {
                GameId = gameId,
                UserId = userId,
                Description = content,
            };
            db.Reviews.Add(review);
            await db.SaveChangesAsync();
        }

        public async Task DeleteReviewAsync(int reviewId)
        {
            using var db = await dbFactory.CreateDbContextAsync();
            var review = await db.Reviews.FirstOrDefaultAsync(r => r.ReviewId == reviewId);
            if (review == null)
            {
                throw new Exception("Review not found");
            }
            db.Reviews.Remove(review);
            await db.SaveChangesAsync();
        }

        public async Task EditReviewAsync(int reviewId, string content)
        {
            using var db = await dbFactory.CreateDbContextAsync();
            var review = await db.Reviews.FirstOrDefaultAsync(r => r.ReviewId == reviewId);
            if (review == null)
            {
                throw new Exception("Review not found");
            }
            review.Description = content;
            db.Reviews.Update(review);
            await db.SaveChangesAsync();
        }

        public async Task<Review> GetReviewByIdAsync(int reviewId)
        {
            using var db = await dbFactory.CreateDbContextAsync();

            var review = await db.Reviews
                .Include(r => r.Game)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId);

            if (review == null)
            {
                throw new Exception("Review not found");
            }

            return review;
        }

        public async Task<ICollection<ReviewDTO>> GetReviewDTOsAsync()
        {
            using var db = await dbFactory.CreateDbContextAsync();

            var reviews = await db.Reviews
                .Include(r => r.Game)
                .Include(r => r.User)
                .Select(r => new ReviewDTO
                {
                    ReviewId = r.ReviewId,
                    UserId = r.UserId,
                    UserName = r.User != null ? r.User.UserName : r.UserId,
                    GameId = r.GameId,
                    GameTitle = r.Game != null ? r.Game.Title : $"GameId: {r.GameId}",
                    Description = r.Description
                })
                .ToListAsync();

            return reviews;
        }

        public async Task<ICollection<Review>> GetReviewsAsync()
        {
            using var db = await dbFactory.CreateDbContextAsync();
            var reviews = await db.Reviews.ToListAsync<Review>();
            return reviews;
        }

        public async Task<ICollection<Review>> GetUserReviewsAsync(string userId)
        {
            using var db = await dbFactory.CreateDbContextAsync();
            var reviews = await db.Reviews
                .Where(r => r.UserId == userId)
                .Include(r => r.Game)
                .Include(r => r.User)
                .ToListAsync();
            return reviews;
        }
    }
}
