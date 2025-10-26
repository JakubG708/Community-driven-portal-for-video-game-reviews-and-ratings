using GamesPlatform.Models;

namespace GamesPlatform.DTOs
{
    public class UserDetailsDTO
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public int? LibraryId { get; set; }
        public ICollection<LibGame>? LibGames { get; set; }
        public ICollection<Review>? Reviews { get; set; }
        public ICollection<Rating>? Ratings { get; set; }


        }
}
