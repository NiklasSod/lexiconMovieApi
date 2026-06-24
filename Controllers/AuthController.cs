using Microsoft.AspNetCore.Mvc;

namespace MovieApi.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            // do stuff, add login somewhere as api, update jwt stuff
            return View();
        }
    }
}
