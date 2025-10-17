using GamesPlatform.Enums;

namespace GamesPlatform.Models
{
    public class LibGame
    {
        public int LibGameId { get; set; }
        public int LibraryId { get; set; }
        public Library Library { get; set; }
        public int GameId { get; set; }
        public Game Game { get; set; }
        public Status Status { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
