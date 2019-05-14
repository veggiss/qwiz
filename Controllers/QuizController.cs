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
            var quizzes = _db.Quizzes
                .Include(q => q.Owner)
                .Skip(0).Take(3);
            
            return View(await quizzes.ToListAsync());
        }

        public async Task<IActionResult> Play(int? id)
        {
            if (id == null) return NotFound();
            
            var quiz = await _db.Quizzes
                .Include(c => c.Questions)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (quiz == null) return NotFound();
            
            quiz.Questions.ForEach(item => item.CorrectAnswer = "");
            
            return View(quiz);
        }

        public IActionResult Create()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            
            Quiz quiz = await _db.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == id);
            if (quiz == null) return NotFound();
            
            return View("../Quiz/Create", quiz);
        }

        [Authorize]
        public async Task<IActionResult> Summary(int? id)
        {
            if (id == null) return NotFound();

            var user = await _db.Users
                .Include(u => u.QuizzesTaken)
                .ThenInclude(q => q.QuestionsTaken)
                .Include(u => u.QuizzesTaken)
                .ThenInclude(q => q.Quiz)
                .ThenInclude(q => q.Questions)
                .SingleOrDefaultAsync(u => u.Id == _um.GetUserId(User));
            
            if (user != null) {
                var quizTaken = user.QuizzesTaken.Find(q => q.Quiz.Id == id);
                if (quizTaken == null) return NotFound();

                return View(quizTaken);
            }
            
            return BadRequest();
        }
    }
}