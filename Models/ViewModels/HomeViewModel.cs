using System.Collections.Generic;

namespace Qwiz.Models
{
    
    public class HomeViewModel
    
    {
        public HomeViewModel(int quizCount, int userCount)
        {
            QuizCount = quizCount;
            UserCount = userCount;
        }
        public int QuizCount { get; set; }
        public int UserCount { get; set; }
    }
    
    
}