using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Qwiz.Models;
using Qwiz.Models.QuestionTypes;

namespace Qwiz.Data
{
    public static class ApplicationDbInitializer
    {
        public static void Initialize(ApplicationDbContext db, UserManager<ApplicationUser> um, RoleManager<IdentityRole> rm)
        {
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            
            var user = new ApplicationUser { UserName = "user@uia.no", Email = "user@uia.no"};
            um.CreateAsync(user, "Password1.").Wait();
            db.SaveChanges();

            var question1 = new MultipleChoice("What color is grass?", "green", "yellow", "brown", "white", "A");
            var question2 = new TrueFalse("Is the grass green?", "true");

            db.MultipleChoices.Add(question1);
            db.TrueFalses.Add(question2);
            
            db.Questions.AddRangeAsync(question1, question2);
            db.SaveChanges();
            
            var questions = new List<Question>() {question1, question2};
            var quiz = new Quiz(user, questions, "Category", "Topic", "Description");

            db.Quizzes.AddRangeAsync(quiz);
            db.SaveChanges();
        }
    }
}