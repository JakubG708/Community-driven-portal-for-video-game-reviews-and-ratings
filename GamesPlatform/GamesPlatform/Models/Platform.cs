using GamesPlatform.Enums;
using System.Reflection.Metadata.Ecma335;

namespace GamesPlatform.Models
{
    public class Platform
    {
        public int PlatformId { get; set; }
        public Platforms PlatformName { get; set; }
        public int GameId { get; set; }
        public Game Game { get; set; }

    }
}
