using Microsoft.AspNetCore.Identity;

namespace Qwiz.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int QuizScore { get; set; }
    }
}