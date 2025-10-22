using GamesPlatform.Models;
using Microsoft.AspNetCore.Identity;

namespace GamesPlatform.DTOs
{
    public class ReviewDTO
    {
        public int ReviewId { get; set; }
        public string UserId { get; set; }
        public string UserName{ get; set; }
        public int GameId { get; set; }
        public string GameTitle { get; set; }
        public string Description { get; set; }
    }
}
