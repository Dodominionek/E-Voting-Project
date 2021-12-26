using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;

namespace HttpClient
{
    public class User
    {

        public string username = "";
        public string password = "";
        public User(string login, string password)
        {
            this.username = login;
            this.password = password;
        }
        public User() { }
    }
    public class Token
    {
        public string token = "";
        public Token() { }
        public Token(string token)
        {
            this.token = token;
        }


    }
    public class Voting
    {
        /*
         *  "answerA": "Gitówa",
         "answerB": "Kozówa",
         "answerC": "Trzecia opcja",
         "answerD": "",
         "id": 1,
         "question": "LOL2",
         "status": "Created",
         "timeEnd": "2021-12-10 20:00:56.550604",
         "timeStart": "2021-12-10 19:58:56.550604"
         */

        public string answerA, answerB, answerC, answerD;
        public int id;
        public string question;
        public string status;
        public Voting()
        {
            answerA = "a";
            answerB = "b";
            answerC = "c";
            answerD = "d";
            id = 0;
            question = "question";
            status = "";
        }
        public Voting(string A, string B, string C, string D, int id, string Question, string Status)
        {
            answerA = A;
            answerB = B;
            answerC = C;
            answerD = D;
            this.id = id;
            question = Question;
            status = Status;
        }

    }
    public class Vote
    {
        public int votingId;
        public string userAnswer;

        public Vote() { }
        public Vote(int id, string answer)
        {
            this.votingId = id;
            this.userAnswer = answer;
        }

    }
}

