using Microsoft.AspNetCore.Mvc;

namespace GamesPlatform.Controllers
{
    public class RankingsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
