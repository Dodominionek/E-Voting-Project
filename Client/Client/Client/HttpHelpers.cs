using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;

namespace SpecialVoting
{
    public class Voting
    {
        public string answerA, answerB, answerC, answerD;
        public string timeEnd, timeStart;
        public string question;
        public Voting()
        {
            answerA = "a";
            answerB = "b";
            answerC = "c";
            answerD = "d";
            question = "question";
        }
        public Voting(string A, string B, string C, string D, string Question,string start,string end)
        {
            answerA = A;
            answerB = B;
            answerC = C;
            answerD = D;
            question = Question;
            timeStart = start;
            timeEnd = end;
        }
    }
}
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
    public class Result
    {
        public int votingId, answerA, answerB, answerC, answerD;
        public Result()
        {
            answerA = 0;
            answerB = 0;
            answerC = 0;
            answerD = 0;
            votingId = 0;

        }
        public Result( int id, int a, int b, int c, int d)
        {
            votingId = id;
            answerA = a;
            answerB = b;
            answerC = c;
            answerD = d;
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

