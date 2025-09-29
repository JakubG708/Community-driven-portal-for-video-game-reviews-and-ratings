using System.ComponentModel.DataAnnotations;

namespace GamesPlatform.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }
}
