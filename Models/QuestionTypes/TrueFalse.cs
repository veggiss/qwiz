namespace Qwiz.Models.QuestionTypes
{
    public class TrueFalse : Question
    {
        public TrueFalse() {}
        public TrueFalse(string text, string answer)
        {
            QuestionType = "true_false";
            QuestionText = text;
            CorrectAnswer = answer;
        }
    }
}