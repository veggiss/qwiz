using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Qwiz.Data;
using Qwiz.Models;

namespace Qwiz.Controllers
{
    [Route("Group")]
    public class GroupController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _um;
        
        public GroupController(ApplicationDbContext db, UserManager<ApplicationUser> um)
        {
            _db = db;
            _um = um;
        }
        
        public IActionResult Index()
        {
            return View();
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> Group(int id)
        {
            var group = await _db.Groups
                .Where(g => g.Id == id)
                .Include(g => g.PendingInvites)
                .Include(g => g.Members)
                .ThenInclude(g => g.Member)
                .FirstOrDefaultAsync();
            if (group == null) return NotFound();

            var username = _um.GetUserName(User);

            if (username != null)
            {
                var waitingRequest = group.PendingInvites.Exists(p => p.Username == username);
                var member = group.Members.Find(m => m.Member.UserName == username);
                
                return View("Group", new GroupViewModel(group, member?.Username, member?.Role, group.Members.Count, waitingRequest));
            }
            
            return View("Group", new GroupViewModel(group, null, null, group.Members.Count, false));
        }
    }
}