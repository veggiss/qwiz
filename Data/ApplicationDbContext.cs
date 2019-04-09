using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Qwiz.Models;
using Qwiz.Models.QuestionTypes;

namespace Qwiz.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {}
        
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<MultipleChoice> MultipleChoices { get; set; }
        public DbSet<TrueFalse> TrueFalses { get; set; }
    }
}
