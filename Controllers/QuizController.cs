using System;
using System.Collections.Generic;
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
    [Route("quiz")]
    public class QuizController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _um;
        private readonly Random _ran = new Random();
        
        public QuizController(ApplicationDbContext db, UserManager<ApplicationUser> um)
        {
            _db = db;
            _um = um;
        }
        
        public async Task<IActionResult> Index()
        {
            if (QuizUtil.OfTheDayTimer < DateTime.Now)
                QuizUtil.OfTheDayList = await UpdateOfTheDay();
            
            return View(QuizUtil.OfTheDayList);
        }

        [HttpGet("{id}")]
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
        
        [HttpGet("create")]
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [HttpGet("edit/{id}")]
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

        [HttpGet("summary/{id}")]
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
        
        private async Task<List<Quiz>> UpdateOfTheDay()
        {
            var newQuizList = new List<Quiz>();
            var quizList = QuizUtil.OfTheDayList.ToList();
            QuizUtil.OfTheDayTimer = DateTime.Now.AddSeconds(10);

            for (var i = 0; i < 3; i++)
            {
                var quiz = await _db.Quizzes.Where(q => quizList.All(q2 => q2.Id != q.Id)).FirstOrDefaultAsync();
                if (quiz == null) break;
                
                quizList.Add(quiz);
                newQuizList.Add(quiz);
            }
            
            return newQuizList;
        }
    }
}