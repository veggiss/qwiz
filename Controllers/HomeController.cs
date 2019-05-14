using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Qwiz.Data;
using Qwiz.Models;

namespace Qwiz.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        
        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }
        
        public async Task<IActionResult> Index()
        {
            var quizzes = _db.Quizzes
                .Include(q => q.Owner)
                .Skip(0).Take(3);
            
            return View(await quizzes.ToListAsync());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
