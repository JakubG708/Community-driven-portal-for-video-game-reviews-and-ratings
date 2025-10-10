using Microsoft.AspNetCore.Identity;

namespace GamesPlatform.Models
{
//gameplay(np. 1–10)
//graphics(1–10)
//optimization(1–10)
//story(1–10)

    public class Rating
    {
        public int RatingId { get; set; }
        public string UserId { get; set; }
        public IdentityUser User{ get; set; }
        public int GameId { get; set; }
        public Game Game { get; set; }
        public int Gameplay { get; set; }
        public int Graphics { get; set; }
        public int Optimization { get; set; }
        public int Story { get; set; }
    }
}
