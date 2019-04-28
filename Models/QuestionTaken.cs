namespace Qwiz.Models
{
    public class QuestionTaken
    {
        public QuestionTaken() {}
        public QuestionTaken(Question question, bool answeredCorrectly)
        {
            Question = question;
            AnsweredCorrectly = answeredCorrectly;
        }
        
        public int Id { get; set; }
        public Question Question { get; set; }
        public bool AnsweredCorrectly { get; set; }
    }
}