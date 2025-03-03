using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Qwiz.Data;
using Qwiz.Models;

namespace Qwiz.Controllers.Api
{
    [Route("api/quiz")]
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
        
        [HttpGet("getList/{type}")]
        public async Task<IActionResult> GetQuizList(string username, int page, int size, string type, int? categoryIndex, string difficulty, string orderBy, string search)
        {
            if (page < 1 || (size > 20 || size < 0)) return BadRequest();

            switch (type)
            {
                case "history" when username != null:
                {
                    var query = await _um.Users
                        .Where(u => u.UserName == username)
                        .SelectMany(q => q.QuizzesTaken)
                        .Include(q => q.Quiz).ToListAsync();
                
                    var entries      = query.Skip((page - 1) * size).Take(size).ToList();
                    var pages        = (int) Math.Ceiling(decimal.Divide(query.Count, size));
                    var authUsername = _um.GetUserName(User);
                    var showSummary  = authUsername != null && authUsername == username;
                
                    return Ok(new {entries, pages, showSummary});
                }

                case "quizzesBy" when username != null:
                {
                    var query = await _db.Quizzes.Where(q => q.OwnerUsername == username).ToListAsync();
                
                    var entries      = query.Skip((page - 1) * size).Take(size).ToList();
                    var pages        = (int) Math.Ceiling(decimal.Divide(query.Count, size));
                    var authUsername = _um.GetUserName(User);
                    var canEdit      = authUsername != null && authUsername == username;
                
                    return Ok(new {entries, pages, canEdit});
                }

                case "search":
                {
                    var category =  QuizUtil.CategoryFromIndex(categoryIndex);
                    var query = await _db.Quizzes.ToListAsync();

                    // If category and difficulty is defined, filter the query
                    if (category != null)        query = query.Where(q => q.Category == category).ToList();
                    else if (difficulty != null) query = query.Where(q => q.Difficulty == difficulty).ToList();
                
                    if (search != null)
                    {
                        var searchArr        = Regex.Split(search.ToLower(), @"\s+").Where(s => s != string.Empty);
                        var queryUserName    = query.Where(q => searchArr.Any(q.OwnerUsername.ToLower().Contains)).ToList();
                        var queryDescription = query.Where(q => searchArr.Any(q.Description.ToLower().Contains)).ToList();
                        var queryCategory    = query.Where(q => searchArr.Any(q.Category.ToLower().Contains)).ToList();
                        var queryTopic       = query.Where(q => searchArr.Any(q.Topic.ToLower().Contains)).ToList();

                        List<Quiz> searchList = new List<Quiz>();
                        searchList.AddRange(queryTopic);
                        searchList.AddRange(queryUserName);
                        searchList.AddRange(queryDescription);
                        searchList.AddRange(queryCategory);
                        query = searchList.Distinct().ToList();
                    }
                
                    if (orderBy != null)
                    {
                        if (orderBy == "views")        query = query.OrderByDescending(q => q.Views).ToList();
                        else if (orderBy == "upvotes") query = query.OrderByDescending(q => q.Upvotes).ToList();
                        else if (orderBy == "recent")  query = query.OrderByDescending(q => q.CreationDate).ToList();
                    }
                
                    var entries = query.Skip((page - 1) * size).Take(size).ToList();
                    var pages   = (int) Math.Ceiling(decimal.Divide(query.Count, size));
                
                    return Ok(new {entries, pages});
                }

                default:
                    return BadRequest("Type not found!");
            }
        }
        
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> CreateQuiz([FromBody][Bind("Topic", "Category", "Description", "ImagePath", "Questions")] Quiz quiz)
        {
            if (quiz.Id != 0) return BadRequest("Invalid ID");

            quiz.Category = QuizUtil.CategoryFromIndex(int.TryParse(quiz.Category, out var i) ? i : (int?) null);
            
            if (quiz.Category == null) return BadRequest("Invalid category!");
            if (!QuizUtil.QuestionsValid(quiz.Questions)) return BadRequest("Invalid question format!");
            if (!ModelState.IsValid) return BadRequest("Invalid quiz format!");
            
            quiz.OwnerUsername = _um.GetUserName(User);
            
            _db.Add(quiz);
            await _db.SaveChangesAsync();
            
            var user = await _um.GetUserAsync(User);
            
            user.MyQuizzes.Add(quiz);
            await _um.UpdateAsync(user);
            
            return Ok();
        }

