using GamesPlatform.Models;
using Microsoft.AspNetCore.Identity;

namespace GamesPlatform.Services.Users
{
    public interface IUserService
    {
        Task<ICollection<IdentityUser>> GetUsersAsync(string? query = null, string? filterBy = null);
        Task<IdentityUser?> GetUserByIdAsync(string id);
    }
}
