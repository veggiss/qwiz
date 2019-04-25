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
        
        public async Task<IActionResult> Index()
        {
            var user = _um.GetUserAsync(User).Result;
            return View();
        }
    }

}


// Accessing the result waits for async completion




/* GET: Users
[Authorize]
        public async Task<IActionResult> Index()
        {
            var applicationDbInitializer = _dbContext.ApplicationUsers.Include(c => c.UserName);
            return View(await ApplicationDbContext.ToListAsync());
        }

// GET: User/Profile
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _dbContext.ApplicationUsers
                .Include(c => c.OwnerId)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
        
         public class ProfileController : Controller
    {
        

        public ProfileController(ApplicationDbContext context)
        {
            _dbContext = context;
            
            
    }*/













/*{
    
        
        
        
        [Authorize]
        public IActionResult Index()
        {
            var user = _um.GetUserAsync(User).Result;

            return View(ApplicationUser);
        }
    }
    
    
}
*/