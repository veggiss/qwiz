using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SpaServices.Prerendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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

        [HttpGet("getQuizList")]
        public async Task<IActionResult> GetQuizList(string username, int page, int size, string type, int? categoryIndex, string difficulty, string orderBy, string search)
        {
            if (page < 1 || (size > 20 || size < 0)) return BadRequest();

            if (type == "history" && username != null)
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
            
            if (type == "quizzesBy" && username != null)
            {
                var query = await _db.Quizzes.Where(q => q.OwnerUsername == username).ToListAsync();
                
                var entries      = query.Skip((page - 1) * size).Take(size).ToList();
                var pages        = (int) Math.Ceiling(decimal.Divide(query.Count, size));
                var authUsername = _um.GetUserName(User);
                var canEdit      = authUsername != null && authUsername == username;
                
                return Ok(new {entries, pages, canEdit});
            }

            if (type == "category")
            {
                var category =  CategoryFromIndex(categoryIndex);
                var query = await _db.Quizzes.ToListAsync();

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

            return BadRequest();
        }

        // TODO: Would it still be rest if we check if the user HAS started a question here? As is, the user can tamp stamp themselves
        [HttpGet("startTimer")]
        [Authorize]
        public async void StartTimer()
        {
            var user = await _um.GetUserAsync(User);
            
            if (user != null) {
                user.LastQuestionStarted = DateTime.Now;
                await _um.UpdateAsync(user);
            }
        }

        [HttpGet("answer")]
        public async Task<IActionResult> CheckAnswer(int? quizId, int? questionId, string guess)
        {
            if (quizId == null || questionId == null || guess == null) return BadRequest();
            
            var question = await _db.Questions.FindAsync(questionId);
            if (question == null) return BadRequest();

            var userId = _um.GetUserId(User);
            var xpGained = 0;
            var bonus = 0;
            
            if (userId != null) {
                var user = await _db.Users
                    .Include(u => u.QuestionsTaken)
                    .ThenInclude(q => q.Question)
                    .Include(u => u.QuizzesTaken)
                    .ThenInclude(q => q.Quiz)
                    .SingleOrDefaultAsync(u => u.Id == userId);
                
                if (user != null && !user.QuestionsTaken.Exists(q => q.Question.Id == questionId)) 
                {
                    var answeredCorrectly = guess == question.CorrectAnswer;
                    var timer = DateTime.Now - user.LastQuestionStarted;
                    var timerSec = (int) Math.Round(timer.TotalSeconds);
                    
                    if (answeredCorrectly && timerSec >= 0 && timerSec <= 15)
                    {
                        xpGained = XpGainedFromQuestion(question.Difficulty);
                        bonus = (int) (xpGained * (((15f - timerSec) / 15f * 100f) / 100f));
                        AddExperience(user, xpGained + bonus);
                    }
                    
                    AddQuestionTaken(user, question, answeredCorrectly, xpGained, bonus, timer, guess);
                }
                    
                UpdateQuizTaken(user, quizId, question);
            }
            
            return Ok(new {correctAlternative = question.CorrectAlternative, xpGained, bonus});
        }
        
        // TODO: Change to a websocket solution?
        [HttpGet("wakeUp")]
        [Authorize]
        public async void SetLastActivity()
        {
            var user = await _um.GetUserAsync(User);

            if (user != null)
            {
                if (user.LastActivity.AddMinutes(4) < DateTime.Now)
                {
                    user.LastActivity = DateTime.Now;
                    await _um.UpdateAsync(user);
                }
            }
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateQuiz([FromBody][Bind("Topic", "Category", "Description", "ImagePath", "Questions")] Quiz quiz)
        {
            if (quiz.Id != 0) return BadRequest();
            if (!ModelState.IsValid) return BadRequest();
            if (!QuestionsValid(quiz.Questions)) return BadRequest();
            
            quiz.OwnerUsername = _um.GetUserName(User);
            
            _db.Add(quiz);
            await _db.SaveChangesAsync();
            
            var user = await _um.GetUserAsync(User);
            
            user.MyQuizzes.Add(quiz);
            await _um.UpdateAsync(user);
            
            return Ok();
        }

        [HttpPost("update")]
        [Authorize]
        public async Task<IActionResult> UpdateQuiz([FromBody][Bind("Topic", "Category", "Description", "ImagePath", "Questions")] Quiz quizForm)
        {
            if (!ModelState.IsValid) return BadRequest();
            
            var quiz = await _db.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == quizForm.Id);
            
            if (quiz == null) return BadRequest();
            if (quiz.OwnerId != _um.GetUserId(User)) return BadRequest();
            if (!QuestionsValid(quiz.Questions)) return BadRequest();
            
            try
            {
                quiz.Topic        = quizForm.Topic;
                quiz.Category     = quizForm.Category;
                quiz.Description  = quizForm.Description;
                quiz.ImagePath    = quizForm.ImagePath;
                quiz.Questions    = quizForm.Questions;
                
                await _db.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                return BadRequest();
            }
            
            return Ok();
        }
        
        [HttpDelete("delete")]
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            var user = await _um.GetUserAsync(User);
            if (id == null && user == null) return BadRequest();
            
            var quiz = await _db.Quizzes
                .Include(q => q.Questions)
                .Include(q => q.Owner)
                .Where(q => q.Owner == user)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null) return BadRequest();

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

        [HttpPost("uploadImage")]
        [Authorize]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile image)
        {
            if (image == null || image.Length == 0) return BadRequest("File not selected");
            if (!image.ContentType.Contains("image")) return BadRequest("File type not supported");
            if (image.Length > 1024 * 1024) return BadRequest("Files bigger than 1MB not allowed");
            
            var filename = Guid.NewGuid() + Path.GetExtension(image.FileName);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", filename);

            using (var stream = new FileStream(path, FileMode.Create))
                await image.CopyToAsync(stream);

            return Ok("/images/" + filename);
        }

        private static int XpGainedFromQuestion(string type)
        {
            int value = 0;

            if (type == "easy")
                value = 100;
            else if (type == "medium")
                value = 150;
            else if (type == "hard") 
                value = 200;

            return value;
        }

        private async void AddQuestionTaken(ApplicationUser user, Question question, bool answeredCorrectly, int xpGained, int bonus, TimeSpan timer, string answer)
        {
            var questionTaken = new QuestionTaken(question, answeredCorrectly, xpGained, bonus, timer, answer);
            await _db.QuestionsTaken.AddAsync(questionTaken);
            user.QuestionsTaken.Add(questionTaken);
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

        // TODO: can we optimize these queries?
        private async void UpdateQuizTaken(ApplicationUser user, int? id, Question question)
        {
            Quiz quiz = await _db.Quizzes
                .Include(m => m.Questions)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (quiz == null) return;
            if (!quiz.Questions.Exists(q => q == question)) return;
            if (user.QuizzesTaken.Exists(q => q.Quiz == quiz)) return;
            // TODO: Might get these from DB
            List<QuestionTaken> questionsTaken = new List<QuestionTaken>();
            var correctAnswers = 0;
            var score = 0;
            
            foreach(var a in quiz.Questions)
            {
                var questionTaken = user.QuestionsTaken.Find(b => b.Question == a);
                if (questionTaken == null) return;
                if (questionTaken.AnsweredCorrectly) correctAnswers++;
                score += XpGainedFromQuestion(questionTaken.Question.Difficulty);
                questionsTaken.Add(questionTaken);
            }
            
            var quizTaken = new QuizTaken(quiz, correctAnswers, score, questionsTaken);
            await _db.QuizzesTaken.AddAsync(quizTaken);
            
            user.QuizzesTaken.Add(quizTaken);
            user.QuizzesTakenCount++;
            await _um.UpdateAsync(user);
        }

        public static string CategoryFromIndex(int? id)
        {
            if (id == null || id < 0 || id > 23) return null;
            
            string[] category =
            {
                "General Knowledge",      //0
                "Books",                  //1
                "Film",                   //2
                "Music",                  //3
                "Musicals & Theatres",    //4
                "Television",             //5
                "Video Games",            //6
                "Board Games",            //7
                "Science & Nature",       //8
                "Computers",              //9
                "Mathematics",            //10
                "Mythology",              //11
                "Sports",                 //12
                "Geography",              //13
                "History",                //14
                "Politics",               //15
                "Art",                    //16
                "Celebrities",            //17
                "Animals",                //18
                "Vehicles",               //19
                "Comics",                 //20
                "Gadgets",                //21
                "Japanese Anime & Manga", //22
                "Cartoon & Animations"    //23
            };

            return category[(int) id];
        }

        // Checks if the alternatives of type multiple choice actually have 4 items
        public bool QuestionsValid(List<Question> questions)
        {
            foreach (var question in questions)
            {
                if (question.QuestionType == "multiple_choice")
                {
                    string[] arr = JsonConvert.DeserializeObject<string[]>(question.Alternatives);
                    if (arr.Length != 4) return false;
                }
            }

            return true;
        }
    }
}