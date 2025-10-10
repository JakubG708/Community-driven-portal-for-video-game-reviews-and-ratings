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
        public ICollection<Platform> Platforms { get; set; } = new List<Platform>();

        public string Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? ThumbNailUrl { get; set; }
    }
}
