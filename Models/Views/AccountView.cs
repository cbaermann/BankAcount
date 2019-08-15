using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BankAccount.Models
{
    public class AccountView
    {
        public Transaction Transaction{get;set;}

        public User User{get;set;}
    }
}