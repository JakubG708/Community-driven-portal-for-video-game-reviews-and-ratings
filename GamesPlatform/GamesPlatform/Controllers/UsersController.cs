using System.Security.Claims;
using GamesPlatform.DTOs;
using GamesPlatform.Models;
using GamesPlatform.Services.Libraries;
using GamesPlatform.Services.Ratings;
using GamesPlatform.Services.Reviews;
using GamesPlatform.Services.Users;
using GamesPlatform.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GamesPlatform.Controllers
{
    public class UsersController : Controller
    {
        private readonly IUserService userService;
        private readonly ILIbraryService lIbraryService;
        private readonly IReviewsService reviewsService;
        private readonly IRatingService ratingService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILIbraryService lIbraryService, IReviewsService reviewsService, IRatingService ratingService, ILogger<UsersController> logger)
        {
            this.userService = userService;
            this.lIbraryService = lIbraryService;
            this.reviewsService = reviewsService;
            this.ratingService = ratingService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? q, string? by)
        {
            ViewBag.SearchQuery = q ?? "";
            ViewBag.FilterBy = by ?? "username";

            var users = await userService.GetUsersAsync(q, by);
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
            catch (Exception)
            {
                return View(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToLibrary(int gameId, Status status = Status.InProgress, string? userId = null)
        {
            if (string.IsNullOrEmpty(userId))
            {
                userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
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
        public async Task<IActionResult> RemoveFromLibrary(int gameId, string? userId = null)
        {
            Console.WriteLine("RemoveFromLibrary called.");
            _logger.LogInformation("RemoveFromLibrary called. gameId={GameId}, userIdParam={UserIdParam}, currentUser={CurrentUser}", gameId, userId, User?.Identity?.Name);

            var currentUserId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                if (string.IsNullOrEmpty(currentUserId))
                {
                    _logger.LogWarning("RemoveFromLibrary forbidden: not authenticated");
                    return Forbid();
                }
                userId = currentUserId;
            }
            else
            {
                if (userId != currentUserId && !User.IsInRole("Admin"))
                {
                    _logger.LogWarning("RemoveFromLibrary forbidden: user {User} tried to remove from {TargetUser}", currentUserId, userId);
                    return Forbid();
                }
            }

            try
            {
                await lIbraryService.RemoveGameFromLibraryAsync(userId, gameId);
                TempData["Success"] = "Gra została usunięta z biblioteki.";
                _logger.LogInformation("Game {GameId} removed from library of user {UserId}", gameId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove game {GameId} from user {UserId} library", gameId, userId);
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id = userId });
        }
    }
}
