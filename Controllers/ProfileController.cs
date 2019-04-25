using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Qwiz.Data;
using Qwiz.Models;
using Microsoft.EntityFrameworkCore;

namespace Qwiz.Controllers
{
    
    public class ProfileController : Controller
    {
    
        private readonly ApplicationDbContext _db;
        
        private UserManager<ApplicationUser> _um;

        public ProfileController(ApplicationDbContext db, UserManager<ApplicationUser> um)


        {
            _db = db;
            _um = um;
            
        }
        
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var usr = await _um.GetUserAsync(User);
            var user = await _db.Users.Include(u => u.QuizzesTaken).SingleOrDefaultAsync(u => u.Id == usr.Id);
            return View(user);
        }
    }

}