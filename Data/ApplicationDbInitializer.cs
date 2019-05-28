using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Qwiz.Controllers;
using Qwiz.Models;

namespace Qwiz.Data
{
    public static class ApplicationDbInitializer
    {
        private static ApplicationDbContext _db;
        private static UserManager<ApplicationUser> _um;
        private static readonly Random _ran = new Random();
        
        public static async Task Initialize(ApplicationDbContext db, UserManager<ApplicationUser> um, RoleManager<IdentityRole> rm)
        {
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            _db = db;
            _um = um;
            
            var adminUser = new ApplicationUser {FirstName = "Admin", LastName = "Boss", UserName = "user123", Email = "user@uia.no"};
            await um.CreateAsync(adminUser, "Password1.");
            await db.SaveChangesAsync();

            // Used for debugging and testing
            //await AddRandomUsers();
            //await AddRandomQuizzes();
            //await AddRandomGroups();
        }

        private static async Task<object> GetObjectFromApi(string host, string path)
        {
            using (var client = new HttpClient())
            {
                object product;
                client.BaseAddress = new Uri(host);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync(path);
                
                if (response.IsSuccessStatusCode) product = await response.Content.ReadAsAsync<object>();
                else product = null;

                return product;
            }
        }

        private static async Task AddRandomQuizzes(int amount)
        {
            // Get questions from open trivia DB API
            for (var i = 0; i < amount; i++)
            {
                dynamic randomQuestion = await GetObjectFromApi("https://opentdb.com/", "api.php?amount=10");
                
                if (randomQuestion == null || randomQuestion.response_code != 0) continue;
                
                var apiQuestions = new List<Question>();
                
                foreach (var q in randomQuestion.results)
                {
                    if (q == null) continue;
                    // TODO: Should strings be json stringified?
                    string text = HttpUtility.HtmlDecode((string) q.question);
                    string qType = q.type;
                    string qDifficulty = q.difficulty;
                    string answer = HttpUtility.HtmlDecode((string) q.correct_answer);
                    Question question;
                    
                    if (qType == "multiple")
                    {
                        string[] alternatives =
                        {
                            answer,
                            HttpUtility.HtmlDecode((string) q.incorrect_answers[0]),
                            HttpUtility.HtmlDecode((string) q.incorrect_answers[1]),
                            HttpUtility.HtmlDecode((string) q.incorrect_answers[2])
                        };
                        var alt = JsonConvert.SerializeObject(alternatives);
                        question = new Question("multiple_choice", text, alt, answer, 'A', qDifficulty, "");
                    } 
                    else
                    {
                        question = new Question("true_false", text, null, answer, answer == "true" ? 'T' : 'F', qDifficulty, null);
                    }
                    
                    apiQuestions.Add(question);
                    await _db.Questions.AddRangeAsync(question);
                    await _db.SaveChangesAsync();
                }

                string randomName = Path.GetRandomFileName().Replace(".", "");
                string category = HttpUtility.HtmlDecode(QuizUtil.CategoryFromIndex(_ran.Next(0, 23)));
                var ranUser = await _db.Users.Skip(_ran.Next(0, _db.Users.Count())).FirstAsync();
                var quiz = new Quiz(ranUser, apiQuestions, category, randomName, "Description", "easy")
                {
                    ImagePath = "/images/logo_transparent_notxt.png", Upvotes = _ran.Next(0, 500), Views = _ran.Next(0, 50000)
                };
                
                await _db.Quizzes.AddRangeAsync(quiz);
                await _db.SaveChangesAsync();
            }
        }

        private static async Task AddRandomGroups(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                var adminUser = await _db.Users.Skip(_ran.Next(0, _db.Users.Count())).FirstOrDefaultAsync();
                var randomName = Path.GetRandomFileName().Replace(".", "");
                var ranBool = _ran.NextDouble() >= 0.5;
                var group = new Group(randomName, adminUser, "Norway", ranBool, false);
                
                var users = await _db.Users.Take(_ran.Next(0, _db.Users.Count())).ToListAsync();
                foreach (var user in users)
                {
                    if (user == adminUser) continue;
                    
                    var groupMember = new GroupMember(_ran.Next(1, 3), user, group);
                    group.Members.Add(groupMember);
                    user.MyGroups.Add(group);
                }
                _db.Groups.Add(group);
                await _db.SaveChangesAsync();
            }
        }

        private static async Task AddRandomUsers(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                string randomUsername = Path.GetRandomFileName().Replace(".", "");
                var user = new ApplicationUser
                {
                    FirstName = "Admin",
                    LastName = "Boss",
                    UserName = randomUsername + "1",
                    Email = randomUsername + "@" + randomUsername + ".no",
                    Level = _ran.Next(1, 20)
                };
                await _um.CreateAsync(user, "Password1.");
                await _db.SaveChangesAsync();
            }
        }
    }
}