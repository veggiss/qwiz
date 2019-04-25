using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Qwiz.Data;
using Qwiz.Models;


namespace Qwiz.Controllers
{
    [Route("api")]
    // ApiController enables automatic model validation
    [ApiController]
    public class ApiQuizController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _um;
        
        public ApiQuizController(ApplicationDbContext db, UserManager<ApplicationUser> um)
        {
            _db = db;
            _um = um;
        }

        [HttpGet("getUser")]
        public async Task<IActionResult> GetUser()
        {
            ApplicationUser user = await _um.FindByEmailAsync("user@uia.no");

            return Ok(user);
        }
        
        // api/answer?id=1&guess=A
        [HttpGet("answer")]
        [Authorize]
        public async Task<IActionResult> CheckAnswer(int quizId, int questionId, string guess)
        {
            var user = await _um.GetUserAsync(User);
            var question = await _db.Questions.FindAsync(questionId);
            if (question == null) return BadRequest("Couldn't find that question");
            
            if (!user.QuestionsTaken.Contains(question))
            {
                AddQuestionTaken(user, question);
                if (guess == question.CorrectAnswer) AddExperience(user, question.Difficulty);
            }
            
            return Ok(new { correctAnswer = question.CorrectAnswer, quizFinished = await UpdateQuizTaken(user, quizId, question)});
        }

        [HttpPost("create")]
        public IActionResult CreateQuiz([FromBody] Quiz quiz)
        {
            if (quiz.Id != 0) return BadRequest();
            if (!ModelState.IsValid) return BadRequest();
            
            _db.Add(quiz);
            _db.SaveChanges();

            return Ok();
        }

        [HttpPost("uploadImage")]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile image)
        {
            if (image == null || image.Length == 0) return BadRequest("File not selected");
            if (!image.ContentType.Contains("image")) return BadRequest("File type not supported");
            if (image.Length > 1024 * 1024) return BadRequest("Files bigger than 1MB not allowed");
            
            string filename = Guid.NewGuid() + Path.GetExtension(image.FileName);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", filename);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            return Ok("/images/" + filename);
        }

        private static int XpGainedFromQuestion(string type)
        {
            int value = 0;

            if (type == "easy")
                value = 100;
            else if (type == "medium")
                value = 150;
            else if (type == "hard") value = 200;

            return value;
        }

        private async void AddQuestionTaken(ApplicationUser user, Question question)
        {
            user.QuestionsTaken.Add(question);
            await _um.UpdateAsync(user);
        }
        
        private async void AddExperience(ApplicationUser user, string type)
        {
            user.Xp += XpGainedFromQuestion(type);
            
            if (user.XpNeeded <= user.Xp) {
                user.Level++;
                user.Xp -= user.XpNeeded;
                user.XpNeeded = (int) Math.Round(user.XpNeeded * 1.1);
            }
            
            await _um.UpdateAsync(user);
        }

        // This operation might be too complex, another option might be to add corresponding quiz to each question on creation.
        private async Task<bool> UpdateQuizTaken(ApplicationUser user, int id, Question question)
        {
            Quiz quiz = await _db.Quizzes
                .Include(m => m.Questions)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (quiz == null) return false;
            if (!quiz.Questions.Contains(question)) return false;
            
            foreach(var q in quiz.Questions.ToList())
            {
                if (!user.QuestionsTaken.Contains(q)) return false;
            }

            var usrContext = await _db.Users.Include(u => u.QuizzesTaken).SingleOrDefaultAsync(u => u.Id == user.Id);
            usrContext.QuizzesTaken.Add(quiz);
            await _um.UpdateAsync(usrContext);
            _db.SaveChanges();
                
            return true;

        }
    }
}