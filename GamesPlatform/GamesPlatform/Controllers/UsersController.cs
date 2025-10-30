using System.Security.Claims;
using GamesPlatform.DTOs;
using GamesPlatform.Models;
using GamesPlatform.Services.Libraries;
using GamesPlatform.Services.Ratings;
using GamesPlatform.Services.Reviews;
using GamesPlatform.Services.Users;
using GamesPlatform.Enums;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToLibrary(int gameId, Status status = Status.InProgress, string userId = null)
        {
            if (string.IsNullOrEmpty(userId))
            {
                userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Forbid();
            }

            try
            {
                await lIbraryService.AddGameToLibraryAsync(userId, gameId, status);
                TempData["Success"] = "Gra dodana do biblioteki.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id = userId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromLibrary(int gameId, string userId = null)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                if (string.IsNullOrEmpty(currentUserId))
                    return Forbid();
                userId = currentUserId;
            }
            else
            {
                if (userId != currentUserId && !User.IsInRole("Admin"))
                    return Forbid();
            }

            try
            {
                await lIbraryService.RemoveGameFromLibraryAsync(userId, gameId);
                TempData["Success"] = "Gra została usunięta z biblioteki.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id = userId });
        }
    }
}
