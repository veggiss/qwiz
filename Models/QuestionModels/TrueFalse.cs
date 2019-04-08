namespace Qwiz.Models.QuestionModels
{
    public class TrueFalse : QuestionModel
    {
        public TrueFalse() {}
        public TrueFalse(string question, string answer)
        {
            Type = "true_false";
            Question = question;
            CorrectAnswer = answer;
        }
    }
}