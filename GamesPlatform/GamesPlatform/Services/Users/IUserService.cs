using GamesPlatform.Models;
using Microsoft.AspNetCore.Identity;

namespace GamesPlatform.Services.Users
{
    public interface IUserService
    {
        Task<ICollection<IdentityUser>> GetUsersAsync();
        Task<IdentityUser?> GetUserByIdAsync(string id);
    }
}
