using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Qwiz.Models;
using Qwiz.Models.QuestionModels;

namespace Qwiz.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {}
        
        public DbSet<QuizModel> Quizzes { get; set; }
        public DbSet<QuestionModel> Questions { get; set; }
        public DbSet<MultipleChoiceSingle> MultipleChoiceSingles { get; set; }
        public DbSet<MultipleChoiceMultiple> MultipleChoiceMultiples { get; set; }
        public DbSet<TrueFalse> TrueFalses { get; set; }
    }
}
