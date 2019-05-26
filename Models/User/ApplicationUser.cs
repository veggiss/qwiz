using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Qwiz.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(64, ErrorMessage = "Must be at least 2 and at max 64 characters long.", MinimumLength = 2)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Only standard letters allowed.")]
        public string FirstName { get; set; }
        [Required]
        [StringLength(64, ErrorMessage = "Must be at least 2 and at max 64 characters long.", MinimumLength = 2)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Only standard letters allowed.")]
        public string LastName { get; set; }
        public int Xp { get; set; }
        public int Level { get; set; } = 1;
        public int QuizzesTakenCount { get; set; }
        public int XpNeeded { get; set; } = 1500;
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        public DateTime LastActivity { get; set; } = DateTime.Now;
        // TODO: Still complies with REST?
        public DateTime LastQuestionStarted { get; set; } = DateTime.Now;
        public List<QuestionTaken> QuestionsTaken { get; set; } = new List<QuestionTaken>();
        public List<QuizTaken> QuizzesTaken { get; set; } = new List<QuizTaken>();
        public List<Quiz> MyQuizzes { get; set; } = new List<Quiz>();
        public List<Group> MyGroups { get; set; } = new List<Group>();
    }
}