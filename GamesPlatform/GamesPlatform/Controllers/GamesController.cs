using System.Security.Claims;
using GamesPlatform.DTOs;
using GamesPlatform.Services.Games;
using GamesPlatform.Services.Platforms;
using GamesPlatform.Services.Ratings;
using GamesPlatform.Services.Libraries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GamesPlatform.Controllers
{
    public class GamesController : Controller
    {
        private readonly IGamesService gamesService;
        private readonly IPlatformService platformService;
        private readonly IRatingService ratingService;
        private readonly ILIbraryService lIbraryService;

        public GamesController(IGamesService gamesService, IPlatformService platformService, IRatingService ratingService, ILIbraryService lIbraryService)
        {
            this.gamesService = gamesService;
            this.platformService = platformService;
            this.ratingService = ratingService;
            this.lIbraryService = lIbraryService;
        }

        // dodano parametry q (query) i by (title/developer/publisher)
        public async Task<IActionResult> Index(string q = null, string by = "title")
        {
            var games = await gamesService.GetGamesAsync(); // pobierz wszystkie (serwis)
            var list = games.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var qq = q.Trim();
                switch ((by ?? "title").ToLowerInvariant())
                {
                    case "developer":
                        list = list.Where(g => (g.Developer ?? "").IndexOf(qq, StringComparison.OrdinalIgnoreCase) >= 0);
                        break;
                    case "publisher":
                        list = list.Where(g => (g.Publisher ?? "").IndexOf(qq, StringComparison.OrdinalIgnoreCase) >= 0);
                        break;
                    default:
                        list = list.Where(g => (g.Title ?? "").IndexOf(qq, StringComparison.OrdinalIgnoreCase) >= 0);
                        break;
                }
            }

            ViewBag.SearchQuery = q;
            ViewBag.FilterBy = by;
            return View(list.ToList());
        }

        public async Task<IActionResult> Game(int id)
        {
            var game = await gamesService.GetGameByIdAsync(id);
            if (game == null)
                return NotFound();

            return View(game);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var game = await gamesService.GetGameByIdAsync(id);
            var allPlatforms = await platformService.GetPlatformsAsync();
            var editDTO = new EditGameDTO
            {
                GameId = game.GameId,
                Title = game.Title,
                Tag = game.Tag,
                ReleaseYear = game.ReleaseYear,
                Developer = game.Developer,
                Publisher = game.Publisher,
                Description = game.Description,
                ImageUrl = game.ImageUrl,
                ThumbNailUrl = game.ThumbNailUrl,
                SelectedPlatformIds = game.Platforms.Select(p => p.PlatformId).ToList(),
                AllPlatforms = allPlatforms.ToList()
            };
            return View(editDTO);

        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(EditGameDTO gameDTO)
        {
            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"Field {state.Key} error: {error.ErrorMessage}");
                    }
                }
                return View(gameDTO);
            }
                
            await gamesService.EditGameAsync(gameDTO.GameId, gameDTO);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Add()
        {
            var game = new GameDTO { ReleaseYear = DateTime.UtcNow };
            return View(game);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(GameDTO gameDTO)
        {
            if (!ModelState.IsValid)
                return View(gameDTO);

            await gamesService.AddGameAsync(gameDTO);
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> AddRating(int gameId)
        {
            var game = await gamesService.GetGameByIdAsync(gameId);
            if (game == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Forbid();

            try
            {
                var lib = await lIbraryService.GetUserLibraryAsync(userId);
                if (!lib.Any(x => x.GameId == gameId))
                {
                    TempData["Error"] = "Aby dodać ocenę musisz najpierw dodać grę do swojej biblioteki.";
                    return RedirectToAction(nameof(Game), new { id = gameId });
                }
            }
            catch
            {
                TempData["Error"] = "Twoja biblioteka nie istnieje. Najpierw dodaj grę do biblioteki.";
                return RedirectToAction(nameof(Game), new { id = gameId });
            }

            var existing = await ratingService.GetUserRatingAsync(gameId, userId);
            if (existing != null)
            {
                var dto = new RatingDTO
                {
                    Gameplay = existing.Gameplay,
                    Graphics = existing.Graphics,
                    Optimization = existing.Optimization,
                    Story = existing.Story
                };

                ViewBag.IsEdit = true;
                ViewBag.RatingId = existing.RatingId;
                ViewBag.GameId = gameId;
                ViewBag.GameTitle = game.Title;
                return View(dto);
            }

            ViewBag.IsEdit = false;
            ViewBag.GameId = gameId;
            ViewBag.GameTitle = game.Title;
            return View(new RatingDTO());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRating(int gameId, RatingDTO rating)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Forbid();

            try
            {
                var lib = await lIbraryService.GetUserLibraryAsync(userId);
                if (!lib.Any(x => x.GameId == gameId))
                {
                    TempData["Error"] = "Aby dodać ocenę musisz najpierw dodać grę do swojej biblioteki.";
                    return RedirectToAction(nameof(Game), new { id = gameId });
                }
            }
            catch
            {
                TempData["Error"] = "Twoja biblioteka nie istnieje. Najpierw dodaj grę do biblioteki.";
                return RedirectToAction(nameof(Game), new { id = gameId });
            }

            if (rating == null
                || rating.Gameplay is < 1 or > 10
                || rating.Graphics is < 1 or > 10
                || rating.Optimization is < 1 or > 10
                || rating.Story is < 1 or > 10)
            {
                ModelState.AddModelError(string.Empty, "Oceny muszą być w zakresie 1–10.");
            }

            if (!ModelState.IsValid)
            {
                var game = await gamesService.GetGameByIdAsync(gameId);
                ViewBag.GameId = gameId;
                ViewBag.GameTitle = game?.Title ?? $"GameId: {gameId}";
                var existing = await ratingService.GetUserRatingAsync(gameId, userId);
                ViewBag.IsEdit = existing != null;
                ViewBag.RatingId = existing?.RatingId ?? 0;
                return View(rating);
            }

            try
            {
                var existing = await ratingService.GetUserRatingAsync(gameId, userId);
                if (existing != null)
                {
                    await ratingService.EditRatingAsync(existing.RatingId, rating);
                    TempData["Success"] = "Ocena została zaktualizowana.";
                }
                else
                {
                    await ratingService.AddRatingAsync(gameId, userId, rating);
                    TempData["Success"] = "Ocena została zapisana.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Game), new { id = gameId });
        }
    }
}
