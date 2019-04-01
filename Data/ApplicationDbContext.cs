using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Qwiz.Models;

namespace Qwiz.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {}
        
        public DbSet<QuestionAnswer> QuestionAnswers { get; set; }
        public DbSet<QuizChallenge> QuizChallenges { get; set; }
    }
}
