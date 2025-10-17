using GamesPlatform.Enums;
using Microsoft.AspNetCore.Identity;
using System;

namespace GamesPlatform.Models
{
    public class Library
    {
        public int LibraryId { get; set; }
        public string UserId { get; set; }
        public IdentityUser User{ get; set; }
        public ICollection<LibGame> LibGames { get; set; } = new List<LibGame>();
        
    }
}
