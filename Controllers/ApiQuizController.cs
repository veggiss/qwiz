using Microsoft.AspNetCore.Mvc;
using Qwiz.Data;

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
    }
}