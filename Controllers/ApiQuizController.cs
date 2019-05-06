using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SpaServices.Prerendering;
using Microsoft.EntityFrameworkCore;
using Qwiz.Data;
using Qwiz.Models;


namespace Qwiz.Controllers
{
    [Route("api")]
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

        [HttpGet("getQuizList")]
        public async Task<IActionResult> GetQuizList(string username, int page, int size, string type)
        {
            if (page < 1 || size > 20) return BadRequest();

            if (type == "history" && username != null)
            {
                var query = _um.Users
                    .Where(u => u.UserName == username)
                    .SelectMany(q => q.QuizzesTaken)
                    .Include(q => q.Quiz)
                    .ThenInclude(q => q.Questions)
                    .Include(q => q.Quiz)
                    .ThenInclude(q => q.Owner).ToList();
                
                var entries = query.Skip((page - 1) * size).Take(size).ToList();
                var totalPages = (int) Math.Ceiling(decimal.Divide(query.Count, size));
                return PartialView("Profile/_HistoryCardPartial", new HistoryCardModel(entries, totalPages));
            } 
            
            if (type == "quizzesBy" && username != null)
            {
                var query = _db.Quizzes
                    .Include(q => q.Owner)
                    .Where(q => q.Owner.UserName == username).ToList();
                
                var partialString = await _um.GetUserAsync(User) != null ? "Profile/_MyQuizPartial" : "Quiz/_QuizCardPartial";
                var entries = query.Skip((page - 1) * size).Take(size).ToList();
                var totalPages = (int) Math.Ceiling(decimal.Divide(query.Count, size));
                return PartialView(partialString, new QuizCardModel(entries, totalPages));
            }

            if (type == "top100")
            {
                var query = _db.Quizzes
                    .Take(100)
                    .Include(q => q.Owner)
                    .OrderByDescending(q => q.Upvotes).ToList();
                
                var entries = query.Skip((page - 1) * size).Take(size).ToList();
                var totalPages = (int) Math.Ceiling(decimal.Divide(query.Count, size));
                
                return PartialView("Quiz/_QuizCardPartial", new QuizCardModel(entries, totalPages));
            }

            return null;
        }
        
        // api/answer?id=1&guess=A
        [HttpGet("answer")]
        [Authorize]
        public async Task<IActionResult> CheckAnswer(int quizId, int questionId, string guess)
        {
            var question = await _db.Questions.FindAsync(questionId);
            if (question == null) return BadRequest("Couldn't find that question");
            var user = await _db.Users
                .Include(u => u.QuestionsTaken)
                .Include(u => u.QuizzesTaken)
                .ThenInclude(q => q.Quiz)
                .SingleOrDefaultAsync(u => u.Id == _um.GetUserId(User));

            if (user.QuestionsTaken.Find(q => q.Question == question) == null)
            {
                var answeredCorrectly = guess == question.CorrectAnswer;
                AddQuestionTaken(user, question, answeredCorrectly);
                if (answeredCorrectly) AddExperience(user, XpGainedFromQuestion(question.Difficulty));
            }
            
            return Ok(new { correctAnswer = question.CorrectAnswer, quizFinished = await UpdateQuizTaken(user, quizId, question)});
        }

        [HttpGet("wakeUp")]
        [Authorize]
        public async void SetLastActivity()
        {
            var user = await _um.GetUserAsync(User);

            if (user != null)
            {
                if (user.LastActivity.AddMinutes(2) < DateTime.Now)
                {
                    user.LastActivity = DateTime.Now;
                    await _um.UpdateAsync(user);
                }
            }
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateQuiz([FromBody] Quiz quiz)
        {
            if (quiz.Id != 0) return BadRequest();
            if (!ModelState.IsValid) return BadRequest();
            
            _db.Add(quiz);
            await _db.SaveChangesAsync();
            
            var user = await _um.GetUserAsync(User);
            
            user.MyQuizzes.Add(quiz);
            await _um.UpdateAsync(user);
            
            return Ok();
        }

        [HttpPost("update")]
        [Authorize]
        public async Task<IActionResult> UpdateQuiz([FromBody] Quiz quizForm)
        {
            if (!ModelState.IsValid) return BadRequest();
            var quiz = await _db.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == quizForm.Id);
            if (quiz == null) return BadRequest();
            if (quiz.OwnerId != _um.GetUserId(User)) return BadRequest();
            
            try
            {
                quiz.Topic = quizForm.Topic;
                quiz.Category  = quizForm.Category;
                quiz.Description  = quizForm.Description;
                quiz.ImagePath  = quizForm.ImagePath;
                quiz.Questions = quizForm.Questions;
                
                await _db.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                return BadRequest();
            }
            
            return Ok();
        }
        
        [HttpDelete("delete")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            // TODO: Fix relation for quizzes and questions taken
            var user = await _db.Users
                .Include(u => u.QuestionsTaken)
                .ThenInclude(u => u.Question)
                .Include(u => u.QuizzesTaken)
                .ThenInclude(q => q.Quiz)
                .SingleOrDefaultAsync(u => u.Id == _um.GetUserId(User));

            if (user == null) return BadRequest();
            
            var quiz = await _db.Quizzes
                .Include(q => q.Questions)
                .Include(q => q.Owner)
                .FirstOrDefaultAsync(q => q.Id == id && q.Owner == user);

            if (quiz == null) return BadRequest();

            var quizTaken = user.QuizzesTaken.Find(q => q.Quiz == quiz);
            user.QuizzesTaken.Remove(quizTaken);

            foreach (var question in quiz.Questions)
            {
                var questionTaken = user.QuestionsTaken.Find(q => q.Question == question);
                user.QuestionsTaken.Remove(questionTaken);
            }
            
            await _um.UpdateAsync(user);
            
            _db.Questions.RemoveRange(quiz.Questions);
            _db.Quizzes.Remove(quiz);
            
            await _db.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("uploadImage")]
        [Authorize]
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

        private async void AddQuestionTaken(ApplicationUser user, Question question, bool answeredCorrectly)
        {
            user.QuestionsTaken.Add(new QuestionTaken(question, answeredCorrectly));
            await _um.UpdateAsync(user);
        }
        
        private async void AddExperience(ApplicationUser user, int amount)
        {
            user.Xp += amount;
            
            if (user.XpNeeded <= user.Xp) {
                user.Level++;
                user.Xp -= user.XpNeeded;
                user.XpNeeded = (int) Math.Round(user.XpNeeded * 1.1);
            }
            
            await _um.UpdateAsync(user);
        }

        // TODO: This operation might be too complex, another option might be to add corresponding quiz to each question on creation.
        private async Task<bool> UpdateQuizTaken(ApplicationUser user, int id, Question question)
        {
            Quiz quiz = await _db.Quizzes
                .Include(m => m.Questions)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (quiz == null) return false;
            if (!quiz.Questions.Exists(q => q == question)) return false;
            if (user.QuizzesTaken.Exists(q => q.Quiz == quiz)) return false;

            var correctAnswers = 0;
            var score = 0;
            
            foreach(var a in quiz.Questions)
            {
                var questionTaken = user.QuestionsTaken.Find(b => b.Question == a);
                if (questionTaken == null) return false;
                if (questionTaken.AnsweredCorrectly) correctAnswers++;
                score += XpGainedFromQuestion(questionTaken.Question.Difficulty);
            }
            
            user.QuizzesTaken.Add(new QuizTaken(quiz, correctAnswers, score));
            user.QuizzesTakenCount++;
            await _um.UpdateAsync(user);
                
            return true;
        }
    }
}