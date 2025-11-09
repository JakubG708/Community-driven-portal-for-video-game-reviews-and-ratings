using GamesPlatform.Enums;
using System;

namespace GamesPlatform.Models
{
    public class Game
    {
        public int GameId { get; set; }
        public string Title { get; set; }
        public Tags Tag { get; set; }
        public DateTime ReleaseYear { get; set; }
        public string Developer { get; set; }
        public string Publisher { get; set; }
        public string Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? ThumbNailUrl { get; set; }
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
        public List<Platforms> Platforms { get; set; } = new List<Platforms>();

    }
}
