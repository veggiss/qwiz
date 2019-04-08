using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Qwiz.Models;
using Qwiz.Models.QuestionModels;

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

            var question1 = new MultipleChoiceSingle("What color is grass?", "green", "yellow", "brown", "white", "A");
            var question2 = new MultipleChoiceMultiple("What color is grass?", "green", "green", "brown", "white", "A,B");
            var question3 = new TrueFalse("Is the grass green?", "true");

            db.MultipleChoiceSingles.Add(question1);
            db.MultipleChoiceMultiples.Add(question2);
            db.TrueFalses.Add(question3);
            
            db.Questions.AddRangeAsync(question1, question2, question3);
            db.SaveChanges();
            
            var questions = new List<Question>() {question1, question2, question3};
            var quiz = new Quiz(user, questions, "Math", "Addition", "Quiz about addition");

            db.Quizzes.AddRangeAsync(quiz);
            db.SaveChanges();
        }
    }
}