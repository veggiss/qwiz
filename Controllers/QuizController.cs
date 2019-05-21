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
            
            // Remove the correct answers, client should not have this information
            quiz.Questions.ForEach(q =>
            {
                q.CorrectAnswer = "";
            });
            
            return View(quiz);
        }
        
        [Authorize]
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

            if (quiz.OwnerUsername == _um.GetUserName(User))
                return View("../Quiz/Create", quiz);
            
            return NotFound();
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