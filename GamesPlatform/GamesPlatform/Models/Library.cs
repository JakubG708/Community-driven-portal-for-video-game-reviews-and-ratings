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
        public int GameId { get; set; }
        public Game Game { get; set; }
        public Status Status { get; set; } 
        public DateTime AddedAt { get; set; }
    }
}
