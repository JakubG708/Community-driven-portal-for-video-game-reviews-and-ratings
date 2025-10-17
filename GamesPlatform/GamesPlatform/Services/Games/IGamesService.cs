using GamesPlatform.DTOs;
using GamesPlatform.Models;

namespace GamesPlatform.Services.Games
{
    public interface IGamesService
    {
        Task<ICollection<Game>> GetGamesAsync();
        Task<GameDTO> GetGameByIdAsync(int id);
        Task EditGameAsync(int id, GameDTO gameDTO);
    }
}
