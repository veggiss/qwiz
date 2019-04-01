using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Qwiz.Models;

namespace Qwiz.Data
{
    public class ApplicationDbInitializer
    {
        public static void Initialize(ApplicationDbContext db, UserManager<ApplicationUser> um, RoleManager<IdentityRole> rm)
        {
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            
            var user = new ApplicationUser { UserName = "user@uia.no", Email = "user@uia.no"};
            um.CreateAsync(user, "Password1.").Wait();

            // Save user and role changes to the database
            db.SaveChanges();

            var question1 = new QuestionAnswer("What is 5 + 5?", "2", "10", "5", "9", "b");
            var question2 = new QuestionAnswer("What is 1 + 1?", "2", "10", "5", "9", "a");
            var question3 = new QuestionAnswer("What is 4 + 1?", "2", "10", "5", "9", "c");
            var question4 = new QuestionAnswer("What is 4 + 5?", "2", "10", "5", "9", "d");
            
            db.QuestionAnswers.AddRangeAsync(question1, question2, question3, question4);

            db.SaveChanges();

            var questionList = new List<QuestionAnswer>() {question1, question2, question3, question4};
            var quizChallenge = new QuizChallenge(user, questionList, "Math", "Addition", "Quiz about addition");
            
            db.QuizChallenges.AddRangeAsync(quizChallenge);

            db.SaveChanges();
        }
    }
}