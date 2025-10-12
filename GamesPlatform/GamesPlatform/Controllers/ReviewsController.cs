using Microsoft.AspNetCore.Mvc;

namespace GamesPlatform.Controllers
{
    public class ReviewsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
