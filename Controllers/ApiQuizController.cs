using System;
using Microsoft.AspNetCore.Mvc;
using Qwiz.Data;
using Qwiz.Models;

namespace Qwiz.Controllers
{
    [Route("api")]
    // ApiController enables automatic model validation
    [ApiController]
    public class ApiQuizController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        
        public ApiQuizController(ApplicationDbContext db)
        {
            _db = db;
        }
        
        // api/answer?id=1
        [HttpGet("answer")]
        public IActionResult Get(int id)
        {
            var question = _db.QuestionAnswers.Find(id);
            if (question == null) return NotFound();
            
            return Ok(question.CorrectAnswer);
        }

        [HttpPost("create")]
        public IActionResult Post(QuizChallenge quizChallenge)
        {
            if (quizChallenge.Id != 0) return BadRequest();

            _db.Add(quizChallenge);
            _db.SaveChanges();

            return Ok(quizChallenge);
        }
    }
}