using System;

namespace Qwiz.Models
{
    public class PendingMember
    {
        public PendingMember() {}

        public PendingMember(ApplicationUser user)
        {
            User = user;
            Username = user.UserName;
            Level = user.Level;
        }
        
        public int Id { get; set; }
        public ApplicationUser User { get; set; }
        public int Level { get; set; }
        public string Username { get; set; }
        public DateTime RequestDate { get; set; } = DateTime.Now;
    }
}