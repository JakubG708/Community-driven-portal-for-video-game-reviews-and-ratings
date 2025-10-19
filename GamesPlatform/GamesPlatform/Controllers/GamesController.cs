using GamesPlatform.DTOs;
using GamesPlatform.Services.Games;
using GamesPlatform.Services.Platforms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GamesPlatform.Controllers
{
    public class GamesController : Controller
    {
        private readonly IGamesService gamesService;
        private readonly IPlatformService platformService;

        public GamesController(IGamesService gamesService, IPlatformService platformService)
        {
            this.gamesService = gamesService;
            this.platformService = platformService;
        }
        public async Task<IActionResult> Index()
        {
            var games = await gamesService.GetGamesAsync();
            return View(games);
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

        public async Task<IActionResult> Add()
        {
            var game = new GameDTO();
            return View(game);
        }

        [HttpPost]
        public async Task<IActionResult> Add(GameDTO gameDTO)
        {
            if (!ModelState.IsValid)
                return View(gameDTO);

            await gamesService.AddGameAsync(gameDTO);
            return RedirectToAction(nameof(Index));
        }


    }
}
