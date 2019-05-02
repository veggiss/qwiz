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
using Qwiz.Areas.Identity.Pages.Account;

namespace Qwiz.Controllers
{
    
    public class ProfileController : Controller
    {
    
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _um;

        public ProfileController(ApplicationDbContext db, UserManager<ApplicationUser> um)
        {
            _db = db;
            _um = um;
        }
        
        public async Task<IActionResult> Index(string username)
        {
            if (username == null) username = _um.GetUserName(User);
            if (username == null) return Redirect("../Identity/Account/Login");
            
            var user = await _db.Users
                .Include(u => u.QuestionsTaken)
                .ThenInclude(u => u.Question)
                .Include(u => u.QuizzesTaken)
                .ThenInclude(u => u.Quiz)
                .Include(u => u.MyQuizzes)
                .SingleOrDefaultAsync(u => u.UserName == username);

            if (user == null) return NotFound();
            else return View(user);
        }
    }

}