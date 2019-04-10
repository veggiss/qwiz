namespace Qwiz.Models
{
    public class Question
    {
        public Question() {}

        public Question(string type, string text, string alt, string answer)
        {
            QuestionType = type;
            QuestionText = text;
            Alternatives = alt;
            CorrectAnswer = answer;

        }
        
        public int Id { get; set; }
        public string QuestionType { get; set; }
        public string QuestionText { get; set; }
        public string Alternatives { get; set; }
        public string CorrectAnswer { get; set; }
    }
}