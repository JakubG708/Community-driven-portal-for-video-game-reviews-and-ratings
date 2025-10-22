using GamesPlatform.DTOs;
using GamesPlatform.Models;

namespace GamesPlatform.Services.Reviews
{
    public interface IReviewsService
    {
        Task AddReviewAsync(int gameId, string userId, string content);
        Task EditReviewAsync(int reviewId, string content);
        Task DeleteReviewAsync(int reviewId);
        Task<ICollection<Review>> GetReviewsAsync();
        Task<Review> GetReviewByIdAsync(int reviewId);
        Task<ICollection<ReviewDTO>> GetReviewDTOsAsync();

    }
}
