using Microsoft.AspNetCore.Mvc;

namespace Qwiz.Controllers
{
    public class LeaderboardsController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return
            View();
        }
    }
}