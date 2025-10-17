using GamesPlatform.DTOs;
using GamesPlatform.Models;

namespace GamesPlatform.Services.Ratings
{
    public interface IRatingService
    {
        Task AddRatingAsync(int gameId, string userId, RatingDTO rating);
        Task EditRatingAsync(int ratingId, RatingDTO rating);
        Task DeleteRatingAsync(int ratingId);
        Task<Rating> GetUserRatingAsync(int gameId, string userId);
        Task<ICollection<Rating>> GetUserRatingsAsync(string userId);
        Task<ICollection<Rating>> GetAllRatingsAsync();
    }
}