        [HttpPut("upvote/{id}")]
        [Authorize]
        public async Task<IActionResult> Upvote(int? id)
        {
            if (id == null) return BadRequest("Invalid request!");

            var quiz = await _db.Quizzes.FindAsync(id);
            if (quiz == null) return BadRequest("Could not find group!");

            var user = await _um.GetUserAsync(User);
            var upvote = await _db.Upvotes.Where(u => u.QuizId == quiz.Id && u.Username == user.UserName).FirstOrDefaultAsync();
            
            if (upvote == null)
            {
                quiz.Upvotes++;
                _db.Upvotes.Add(new Upvote(user.UserName, quiz.Id));
            }
            else
            {
                quiz.Upvotes--;
                _db.Upvotes.Remove(upvote);
            }

            await _db.SaveChangesAsync();
            return Ok(quiz.Upvotes);
        }

        [HttpPost("update")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> UpdateQuiz([FromBody][Bind("Topic", "Category", "Description", "ImagePath", "Questions")] Quiz quizForm)
        {
            if (!ModelState.IsValid) return BadRequest("Model state not valid!");
            
            var quiz = await _db.Quizzes
                .Where(q => q.Id == quizForm.Id)
                .Include(q => q.Questions)
                .FirstOrDefaultAsync();
            
            if (quiz == null) return BadRequest("Quiz not found!");
            if (quiz.OwnerId != _um.GetUserId(User)) return BadRequest("You are not the owner of this quiz!");
            if (!QuizUtil.QuestionsValid(quiz.Questions)) return BadRequest("Question format not valid!");

            if (await _db.QuizzesTaken.AnyAsync(q => q.Quiz == quiz))
                return BadRequest("You cannot edit this quiz because someone has already taken it. You can however delete it, and create a new.");
            
            try
            {
                if (quizForm.Category != quiz.Category) {
                    quiz.Category = QuizUtil.CategoryFromIndex(int.TryParse(quiz.Category, out var i) ? i : (int?) null);
                    if (quiz.Category == null) return BadRequest("Invalid category!");
                }
                
                quiz.Topic        = quizForm.Topic;
                quiz.Description  = quizForm.Description;
                quiz.ImagePath    = quizForm.ImagePath;
                
                _db.Questions.RemoveRange(quiz.Questions);
                quiz.Questions = quizForm.Questions;
                
                await _db.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                return BadRequest("Woopsie! Database error! :(");
            }
            
            return Ok();
        }
        
        [HttpDelete("delete/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            var user = await _um.GetUserAsync(User);
            if (id == null && user == null) return BadRequest("Invalid request!");
            
            var quiz = await _db.Quizzes
                .Include(q => q.Questions)
                .Include(q => q.Owner)
                .FirstOrDefaultAsync(q => q.Id == id);
        
            if (quiz == null) return BadRequest("That quiz does not exist!");
            if (quiz.Owner != user) return BadRequest("You are not the owner!");

            var quizTaken = await _db.QuizzesTaken
                .Include(q => q.QuestionsTaken)
                .Where(q => q.QuizId == id).ToListAsync();
            var questionsTaken = quizTaken.SelectMany(q => q.QuestionsTaken);

            _db.QuestionsTaken.RemoveRange(questionsTaken);
            _db.QuizzesTaken.RemoveRange(quizTaken);
            _db.Questions.RemoveRange(quiz.Questions);
            _db.Quizzes.Remove(quiz);
                
            await _db.SaveChangesAsync();

            return Ok();
        }
    }
}