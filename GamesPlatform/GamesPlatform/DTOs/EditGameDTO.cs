using GamesPlatform.Enums;
using GamesPlatform.Models;
using System.ComponentModel.DataAnnotations;

public class EditGameDTO
{
    public int GameId { get; set; }
    public string Title { get; set; }
    public Tags Tag { get; set; }
    public DateTime ReleaseYear { get; set; }
    public string Developer { get; set; }
    public string Publisher { get; set; }
    public string Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? ThumbNailUrl { get; set; }

    public List<Platforms> Platforms { get; set; } = new List<Platforms>();
    }
