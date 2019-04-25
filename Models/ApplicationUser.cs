using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Qwiz.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Xp { get; set; }
        public int Level { get; set; } = 1;
        public int XpNeeded { get; set; } = 500;
    }
}