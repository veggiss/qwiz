using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        
        public ApiQuizController(ApplicationDbContext db)
        {
            _db = db;
        }
        
        // api/answer?id=1
        [HttpGet("answer")]
        public IActionResult CheckAnswer(int id)
        {
            var question = _db.Questions.Find(id);
            if (question == null) return NotFound();
            
            return Ok(question.CorrectAnswer);
        }

        [HttpPost("create")]
        public IActionResult CreateQuiz([FromBody] Quiz quiz)
        {
            //var value = obj.questions;
            //Console.WriteLine(value[0].questionType);
            
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
    }
}