using System.Collections.Generic;

namespace Qwiz.Models
{
    public class HistoryCardModel
    {
        public HistoryCardModel() {}

        public HistoryCardModel(List<QuizTaken> quizTakenList, int totalPages)
        {
            QuizTakenList = quizTakenList;
            TotalPages = totalPages;
        }
        
        public List<QuizTaken> QuizTakenList { get; set; }
        public int TotalPages { get; set; }
    }
}