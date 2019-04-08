using System.Collections.Generic;

namespace Qwiz.Models
{
    public class QuizModel
    {
        public QuizModel() {}

        public QuizModel(ApplicationUser user, List<QuestionModel> questions, string category, string topic, string description)
        {
            Owner = user;
            Questions = questions;
            Category = category;
            Topic = topic;
            Description = description;
        }
        
        public int Id { get; set; }
        private ApplicationUser Owner { get; set; }
        public List<QuestionModel> Questions { get; set; }
        public string Category { get; set; }
        public string Topic { get; set; }
        public string Description { get; set; }
    }
}