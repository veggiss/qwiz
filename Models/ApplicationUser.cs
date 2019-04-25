using Microsoft.AspNetCore.Identity;

namespace Qwiz.Models
{
    public class ApplicationUser : IdentityUser
    {
       
        
        public string FirstName { get; set; }
        public string LastName { get; set; }
        
        public string Nickname { get; set; }
        public int QuizScore { get; set; }
        
        
        public string ApplicationUserId { get; set; }
        
        public ApplicationUser ProfileUser{ get; set; }
        
        
        /*test
        public ApplicationUser OwnerId { get; set; }
        
        public int Id { get; set; }*/
    }
}