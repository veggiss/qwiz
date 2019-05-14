using System.Collections.Generic;

namespace Qwiz.Models
{
    public class QuizCardModel
    {
        public QuizCardModel() {}

        public QuizCardModel(List<Quiz> quizList, int totalPages)
        {
            QuizList = quizList;
            TotalPages = totalPages;
        }
        
        public List<Quiz> QuizList { get; set; }
        public int TotalPages { get; set; }
    }
}