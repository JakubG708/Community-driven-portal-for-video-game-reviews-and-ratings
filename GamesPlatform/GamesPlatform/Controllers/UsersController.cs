using Microsoft.AspNetCore.Mvc;

namespace GamesPlatform.Controllers
{
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
