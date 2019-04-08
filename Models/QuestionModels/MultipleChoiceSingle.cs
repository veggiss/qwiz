namespace Qwiz.Models.QuestionModels
{
    public class MultipleChoiceSingle : Question
    {
        public MultipleChoiceSingle() {}
        
        public MultipleChoiceSingle(string text, string a, string b, string c, string d, string answer)
        {
            QuestionType = "multiple_choice_single";
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