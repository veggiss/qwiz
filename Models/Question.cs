using System.ComponentModel.DataAnnotations;

namespace Qwiz.Models
{
    public class Question
    {
        public Question() {}

        public Question(string type, string text, string alt, string answer, string difficulty, string imagePath)
        {
            QuestionType = type;
            QuestionText = text;
            Alternatives = alt;
            CorrectAnswer = answer;
            Difficulty = difficulty;
            ImagePath = imagePath;
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
        [RegularExpression("^(^[A-D]|true|false)$", ErrorMessage = "Answer type not accepted")]
        public string CorrectAnswer { get; set; }
        [MaxLength(64)]
        public string ImagePath { get; set; }
        [Required]
        [RegularExpression("^(easy|medium|hard)$", ErrorMessage = "Difficulty type not accepted")]
        public string Difficulty { get; set; }
    }
}