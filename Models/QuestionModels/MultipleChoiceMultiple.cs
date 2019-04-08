namespace Qwiz.Models.QuestionModels
{
    public class MultipleChoiceMultiple : Question
    {
        public MultipleChoiceMultiple() {}

        public MultipleChoiceMultiple(string text, string a, string b, string c, string d, string answer)
        {
            QuestionType = "multiple_choice_multiple";
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