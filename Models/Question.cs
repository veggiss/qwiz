namespace Qwiz.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string QuestionType { get; set; }
        public string QuestionText { get; set; }
        public string CorrectAnswer { get; set; }
    }
}