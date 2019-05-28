using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Qwiz.Data;
using Qwiz.Models;

namespace Qwiz.Controllers.Api
{
    [Route("api/leaderboard")]
    [ApiController]
    public class ApiLeaderboardController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _um;
        
        public ApiLeaderboardController(ApplicationDbContext db, UserManager<ApplicationUser> um)
        {
            _db = db;
            _um = um;
        }

        [HttpGet("getList/{type}")]
        public async Task<IActionResult> GetLeaderboard(string type, int size, int page, int? id)
        {
            if (page < 1 || size < 1 || type == null) return BadRequest("Invalid request!");
            
            switch (type)
            {
                case "level":
                {
                    var query = await _db.Users
                        .OrderByDescending(u => u.Level)
                        .ThenByDescending(u => u.Xp).ToListAsync();
                    
                    var entries = query.Select(u => new
                    {
                        u.Xp,
                        u.Score,
                        u.Level,
                        u.UserName,
                        u.ImagePath,
                        u.QuizzesTakenCount
                    }).Skip((page - 1) * size).Take(size).ToList();
                    
                    var pages = (int) Math.Ceiling(decimal.Divide(query.Count, size));
                    
                    return Ok(new {entries, pages});
                }
                
                case "today":
                case "week":
                case "month":
                case "all":
                {
                    DateTime time;
                    
                    if (type == "today") time = DateTime.Today;
                    else if (type == "week") time = StartOfWeek();
                    else if (type == "month") time = StartOfMonth();
                    else time = DateTime.MinValue;
                    
                    if (id == null)
                    {
                        var query = await _db.Users
                            .Include(u => u.QuizzesTaken)
                            .Where(u => u.QuizzesTaken.Any(q => q.DateTaken > time)).ToListAsync();
                    
                        var entries = query.Select(u => new
                        {
                            u.Level,
                            u.UserName,
                            u.ImagePath,
                            quizzesTakenCount = u.QuizzesTaken.Count(q => q.DateTaken > time),
                            score = u.QuizzesTaken.Where(q => q.DateTaken > time).Sum(q => q.Score)
                        }).OrderByDescending(q => q.score).Skip((page - 1) * size).Take(size).ToList();

                        var pages = (int) Math.Ceiling(decimal.Divide(query.Count, size));
                    
                        return Ok(new {entries, pages});
                    }
                    else
                    {
                        var query = await _db.QuizzesTaken
                            .Include(q => q.Quiz)
                            .Where(q => q.Quiz.Id == id && q.DateTaken > time).ToListAsync();

                        var entries = query.Select(q => new
                        {
                            q.Score,
                            q.TakerUsername,
                            q.CorrectAnswers,
                            q.QuestionsLength,
                            q.DateTakenFormatted,
                            user = _um.Users.Where(u => u.UserName == q.TakerUsername).Select(u => new
                            {
                                u.Level,
                                u.ImagePath
                            }).FirstOrDefault()
                        }).OrderByDescending(q => q.Score).ThenByDescending(q => q.CorrectAnswers).ToList();
                        
                        var pages = (int) Math.Ceiling(decimal.Divide(query.Count, size));
                    
                        return Ok(new {entries, pages});
                    }
                }
                
                default:
                    return BadRequest("Type or id not found!");
            }
        }
        
        // Gets the date of monday this week
        private DateTime StartOfWeek()
        {
            return DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);
        }

        // Gets the first day of current month
        private DateTime StartOfMonth()
        {
            DateTime now = DateTime.Now;
            return new DateTime(now.Year, now.Month, 1);
        }
    }
}