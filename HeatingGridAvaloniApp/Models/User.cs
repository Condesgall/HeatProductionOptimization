using System;
using System.Collections;
using System.Collections.Generic;

namespace HeatingGridAvaloniaApp.Models
{
    public class User
    {
        private string userName;
        private string password;
        private Guid ID;
        private string email;

        public User(string userName_, Guid ID_, string password_, string email_)
        {
            this.userName = userName_;
            this.ID = ID_;
            this.password = password_;
            this.email = email_;
        }

        public string UserName
        {
            get { return userName; }
            set { userName = value; }
        }

        public string Password
        {
            get { return password; }
            set { password = value; }
        }
    }

    public class UserManager
    {
        private static Dictionary<string, string> users = new Dictionary<string, string>();

        public UserManager()
        {
            users = new Dictionary<string, string>
            {
                { "admin", "password" },
            };
        }
        
        public static bool Login(string userName, string password)
        {
            if (users.ContainsKey(userName))
            {
                if (password != users[userName])
                {
                    return false;
                }
                else
                {
                    return true;
                };
            }
            else
            {
                return false;
            }
        }

        public static void SignUp(string userName, string password)
        {
            if (!users.ContainsKey(userName))
            {
                users.Add(userName, password);
            }
        }
    }
}