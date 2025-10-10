using Microsoft.AspNetCore.Identity;

namespace GamesPlatform.Models
{
    public class Review
    {
        public int ReviewId { get; set; }
        public string UserId { get; set; }
        public IdentityUser User{ get; set; }
        public int GameId { get; set; }
        public Game Game { get; set; }
        public string Description { get; set; }
    }
}
