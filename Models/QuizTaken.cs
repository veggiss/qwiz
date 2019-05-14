namespace Qwiz.Models
{
    public class QuizTaken
    {
        public QuizTaken() {}
        public QuizTaken(Quiz quiz, int correctAnswers, int score)
        {
            Quiz = quiz;
            CorrectAnswers = correctAnswers;
            Score = score;
        }
        
        public int Id { get; set; }
        public Quiz Quiz { get; set; }
        public int CorrectAnswers { get; set; }
        public int Score { get; set; }
    }
}