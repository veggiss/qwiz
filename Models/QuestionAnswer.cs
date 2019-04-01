namespace Qwiz.Models
{
    public class QuestionAnswer
    {
        public QuestionAnswer() {}

        public QuestionAnswer(string question, string a, string b, string c, string d, string answer)
        {
            Question = question;
            AnswerA = a;
            AnswerB = b;
            AnswerC = c;
            AnswerD = d;
            CorrectAnswer = answer;
        }
        
        public int Id { get; set; }
        public string Question { get; set; }
        public string AnswerA { get; set; }
        public string AnswerB { get; set; }
        public string AnswerC { get; set; }
        public string AnswerD { get; set; }
        public string CorrectAnswer { get; set; }
    }
}