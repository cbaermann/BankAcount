using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BankAccount.Models;

namespace BankAccount.Controllers
{
    public class HomeController : Controller
    {
        private MyContext dbContext;
        public HomeController(MyContext context)
        {
            dbContext = context;
        }
        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("register")]
        public IActionResult CreateUser(LoginRegView viewModel)
        {
            Console.WriteLine("############################");

            if(ModelState.IsValid)
            {
                if(dbContext.Users.Any(u=>u.Email == viewModel.newUser.Email))
                {
                    ModelState.AddModelError("Email", "Email already in use!");
                    Console.WriteLine("$$$$$$$$$$$$$$$$$$$$$$$");
                    return View("Index");
                }
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                viewModel.newUser.Password = Hasher.HashPassword(viewModel.newUser, viewModel.newUser.Password);

                dbContext.Users.Add(viewModel.newUser);
                dbContext.SaveChanges();

                HttpContext.Session.SetInt32("InSession", viewModel.newUser.UserId);

                return RedirectToAction("Account");


            }
                else
                {
                    Console.WriteLine("********************");
                    return View("Index");
                }
        }

        [HttpPost("login")]
        public IActionResult LoginUser(LoginRegView viewModel)
        {
             if(ModelState.IsValid)
            {
                var dbUser = dbContext.Users.FirstOrDefault(u=>u.Email == viewModel.newLogin.loginEmail);
                if(dbUser == null)
                {
                    ModelState.AddModelError("Email", "Invalid Email");
                    return View("Index");
                }

                var hasher = new PasswordHasher<Login>();
                var result = hasher.VerifyHashedPassword(viewModel.newLogin, dbUser.Password, viewModel.newLogin.loginPassword);
                if(result == 0)
                {
                    ModelState.AddModelError("Password", "Password does not match email");
                    return View("Index");
                }
                HttpContext.Session.SetInt32("InSession", dbUser.UserId);

                return RedirectToAction("Account");
            }
            else
            {
                return View("Index");
            }
        }

        [HttpGet("account")]
        public IActionResult Account()
        {
            System.Console.WriteLine(HttpContext.Session.GetInt32("InSession"));
            if(HttpContext.Session.GetInt32("InSession")!= null)
            {
                AccountView viewModel = new AccountView();

                viewModel.User = dbContext.Users
                    .Include(u=>u.Transactions)
                    .FirstOrDefault(u=>u.UserId == HttpContext.Session.GetInt32("InSession"));

                    decimal sum = viewModel.User.Transactions.Select(a=>a.Amount).Sum();
                    ViewBag.sum = sum;
                    return View("account", viewModel);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost("createTransaction")]
        public IActionResult NewTransaction(Transaction transaction)
        {
            if(HttpContext.Session.GetString("InSession")!= null)
            {
                decimal balance = dbContext.Users
                    .Include(u=> u.Transactions)
                    .FirstOrDefault(u=>u.UserId==HttpContext.Session.GetInt32("InSession"))
                    .Transactions.Select(u=>u.Amount)
                    .Sum();

                if(balance + transaction.Amount<0)
                {
                    ModelState.AddModelError("Ammount", "Withdrawl Exceeds Available Funds!");
                    return RedirectToAction("Account");
                }
                transaction.CreatedAt = DateTime.Now;
                transaction.UserId = (int)HttpContext.Session.GetInt32("InSession");
                dbContext.Add(transaction);
                dbContext.SaveChanges();
                return RedirectToAction("Account");
            }
            return RedirectToAction("Index");
        }

        [HttpGet("/logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
        
    }
}
