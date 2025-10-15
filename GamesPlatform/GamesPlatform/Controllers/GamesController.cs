using GamesPlatform.Services.Games;
using Microsoft.AspNetCore.Mvc;

namespace GamesPlatform.Controllers
{
    public class GamesController : Controller
    {
        private readonly IGamesService gamesService;

        public GamesController(IGamesService gamesService)
        {
            this.gamesService = gamesService;
        }
        public async Task<IActionResult> Index()
        {
            var games = await gamesService.GetGamesAsync();
            return View(games);
        }

        public async Task<IActionResult> Game(int id)
        {
            var game = await gamesService.GetGameByIdAsync(id);
            return View(game);
        }
    }
}
