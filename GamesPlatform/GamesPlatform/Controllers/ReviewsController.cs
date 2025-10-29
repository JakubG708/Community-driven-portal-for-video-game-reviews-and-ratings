using GamesPlatform.Models;
using GamesPlatform.Services.Reviews;
using GamesPlatform.Services.Games;
using GamesPlatform.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using System.Linq;
using GamesPlatform.Services.Libraries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace GamesPlatform.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly IReviewsService reviewsService;
        private readonly IGamesService gamesService;
        private readonly ILIbraryService lIbraryService;

        [ActivatorUtilitiesConstructor]
        public ReviewsController(IReviewsService reviewsService, IGamesService gamesService, ILIbraryService lIbraryService)
        {
            this.reviewsService = reviewsService;
            this.gamesService = gamesService;
            this.lIbraryService = lIbraryService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var reviews = await reviewsService.GetReviewDTOsAsync();
            return View(reviews);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var review = await reviewsService.GetReviewByIdAsync(id);
                return View(review);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Create(int? gameId)
        {
            var games = await gamesService.GetGamesAsync();
            ViewBag.Games = new SelectList(games, "GameId", "Title");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Forbid();

            if (gameId.HasValue)
            {
                var selected = games.FirstOrDefault(g => g.GameId == gameId.Value);
                if (selected == null) return NotFound();
                // sprawdź czy użytkownik ma grę w bibliotece
                try
                {
                    var lib = await lIbraryService.GetUserLibraryAsync(userId);
                    if (!lib.Any(x => x.GameId == gameId.Value))
                        return Forbid();
                }
                catch
                {
                    // brak biblioteki -> zabroń dodawania recenzji
                    return Forbid();
                }

                ViewBag.SelectedGameId = selected.GameId;
                ViewBag.SelectedGameTitle = selected.Title;
                ViewBag.IsGameFixed = true;
            }

            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int gameId, string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                ModelState.AddModelError("", "Review content cannot be empty.");
                var gamesForError = await gamesService.GetGamesAsync();
                ViewBag.Games = new SelectList(gamesForError, "GameId", "Title");
                return View();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError("", "User must be logged in to add a review.");
                var gamesForAuthError = await gamesService.GetGamesAsync();
                ViewBag.Games = new SelectList(gamesForAuthError, "GameId", "Title");
                return View();
            }

            // sprawdź czy użytkownik ma dodaną grę w bibliotece
            try
            {
                var lib = await lIbraryService.GetUserLibraryAsync(userId);
                if (!lib.Any(x => x.GameId == gameId))
                {
                    ModelState.AddModelError("", "Musisz mieć tę grę w swojej bibliotece, aby dodać recenzję.");
                    var gamesForLibError = await gamesService.GetGamesAsync();
                    ViewBag.Games = new SelectList(gamesForLibError, "GameId", "Title");
                    return View();
                }
            }
            catch
            {
                ModelState.AddModelError("", "Twoja biblioteka nie istnieje. Nie możesz dodać recenzji.");
                var gamesForLibError = await gamesService.GetGamesAsync();
                ViewBag.Games = new SelectList(gamesForLibError, "GameId", "Title");
                return View();
            }

            await reviewsService.AddReviewAsync(gameId, userId, description);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var review = await reviewsService.GetReviewByIdAsync(id);
                if (review == null)
                    return NotFound();

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(currentUserId) || review.UserId != currentUserId)
                    return Forbid();

                return View(review);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                ModelState.AddModelError("", "Review content cannot be empty.");
                var reviewForError = await reviewsService.GetReviewByIdAsync(id);
                return View(reviewForError);
            }

            try
            {
                var review = await reviewsService.GetReviewByIdAsync(id);
                if (review == null)
                    return NotFound();

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(currentUserId) || review.UserId != currentUserId)
                    return Forbid();

                await reviewsService.EditReviewAsync(id, description);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var review = await reviewsService.GetReviewByIdAsync(id);
                if (review == null)
                    return NotFound();

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");

                if (string.IsNullOrEmpty(currentUserId) || (review.UserId != currentUserId && !isAdmin))
                    return Forbid();

                return View(review);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var review = await reviewsService.GetReviewByIdAsync(id);
                if (review == null)
                    return NotFound();

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");

                if (string.IsNullOrEmpty(currentUserId) || (review.UserId != currentUserId && !isAdmin))
                    return Forbid();

                await reviewsService.DeleteReviewAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
