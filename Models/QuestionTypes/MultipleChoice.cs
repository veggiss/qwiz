namespace Qwiz.Models.QuestionTypes
{
    public class MultipleChoice : Question
    {
        public MultipleChoice() {}
        
        public MultipleChoice(string text, string a, string b, string c, string d, string answer)
        {
            QuestionType = "multiple_choice";
            QuestionText = text;
            CorrectAnswer = answer;
            AnswerA = a;
            AnswerB = b;
            AnswerC = c;
            AnswerD = d;
        }
        
        public string AnswerA { get; set; }
        public string AnswerB { get; set; }
        public string AnswerC { get; set; }
        public string AnswerD { get; set; }
    }
}