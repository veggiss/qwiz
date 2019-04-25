using System.Collections.Generic;

namespace Qwiz.Models
{
    public class QuizViewModel
    {
        public QuizViewModel(int id, List<Question> questions)
        {
            Id = id;
            Questions = questions;
        }
        
        public int Id { get; set; }
        public List<Question> Questions { get; set; }
    }
}