using System.Collections.Generic;

namespace Qwiz.Models
{
    public class QuizChallenge
    {
        public QuizChallenge() {}

        public QuizChallenge(ApplicationUser user, List<QuestionAnswer> questions, string category, string topic, string description)
        {
            Owner = user;
            Questions = questions;
            Category = category;
            Topic = topic;
            Description = description;
        }
        
        public int Id { get; set; }
        public ApplicationUser Owner { get; set; }
        public List<QuestionAnswer> Questions { get; set; }
        public string Category { get; set; }
        public string Topic { get; set; }
        public string Description { get; set; }
    }
}