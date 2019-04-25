using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language;
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
        public async Task<IActionResult> CheckAnswer(int id, string guess)
        {
            var question = _db.Questions.Find(id);
            if (question == null) return NotFound();

            if (guess == question.CorrectAnswer)
            {
                var user = await _um.GetUserAsync(User);
                AddExperience(user, question.Difficulty); 
            }

            return Ok(question.CorrectAnswer);
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
    }
}