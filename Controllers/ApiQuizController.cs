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

        // TODO: Return startpage if page is null and endpage if page exceeds questions length
        [HttpGet("getQuestion")]
        public async Task<IActionResult> GetQuestion(int? id, int? page)
        {
            if (id == null) return BadRequest();
            
            var quiz = await _db.Quizzes
                .Include(q => q.Questions)
                .SingleOrDefaultAsync(q => q.Id == id);
            
            if (quiz == null) return BadRequest();
            if (page == null || page > quiz.Questions.Count) 
                return PartialView("Play/_QuestionIntroPartial", quiz);
            
            var question = quiz.Questions.ElementAt((int) page);
            if (question == null) return BadRequest();

            return PartialView("Play/_QuestionPartial", question);
        }

        // TODO: Costly operation?
        [HttpGet("getQuizSummary")]
        [Authorize]
        public async Task<IActionResult> GetQuizSummary(int? id)
        {
            if (id == null) return BadRequest();

            var user = await _db.Users
                .Include(u => u.QuizzesTaken)
                .ThenInclude(q => q.Quiz)
                .SingleOrDefaultAsync(u => u.Id == _um.GetUserId(User));
            
            if (user == null) return BadRequest();

            var quizTaken = user.QuizzesTaken.Find(q => q.Quiz.Id == id);
            if (quizTaken == null) return BadRequest();

            return PartialView("Play/_QuizSummaryPartial", quizTaken);
        }

        [HttpGet("getQuizList")]
        public async Task<IActionResult> GetQuizList(string username, int page, int size, string type, int? categoryIndex, string difficulty, string orderBy, string search)
        {
            if (page < 1 || size > 20) return BadRequest();

            if (type == "history" && username != null)
            {
                var query = await _um.Users
                    .Where(u => u.UserName == username)
                    .SelectMany(q => q.QuizzesTaken)
                    .Include(q => q.Quiz)
                    .ThenInclude(q => q.Questions)
                    .Include(q => q.Quiz)
                    .ThenInclude(q => q.Owner).ToListAsync();
                
                var entries = query.Skip((page - 1) * size).Take(size).ToList();
                var totalPages = (int) Math.Ceiling(decimal.Divide(query.Count, size));
                
                return PartialView("Profile/_HistoryCardPartial", new HistoryCardModel(entries, totalPages));
            }
            
            if (type == "quizzesBy" && username != null)
            {
                var query = await _db.Quizzes
                    .Include(q => q.Owner)
                    .Where(q => q.Owner.UserName == username).ToListAsync();
                
                var partialString = (await _um.GetUserAsync(User)).UserName == username ? "Profile/_MyQuizPartial" : "Quiz/_QuizCardPartial";
                var entries = query.Skip((page - 1) * size).Take(size).ToList();
                var totalPages = (int) Math.Ceiling(decimal.Divide(query.Count, size));
                
                return PartialView(partialString, new QuizCardModel(entries, totalPages));
            }

            if (type == "category")
            {
                var category =  CategoryFromIndex(categoryIndex);
                var query = await _db.Quizzes
                    .Include(q => q.Owner).ToListAsync();

                if (category != null) 
                    query = query.Where(q => q.Category == category).ToList();
                
                if (difficulty != null) 
                    query = query.Where(q => q.Difficulty == difficulty).ToList();
                
                if (search != null)
                {
                    var searchArr = Regex.Split(search.ToLower(), @"\s+").Where(s => s != string.Empty);
                    var queryTopic = query.Where(q => searchArr.Any(q.Topic.ToLower().Contains)).ToList();
                    var queryUserName = query.Where(q => searchArr.Any(q.Owner.UserName.ToLower().Contains)).ToList();
                    var queryDescription = query.Where(q => searchArr.Any(q.Description.ToLower().Contains)).ToList();
                    var queryCategory = query.Where(q => searchArr.Any(q.Category.ToLower().Contains)).ToList();

                    List<Quiz> searchList = new List<Quiz>();
                    searchList.AddRange(queryTopic);
                    searchList.AddRange(queryUserName);
                    searchList.AddRange(queryDescription);
                    searchList.AddRange(queryCategory);
                    query = searchList.Distinct().ToList();
                }
                
                if (orderBy != null)
                {
                    if (orderBy == "views") query = query.OrderByDescending(q => q.Views).ToList();
                    if (orderBy == "upvotes") query = query.OrderByDescending(q => q.Upvotes).ToList();
                    if (orderBy == "recent") query = query.OrderByDescending(q => q.CreationDate).ToList();
                }
                
                var entries = query.Skip((page - 1) * size).Take(size).ToList();
                var totalPages = (int) Math.Ceiling(decimal.Divide(query.Count, size));
                
                return PartialView("Quiz/_QuizCardPartial", new QuizCardModel(entries, totalPages));
            }

            return BadRequest();
        }
        
        [HttpGet("answer")]
        public async Task<IActionResult> CheckAnswer(int quizId, int questionId, string guess)
        {
            var question = await _db.Questions.FindAsync(questionId);
            if (question == null) return BadRequest("Couldn't find that question");

            var userId = _um.GetUserId(User);
            
            if (userId != null) {
                var user = await _db.Users
                    .Include(u => u.QuestionsTaken)
                    .Include(u => u.QuizzesTaken)
                    .ThenInclude(q => q.Quiz)
                    .SingleOrDefaultAsync(u => u.Id == userId);

                if (user.QuestionsTaken.Find(q => q.Question == question) == null)
                {
                    var answeredCorrectly = guess == question.CorrectAnswer;
                    AddQuestionTaken(user, question, answeredCorrectly);
                    if (answeredCorrectly)
                        AddExperience(user, XpGainedFromQuestion(question.Difficulty));
                }

                UpdateQuizTaken(user, quizId, question);
            }
            
            return Ok(new {correctAnswer = question.CorrectAnswer});
        }
        
        // TODO: Change to a websocket solution?
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
        // TODO: Should be tested for a possible way to modify static properties
        public async Task<IActionResult> UpdateQuiz([FromBody] Quiz quizForm)
        {
            // Unbinds properties from the model, these should not be touched
            Unbind(ModelState, "CreationDate", "Views", "Upvotes", "Owner");
            
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

        // TODO: can we optimize these queries?
        private async void UpdateQuizTaken(ApplicationUser user, int id, Question question)
        {
            Quiz quiz = await _db.Quizzes
                .Include(m => m.Questions)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (quiz == null) return;
            if (!quiz.Questions.Exists(q => q == question)) return;
            if (user.QuizzesTaken.Exists(q => q.Quiz == quiz)) return;

            var correctAnswers = 0;
            var score = 0;
            
            foreach(var a in quiz.Questions)
            {
                var questionTaken = user.QuestionsTaken.Find(b => b.Question == a);
                if (questionTaken == null) return;
                if (questionTaken.AnsweredCorrectly) correctAnswers++;
                score += XpGainedFromQuestion(questionTaken.Question.Difficulty);
            }
            
            user.QuizzesTaken.Add(new QuizTaken(quiz, correctAnswers, score));
            user.QuizzesTakenCount++;
            await _um.UpdateAsync(user);;
        }

        public static string CategoryFromIndex(int? id)
        {
            if (id == null || id < 0 || id > 23) return null;
            
            string[] category =
            {
                "General Knowledge",                     //0
                "Entertainment: Books",                  //1
                "Entertainment: Film",                   //2
                "Entertainment: Music",                  //3
                "Entertainment: Musicals & Theatres",    //4
                "Entertainment: Television",             //5
                "Entertainment: Video Games",            //6
                "Entertainment: Board Games",            //7
                "Science &amp; Nature",                  //8
                "Science: Computers",                    //9
                "Science: Mathematics",                  //10
                "Mythology",                             //11
                "Sports",                                //12
                "Geography",                             //13
                "History",                               //14
                "Politics",                              //15
                "Art",                                   //16
                "Celebrities",                           //17
                "Animals",                               //18
                "Vehicles",                              //19
                "Entertainment: Comics",                 //20
                "Science: Gadgets",                      //21
                "Entertainment: Japanese Anime & Manga", //22
                "Entertainment: Cartoon & Animations"    //23
            };

            return category[(int) id];
        }
        
        private static void Unbind(ModelStateDictionary modelState, params string[] modelProperties)
        {
            foreach (var prop in modelProperties)
                modelState.Remove(prop);
        }
    }
}