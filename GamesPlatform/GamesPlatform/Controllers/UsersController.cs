using GamesPlatform.DTOs;
using GamesPlatform.Models;
using GamesPlatform.Services.Libraries;
using GamesPlatform.Services.Ratings;
using GamesPlatform.Services.Reviews;
using GamesPlatform.Services.Users;
using Microsoft.AspNetCore.Mvc;

namespace GamesPlatform.Controllers
{
    public class UsersController : Controller
    {
        private readonly IUserService userService;
        private readonly ILIbraryService lIbraryService;
        private readonly IReviewsService reviewsService;
        private readonly IRatingService ratingService;

        public UsersController(IUserService userService, ILIbraryService lIbraryService, IReviewsService reviewsService, IRatingService ratingService)
        {
            this.userService = userService;
            this.lIbraryService = lIbraryService;
            this.reviewsService = reviewsService;
            this.ratingService = ratingService;
        }
        public async Task<IActionResult> Index()
        {
            var users = await userService.GetUsersAsync();
            return View(users);
        }

        public async Task<IActionResult> Details(string id)
        {
            var user = await userService.GetUserByIdAsync(id);

            if (user == null)
                return NotFound();

            var libgames = await lIbraryService.GetUserLibraryAsync(id);
            var reviews = await reviewsService.GetUserReviewsAsync(id);
            var ratings = await ratingService.GetUserRatingsAsync(id);
            try 
            {
                var libraryId = await lIbraryService.GetLibraryId(id);

                var userDetails = new UserDetailsDTO
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    LibraryId = libraryId,
                    LibGames = libgames,
                    Reviews = reviews,
                    Ratings = ratings
                };

                return View(userDetails);
            }
            catch (Exception ex)
            {
                return View(nameof(Index));
            }
            

            
        }
    }
}
