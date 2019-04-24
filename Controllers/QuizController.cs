using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Qwiz.Data;

namespace Qwiz.Controllers
{
    public class QuizController : Controller
    {
        private readonly ApplicationDbContext _db;
        
        public QuizController(ApplicationDbContext db)
        {
            _db = db;
        }
        
        public async Task<IActionResult> Index()
        {
            var quizzes = _db.Quizzes;
            
            return View(await quizzes.ToListAsync());
        }

        public async Task<IActionResult> Play(int? id)
        {
            var quizzes = await _db.Quizzes
                .Include(c => c.Questions)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (quizzes == null) return NotFound();

            var questions = quizzes.Questions;
            if (questions == null) return NotFound();
            questions.ForEach(item => item.CorrectAnswer = "");
            
            return View(questions.ToList());
        }

        public IActionResult Create()
        {
            return View();
        }
    }
}