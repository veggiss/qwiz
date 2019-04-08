namespace Qwiz.Models.QuestionModels
{
    public class MultipleChoiceMultiple : QuestionModel
    {
        public MultipleChoiceMultiple() {}

        public MultipleChoiceMultiple(string question, string a, string b, string c, string d, string answer)
        {
            Type = "multiple_choice_multiple";
            Question = question;
            AnswerA = a;
            AnswerB = b;
            AnswerC = c;
            AnswerD = d;
            CorrectAnswer = answer;
        }
        
        public string AnswerA { get; set; }
        public string AnswerB { get; set; }
        public string AnswerC { get; set; }
        public string AnswerD { get; set; }
        
    }
}