namespace Qwiz.Models
{
    public class Upvote
    {
        public Upvote() {}

        public Upvote(string username, int quizId)
        {
            Username = username;
            QuizId = quizId;
        }
        
        public int Id { get; set; }
        public int QuizId { get; set; }
        public string Username { get; set; }
    }
}