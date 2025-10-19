using GamesPlatform.Models;

namespace GamesPlatform.Services.Platforms
{
    public interface IPlatformService
    {
        Task<ICollection<Platform>> GetPlatformsAsync();
    }
}
