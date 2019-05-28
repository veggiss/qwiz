using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using Qwiz.Data;
using Qwiz.Models;

namespace Qwiz.Controllers.Api
{
    [Route("api/question")]
    [ApiController]
    public class ApiQuestionController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _um;
        
        public ApiQuestionController(ApplicationDbContext db, UserManager<ApplicationUser> um)
        {
            _db = db;
            _um = um;
        }
        
        // TODO: Would it still be rest if we check if the user HAS started a question here? As is, the user can time stamp whenever
        [HttpPut("startTimer")]
        [Authorize]
        public async void StartTimer()
        {
            var user = await _um.GetUserAsync(User);
            
            if (user != null) {
                user.LastQuestionStarted = DateTime.Now;
                await _um.UpdateAsync(user);
            }
        }

        [HttpPut("answer")]
        public async Task<IActionResult> CheckAnswer(int? quizId, int? questionId, char guessAlternative)
        {
            if (quizId == null || questionId == null || guessAlternative == 0) return BadRequest();
            
            var question = await _db.Questions.FindAsync(questionId);
            if (question == null) return BadRequest();

            var userId = _um.GetUserId(User);
            var xpGained = 0;
            var bonus = 0;
            
            // If user is authorized
            if (userId != null) {
                var user = await _db.Users
                    .Include(u => u.QuestionsTaken)
                    .Include(u => u.QuizzesTaken)
                    .Include(u => u.MyQuizzes)
                    .SingleOrDefaultAsync(u => u.Id == userId);
                
                // Check if user has answered this question before
                if (!user.QuestionsTaken.Exists(q => q.QuestionId == questionId)){
                    // Check answer by alternative
                    var answeredCorrectly = guessAlternative == question.CorrectAlternative;
                    var timer = DateTime.Now - user.LastQuestionStarted;
                    var timerSec = (int) Math.Round(timer.TotalSeconds);
                    var isOwner = user.MyQuizzes.Any(q => q.Id == quizId);
                    
                    // If answered within time frame
                    if (answeredCorrectly && timerSec >= 0 && timerSec <= 15 && !isOwner)
                    {
                        xpGained = QuizUtil.XpGainedFromQuestion(question.Difficulty);
                        bonus = (int) (xpGained * (((15f - timerSec) / 15f * 100f) / 100f));
                        AddExperience(user, xpGained + bonus);
                    }
                    
                    AddQuestionTaken(user, question, answeredCorrectly, xpGained, bonus, timer, guessAlternative);
                }
                
                // Check if all the questions of the quiz has been answered
                UpdateQuizTaken(user, quizId, questionId);
            }
            
            return Ok(new {correctAlternative = question.CorrectAlternative, xpGained, bonus});
        }
        
        private async void UpdateQuizTaken(ApplicationUser user, int? quizId, int? questionId)
        {
            Quiz quiz = await _db.Quizzes
                .Include(m => m.Questions)
                .FirstOrDefaultAsync(i => i.Id == quizId);
            
            if (quiz == null) return;
            if (!quiz.Questions.Exists(q => q.Id == questionId)) return;
            if (user.QuizzesTaken.Exists(q => q.QuizId == quizId)) return;
            
            List<QuestionTaken> questionsTaken = new List<QuestionTaken>();
            var correctAnswers = 0;
            var score = 0;
            
            foreach(var a in quiz.Questions)
            {
                var questionTaken = user.QuestionsTaken.Find(b => b.QuestionId == a.Id);
                if (questionTaken == null) return;
                if (questionTaken.AnsweredCorrectly) correctAnswers++;
                score += questionTaken.XpGained + questionTaken.Bonus;
                questionsTaken.Add(questionTaken);
            }

            quiz.Views++;
            var quizTaken = new QuizTaken(quiz, correctAnswers, score, questionsTaken, user.UserName);
            await _db.QuizzesTaken.AddAsync(quizTaken);
            
            user.QuizzesTaken.Add(quizTaken);
            user.QuizzesTakenCount++;
            await _um.UpdateAsync(user);
            await _db.SaveChangesAsync();
        }
        
        private async void AddExperience(ApplicationUser user, int amount)
        {
            user.Score += amount;
            user.Xp += amount;
            
            if (user.XpNeeded <= user.Xp) {
                user.Level++;
                user.Xp -= user.XpNeeded;
                user.XpNeeded = (int) Math.Round(user.XpNeeded * 1.1);
            }
            
            await _um.UpdateAsync(user);
        }
        
        private async void AddQuestionTaken(ApplicationUser user, Question question, bool answeredCorrectly, int xpGained, int bonus, TimeSpan timer, char alternative)
        {
            var answer = GetAnswerFromAlternative(alternative, question);
            if (answer == null) return;
            
            var questionTaken = new QuestionTaken(question, answeredCorrectly, xpGained, bonus, timer, answer, alternative);
            await _db.QuestionsTaken.AddAsync(questionTaken);
            user.QuestionsTaken.Add(questionTaken);
            await _um.UpdateAsync(user);
        }

        private string GetAnswerFromAlternative(char? alternative, Question question)
        {
            string[] arr = null;
            
            if (question.QuestionType == "multiple_choice")
                arr = JsonConvert.DeserializeObject<string[]>(question.Alternatives);
            
            switch (alternative)
            {
                case 'A':
                    return arr?[0];
                case 'B':
                    return arr?[1];
                case 'C':
                    return arr?[2];
                case 'D':
                    return arr?[3];
                case 'T':
                    return "True";
                case 'F':
                    return "False";
                
                default: return null;
            }
        }
    }
}