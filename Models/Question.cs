using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Qwiz.Models
{
    public class Question
    {
        public Question() {}

        public Question(string type, string text, string alt, string answer, string alternative, string difficulty, string imagePath)
        {
            QuestionType = type;
            QuestionText = text;
            CorrectAnswer = answer;
            CorrectAlternative = alternative;
            Difficulty = difficulty;
            ImagePath = imagePath;
            Alternatives = alt;
        }
        
        public int Id { get; set; }
        [Required]
        [RegularExpression("^(multiple_choice|true_false)$", ErrorMessage = "Question type not accepted")]
        public string QuestionType { get; set; }
        [Required]
        [MaxLength(128)]
        public string QuestionText { get; set; }
        public string Alternatives { get; set; }
        [Required]
        [MaxLength(64)]
        public string CorrectAnswer { get; set; }
        [Required]
        [RegularExpression("^(^[A-D]|true|false)$", ErrorMessage = "Answer type not accepted")]
        public string CorrectAlternative { get; set; }
        [MaxLength(64)]
        public string ImagePath { get; set; }
        [Required]
        [RegularExpression("^(easy|medium|hard)$", ErrorMessage = "Difficulty type not accepted")]
        public string Difficulty { get; set; }
    }
}