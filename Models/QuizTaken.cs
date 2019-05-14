using System;
using System.Collections.Generic;

namespace Qwiz.Models
{
    public class QuizTaken
    {
        public QuizTaken() {}
        public QuizTaken(Quiz quiz, int correctAnswers, int score, List<QuestionTaken> questionsTaken)
        {
            Quiz = quiz;
            CorrectAnswers = correctAnswers;
            Score = score;
            QuestionsTaken = questionsTaken;
        }
        
        public int Id { get; set; }
        public Quiz Quiz { get; set; }
        public List<QuestionTaken> QuestionsTaken { get; set; }
        public int CorrectAnswers { get; set; }
        public int Score { get; set; }
        public DateTime DateTaken { get; set; } = DateTime.Now;
    }
}