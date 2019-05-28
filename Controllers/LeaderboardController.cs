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
    [Route("leaderboard")]
    public class LeaderboardController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _um;
        
        public LeaderboardController(ApplicationDbContext db, UserManager<ApplicationUser> um)
        {
            _db = db;
            _um = um;
        }
        
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> Quiz(int id)
        {
            var quiz = await _db.Quizzes.Where(q => q.Id == id).FirstOrDefaultAsync();

            if (quiz == null)
                return NotFound();
            
            return View(quiz);
        }
    }
}