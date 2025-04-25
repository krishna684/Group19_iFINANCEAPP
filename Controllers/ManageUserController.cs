using System;
using System.Linq;
using System.Web.Mvc;
using Group19_iFINANCEAPP.Models;

namespace Group19_iFINANCEAPP.Controllers
{
    // Controller for admin to register new users
    public class ManageUserController : Controller
    {
        private Group19_iFINANCEDBEntities db = new Group19_iFINANCEDBEntities();

        // Show the register new user page
        public ActionResult Register()
        {
            if (Session["UserRole"]?.ToString() != "Admin")
            {
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }

        // Handle registration of a new user (POST)
        [HttpPost]
        public ActionResult Register(
            string ID,
            string UserName,
            string Password,
            string Role,
            string Name,
            string Address,
            string Email,
            string SecurityQuestion,
            string SecurityAnswer,
            DateTime? DateHired,
            DateTime? DateFinished)
        {
            if (Session["UserRole"]?.ToString() != "Admin")
            {
                return RedirectToAction("Login", "Auth");
            }

            if (db.UserPassword.Any(u => u.ID == ID || u.UserName == UserName))
            {
                ViewBag.Message = "User ID or Username already exists.";
                return View();
            }

            // Add user credentials to UserPassword table
            db.UserPassword.Add(new UserPassword
            {
                ID = ID,
                UserName = UserName,
                EncryptedPassword = Password, // Note: Should hash password later
                PasswordExpiryTime = 90,
                UserAccountExpiryDate = new DateTime(2026, 1, 1),
                SecurityQuestion = SecurityQuestion,
                SecurityAnswer = SecurityAnswer
            });

            // Assign role-specific data
            if (Role == "Admin")
            {
                db.Administrator.Add(new Administrator
                {
                    ID = ID,
                    Name = Name,
                    DateHired = DateHired ?? DateTime.Now,
                    DateFinished = DateFinished
                });
            }
            else
            {
                db.NonAdminUser.Add(new NonAdminUser
                {
                    ID = ID,
                    Name = Name,
                    Address = Address,
                    Email = Email
                });
            }

            db.SaveChanges();

            ViewBag.Message = "User created successfully.";
            return View();
        }
    }
}
