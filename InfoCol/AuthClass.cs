using System;
using System.IO;
using System.Collections.Generic;

namespace InfoCol
{

    public class AuthClass
    {
        class ValidUser
        {
            public String username;
            public String password;
        }

        public const String auth_dat = "auth.txt";
        private List<ValidUser> userList = new List<ValidUser>();

        public void load()
        {
            StreamReader reader = new StreamReader(auth_dat);
            userList.Clear();
            while (!reader.EndOfStream)
            {
                ValidUser validUser = new ValidUser();
                validUser.username = reader.ReadLine();
                validUser.password = reader.ReadLine();
                userList.Add(validUser);
            }
        }

        private void initialize()
        {
            userList.Clear();
            ValidUser validUser = new ValidUser();
            validUser.username = "admin";
            validUser.password = "admin";
            userList.Add(validUser);
        }

        public AuthClass()
        {
            if (File.Exists(auth_dat))
                load();
            else
                initialize();
        }

        public Boolean verify(String username, String password)
        {
            foreach(ValidUser user in userList)
            {
                if (user.username.Equals(username) && user.password.Equals(password))
                    return true;
            }
            return false;
        }
    }
}