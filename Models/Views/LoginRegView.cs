using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BankAccount.Models
{
    public class LoginRegView
    {
        public User newUser {get;set;}
        public Login newLogin {get;set;}
    }
}