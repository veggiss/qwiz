using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Qwiz.Models;

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
            
            var question1 = new Question("multiple_choice", "What color is grass?", "green|yellow|white|brown", "0");
            var question2 = new Question("true_false", "Is grass green?", null, "true");
            
            db.Questions.AddRangeAsync(question1, question2);
            db.SaveChanges();
            
            var questions = new List<Question>() {question1, question2};
            var quiz = new Quiz(user, questions, "Category", "Topic", "Description");

            db.Quizzes.AddRangeAsync(quiz);
            db.SaveChanges();
        }
    }
}