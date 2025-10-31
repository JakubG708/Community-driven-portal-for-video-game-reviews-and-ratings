using GamesPlatform.Services.Games;
using GamesPlatform.Services.Ratings;
using GamesPlatform.DTOs;
using Microsoft.AspNetCore.Mvc;
using GamesPlatform.Enums; // dodane

namespace GamesPlatform.Controllers
{
    public class RankingsController : Controller
    {
        private readonly IRatingService ratingService;
        private readonly IGamesService gamesService;

        public RankingsController(IRatingService ratingService, IGamesService gamesService)
        {
            this.ratingService = ratingService;
            this.gamesService = gamesService;
        }

        public async Task<IActionResult> Index([FromQuery] string[]? metrics, [FromQuery] int? limit, [FromQuery] string? tag)
        {
            var selectedMetrics = (metrics == null || metrics.Length == 0)
                ? new[] { "overall" }
                : metrics.Select(m => m.Trim().ToLowerInvariant()).Where(m => !string.IsNullOrEmpty(m)).ToArray();

            var games = (await gamesService.GetGamesAsync()).ToList();
            var ratings = (await ratingService.GetAllRatingsAsync()).ToList();

            var selectedTag = (tag ?? "").Trim();
            if (!string.IsNullOrEmpty(selectedTag) && !string.Equals(selectedTag, "all", StringComparison.OrdinalIgnoreCase))
            {
                if (Enum.TryParse<Tags>(selectedTag, true, out var parsedTag))
                {
                    games = games.Where(g => g.Tag == parsedTag).ToList();
                }
                else
                {
                    games = new List<Models.Game>();
                }
            }

            var totalGames = games.Count;
            var maxLimit = Math.Max(1, totalGames);
            var take = limit.HasValue && limit.Value > 0 ? Math.Min(limit.Value, totalGames == 0 ? limit.Value : totalGames) : 100;
            if (take <= 0) take = 100;

            var result = new List<TopGameDTO>(games.Count);
            foreach (var g in games)
            {
                var gameRatings = ratings.Where(r => r.GameId == g.GameId).ToList();
                double avgScore;

                if (!gameRatings.Any())
                {
                    avgScore = 0.0;
                }
                else
                {
                    var perRatingScores = gameRatings.Select(r =>
                    {
                        var list = new List<double>();
                        if (selectedMetrics.Contains("overall"))
                        {
                            list.Add((r.Gameplay + r.Graphics + r.Optimization + r.Story) / 4.0);
                        }
                        else
                        {
                            if (selectedMetrics.Contains("gameplay")) list.Add(r.Gameplay);
                            if (selectedMetrics.Contains("graphics")) list.Add(r.Graphics);
                            if (selectedMetrics.Contains("optimization")) list.Add(r.Optimization);
                            if (selectedMetrics.Contains("story")) list.Add(r.Story);
                        }
                        if (!list.Any()) return 0.0;
                        return list.Average();
                    });

                    avgScore = perRatingScores.Average();
                }

                result.Add(new TopGameDTO
                {
                    GameId = g.GameId,
                    Title = g.Title,
                    Score = avgScore,
                    RatingsCount = gameRatings.Count
                });
            }

            var ordered = result
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.RatingsCount)
                .Take(take)
                .ToList();

            ViewBag.SelectedMetrics = selectedMetrics;
            ViewBag.TotalGames = totalGames;
            ViewBag.CurrentLimit = take;
            ViewBag.SelectedTag = selectedTag;

            return View(ordered);
        }
    }
}
