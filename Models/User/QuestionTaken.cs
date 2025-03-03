using System;

namespace Qwiz.Models
{
    public class QuestionTaken
    {
        public QuestionTaken() {}
        public QuestionTaken(Question question, bool answeredCorrectly, int xpGained, int bonus, TimeSpan time, string answer, char alternative)
        {
            Question = question;
            AnsweredCorrectly = answeredCorrectly;
            XpGained = xpGained;
            Bonus = bonus;
            Time = time;
            Answer = answer;
            AnswerAlternative = alternative;
            QuestionId = question.Id;
        }
        
        public int Id { get; set; }
        public Question Question { get; set; }
        public int QuestionId { get; set; }
        public bool AnsweredCorrectly { get; set; }
        public string Answer { get; set; }
        public char AnswerAlternative { get; set; }
        public int XpGained { get; set; }
        public int Bonus { get; set; }
        public TimeSpan Time { get; set; }
        public DateTime DateTaken { get; set; } = DateTime.Now;
        public string DateTakenFormatted { get; set; } = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
    }
}