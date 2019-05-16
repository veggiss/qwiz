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
            Score = score;
            QuizId = quiz.Id;
            CorrectAnswers = correctAnswers;
            QuestionsTaken = questionsTaken;
            QuestionsLength = quiz.Questions.Count;
        }
        public int Id { get; set; }
        public int QuizId { get; set; }
        public Quiz Quiz { get; set; }
        public List<QuestionTaken> QuestionsTaken { get; set; }
        public int QuestionsLength { get; set; }
        public int CorrectAnswers { get; set; }
        public int Score { get; set; }
        public DateTime DateTaken { get; set; } = DateTime.Now;
        public string DateTakenFormatted { get; set; } = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
    }
}