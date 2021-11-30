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
        public string token= "";
        public Token() { }
        public Token(string token) 
        {
            this.token = token;
        }


    }
}
