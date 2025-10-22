using GamesPlatform.Models;
using GamesPlatform.Services.Reviews;
using GamesPlatform.Services.Games;
using GamesPlatform.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using System.Linq;

namespace GamesPlatform.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly IReviewsService reviewsService;
        private readonly IGamesService gamesService;

        public ReviewsController(IReviewsService reviewsService, IGamesService gamesService)
        {
            this.reviewsService = reviewsService;
            this.gamesService = gamesService;
        }

        // GET: /Reviews
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var reviews = await reviewsService.GetReviewDTOsAsync();
            return View(reviews);
        }

        // GET: /Reviews/Details/5
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

        // GET: /Reviews/Create
        [HttpGet]
        public async Task<IActionResult> Create(int? gameId)
        {
            var games = await gamesService.GetGamesAsync();
            ViewBag.Games = new SelectList(games, "GameId", "Title");

            if (gameId.HasValue)
            {
                var selected = games.FirstOrDefault(g => g.GameId == gameId.Value);
                if (selected == null) return NotFound();
                ViewBag.SelectedGameId = selected.GameId;
                ViewBag.SelectedGameTitle = selected.Title;
                ViewBag.IsGameFixed = true;
            }

            return View();
        }

        // POST: /Reviews/Create
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

            await reviewsService.AddReviewAsync(gameId, userId, description);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Reviews/Edit/5
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

        // POST: /Reviews/Edit/5
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

        // GET: /Reviews/Delete/5
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

        // POST: /Reviews/DeleteConfirmed/5
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
