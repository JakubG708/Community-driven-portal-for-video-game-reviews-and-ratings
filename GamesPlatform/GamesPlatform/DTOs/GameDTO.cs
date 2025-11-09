using GamesPlatform.Enums;
using GamesPlatform.Models;

namespace GamesPlatform.DTOs
{
    public class GameDTO
    {
        public int GameId { get; set; }
        public string Title { get; set; }
        public Tags Tag { get; set; }
        public DateTime ReleaseYear { get; set; } = DateTime.UtcNow;
        public string Developer { get; set; }
        public string Publisher { get; set; }
        public string Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? ThumbNailUrl { get; set; }
        public int? AvgGameplayRating { get; set; }
        public int? AvgGraphicsRating { get; set; }
        public int? AvgOptimizationRating { get; set; }
        public int? AvgStoryRating { get; set; }
        public List<Review>? Reviews { get; set; }
        public List<Platforms>? Platforms { get; set; }
    }
}
