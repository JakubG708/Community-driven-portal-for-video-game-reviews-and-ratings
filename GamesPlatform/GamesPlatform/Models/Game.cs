using GamesPlatform.Enums;
using System.ComponentModel.DataAnnotations;

namespace GamesPlatform.Models
{
    public class Game
    {
        [Key]
        public int GameId { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public Genre Genre { get; set; }
        [Required]
        public string Platform { get; set; }
        [Required]
        public DateTime ReleaseDate { get; set; }
        [Required]
        public string Developer { get; set; }
        [Required]
        public string Publisher { get; set; }
        [Required]
        public string Description { get; set; }

        //nie wiem czy dodać jeszcze string na cover image
    }
}
