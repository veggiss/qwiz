using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Qwiz.Data;
using Qwiz.Models;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Qwiz.Controllers
{
    public class LeaderboardController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _um;
        
        public LeaderboardController(ApplicationDbContext db, UserManager<ApplicationUser> um)
        {
            _db = db;
            _um = um;
        }
        
        public async Task<IActionResult> Index()
        {
            var topPlayers = _um.Users
                .Take(100)
                .OrderByDescending(u => u.Xp);
                
                
            return View(await topPlayers.ToListAsync());
        }
    }
}