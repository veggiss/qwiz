using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Qwiz.Models;

namespace Qwiz.Data
{
    public static class ApplicationDbInitializer
    {
        public static async Task Initialize(ApplicationDbContext db, UserManager<ApplicationUser> um, RoleManager<IdentityRole> rm)
        {
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            
            var user = new ApplicationUser {FirstName = "Admin", LastName = "Boss", UserName = "user123", Email = "user@uia.no"};
            await um.CreateAsync(user, "Password1.");
            await db.SaveChangesAsync();
            
            var question1 = new Question("multiple_choice", "What color is grass?", "[\"a\", \"b\", \"c\", \"d\"]", "A", "hard", "");
            var question2 = new Question("true_false", "Is grass green?", null, "true", "easy", "");
            
            await db.Questions.AddRangeAsync(question1, question2);
            await db.SaveChangesAsync();
            
            var questions = new List<Question>() {question1, question2};
            var quiz = new Quiz(user, questions, "Category", "Topic", "Description", "easy");

            await db.Quizzes.AddAsync(quiz);
            await db.SaveChangesAsync();

            // Get questions from open trivia DB API
            for (var i = 0; i < 20; i++)
            {
                dynamic apiResponse = await GetRandomQuestion(1);
                
                var apiQuestions = new List<Question>();
                
                foreach (var q in apiResponse.results)
                {
                    if (q == null) continue;
                    
                    string text = System.Web.HttpUtility.HtmlDecode((string) q.question);
                    string qType = q.type;
                    string qDifficulty = q.difficulty;
                    
                    Question question;
                    if (qType == "multiple") {
                        string alt = "[\"" + q.correct_answer + "\",\"" + q.incorrect_answers[0] + "\",\"" + q.incorrect_answers[1] + "\",\"" + q.incorrect_answers[2] + "\"]";
                        question = new Question("multiple_choice", text, alt, "A", qDifficulty, "");
                    } 
                    else
                    {
                        string answer = q.correct_answer;
                        question = new Question("true_false", text, null, answer.ToLower(), qDifficulty, "");
                    }
                    
                    apiQuestions.Add(question);
                    await db.Questions.AddRangeAsync(question);
                    db.SaveChanges();
                }
                
                await db.Quizzes.AddRangeAsync(new Quiz(user, apiQuestions, "Random", "Random", "Random", "easy"));
                db.SaveChanges();
            }
        }

        private static async Task<object> GetRandomQuestion(int amount)
        {
            using (var client = new HttpClient())
            {
                object product;
                client.BaseAddress = new Uri("https://opentdb.com/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync("api.php?amount=" + amount);
                
                if (response.IsSuccessStatusCode) product = await response.Content.ReadAsAsync<object>();
                else product = null;

                return product;
            }
        }
    }
}