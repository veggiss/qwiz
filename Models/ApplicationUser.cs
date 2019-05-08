using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Qwiz.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(64, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Use letters only please")]
        public string FirstName { get; set; }
        [Required]
        [StringLength(64, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Use letters only please")]
        public string LastName { get; set; }
        public int Xp { get; set; }
        public int Level { get; set; } = 1;
        public int QuizzesTakenCount { get; set; }
        public int XpNeeded { get; set; } = 500;
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        public DateTime LastActivity { get; set; } = DateTime.Now;
        public List<QuestionTaken> QuestionsTaken { get; set; } = new List<QuestionTaken>();
        public List<QuizTaken> QuizzesTaken { get; set; } = new List<QuizTaken>();
        public List<Quiz> MyQuizzes { get; set; } = new List<Quiz>();
    }
}