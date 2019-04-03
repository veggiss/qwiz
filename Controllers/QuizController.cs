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
            var quizList = _db.QuizChallenges;
            
            return View(await quizList.ToListAsync());
        }

        public async Task<IActionResult> Play(int? id)
        {
            var quizChallenge = await _db.QuizChallenges
                .Include(c => c.Questions)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (quizChallenge == null) return NotFound();
            
            var questionAnswer = quizChallenge.Questions;
            if (questionAnswer == null) return NotFound();
            questionAnswer.ForEach(item => item.CorrectAnswer = "");
            
            return View(questionAnswer.ToList());
        }

        public IActionResult Create()
        {
            return View();
        }
        
        public ActionResult QuizMakerPartial()
        {
            return PartialView("_quizMakerPartial");
        }
    }
}