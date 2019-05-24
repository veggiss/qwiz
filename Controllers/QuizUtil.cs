using System.Collections.Generic;
using Newtonsoft.Json;
using Qwiz.Models;

namespace Qwiz.Controllers
{
    public static class QuizUtil
    {
        public static int XpGainedFromQuestion(string type)
        {
            int value = 0;

            if (type == "easy")
                value = 100;
            else if (type == "medium")
                value = 150;
            else if (type == "hard") 
                value = 200;

            return value;
        }

        public static string CategoryFromIndex(int? id)
        {
            if (id == null || id < 0 || id > 23) return null;
            
            string[] category =
            {
                "General Knowledge",      //0
                "Books",                  //1
                "Film",                   //2
                "Music",                  //3
                "Musicals & Theatres",    //4
                "Television",             //5
                "Video Games",            //6
                "Board Games",            //7
                "Science & Nature",       //8
                "Computers",              //9
                "Mathematics",            //10
                "Mythology",              //11
                "Sports",                 //12
                "Geography",              //13
                "History",                //14
                "Politics",               //15
                "Art",                    //16
                "Celebrities",            //17
                "Animals",                //18
                "Vehicles",               //19
                "Comics",                 //20
                "Gadgets",                //21
                "Japanese Anime & Manga", //22
                "Cartoon & Animations"    //23
            };

            return category[(int) id];
        }

        // Checks if the alternatives of type multiple choice actually have 4 items
        public static bool QuestionsValid(List<Question> questions)
        {
            foreach (var question in questions)
            {
                if (question.QuestionType == "multiple_choice")
                {
                    string[] arr = JsonConvert.DeserializeObject<string[]>(question.Alternatives);
                    if (arr.Length != 4) return false;
                }
            }

            return true;
        }
    }
}