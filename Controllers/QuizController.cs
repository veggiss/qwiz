using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Qwiz.Data;
using Qwiz.Models;

namespace Qwiz.Controllers
{
    public class QuizController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _um;
        
        public QuizController(ApplicationDbContext db, UserManager<ApplicationUser> um)
        {
            _db = db;
            _um = um;
        }
        
        public async Task<IActionResult> Index()
        {
            var quizzes = _db.Quizzes;
            
            return View(await quizzes.ToListAsync());
        }

        public async Task<IActionResult> Play(int id)
        {
            var quizzes = await _db.Quizzes
                .Include(c => c.Questions)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (quizzes == null) return NotFound();

            var questions = quizzes.Questions;
            if (questions == null) return NotFound();
            questions.ForEach(item => item.CorrectAnswer = "");
            
            return View(new QuizViewModel(id, questions.ToList()));
        }

        public IActionResult Create()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            Quiz quiz = await _db.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == id);
            if (quiz == null) return NotFound();
            
            return View("../Quiz/Create", quiz);
        }
    }
}