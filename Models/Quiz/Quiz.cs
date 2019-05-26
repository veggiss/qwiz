using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Qwiz.Models
{
    public class Quiz
    {
        public Quiz() {}

        public Quiz(ApplicationUser user, List<Question> questions, string category, string topic, string description, string difficulty)
        {
            Owner = user;
            Questions = questions;
            Category = category;
            Topic = topic;
            Description = description;
            Difficulty = difficulty;
            OwnerUsername = user.UserName;
        }
        
        public int Id { get; set; }
        public string OwnerId { get; set; }
        public string OwnerUsername { get; set; }
        public ApplicationUser Owner { get; set; }
        [Required]
        public List<Question> Questions { get; set; }
        [Required]
        [MaxLength(128)]
        public string Category { get; set; }
        [Required]
        [MaxLength(128)]
        public string Topic { get; set; }
        [Required]
        [MaxLength(128)]
        public string Description { get; set; }
        [MaxLength(128)]
        public string ImagePath { get; set; }
        [Required]
        [RegularExpression("^(easy|medium|hard)$", ErrorMessage = "Difficulty type not accepted")]
        public string Difficulty { get; set; }
        public int Upvotes { get; set; }
        public int Views { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public string CreationDateFormatted { get; set; } = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
    }
}