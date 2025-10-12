using GamesPlatform.Models;

namespace GamesPlatform.Services.Games
{
    public interface IGamesService
    {
        Task<ICollection<Game>> GetGamesAsync();
    }
}
