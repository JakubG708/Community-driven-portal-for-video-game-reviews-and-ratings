using GamesPlatform.Enums;
using GamesPlatform.Models;

namespace GamesPlatform.Services.Libraries
{
    public interface ILIbraryService
    {
        Task AddGameToLibraryAsync(string userId, int gameId, Status status);
        Task RemoveGameFromLibraryAsync(string userId, int gameId);
        Task<ICollection<LibGame>> GetUserLibraryAsync(string userId);
        Task<int> GetLibraryId(string userId);
        Task CreateLibraryAsync(string userId);
    }
}
