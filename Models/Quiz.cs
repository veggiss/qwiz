using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Qwiz.Models
{
    public class Quiz
    {
        public Quiz() {}

        public Quiz(ApplicationUser user, List<Question> questions, string category, string topic, string description)
        {
            Owner = user;
            Questions = questions;
            Category = category;
            Topic = topic;
            Description = description;
        }
        
        public int Id { get; set; }
        private ApplicationUser Owner { get; set; }
        public List<Question> Questions { get; set; }
        public string Category { get; set; }
        public string Topic { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
    }
}