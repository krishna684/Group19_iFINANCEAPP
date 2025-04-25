using System;
using System.Linq;
using System.Web.Mvc;
using Group19_iFINANCEAPP.Models;

namespace Group19_iFINANCEAPP.Controllers
{
    public class ManageUserController : Controller
    {
        private Group19_iFINANCEDBEntities db = new Group19_iFINANCEDBEntities();

        // GET: ManageUser/Register
        public ActionResult Register()
        {
            if (Session["UserRole"]?.ToString() != "Admin")
                return RedirectToAction("Login", "Auth");

            return View();
        }

        // POST: ManageUser/Register
        [HttpPost]
        public ActionResult Register(string ID, string UserName, string Password, string Role,
                                     string Name, string Address, string Email,
                                     string SecurityQuestion, string SecurityAnswer,
                                     DateTime? DateHired, DateTime? DateFinished)
        {
            if (Session["UserRole"]?.ToString() != "Admin")
                return RedirectToAction("Login", "Auth");

            if (db.UserPassword.Any(u => u.ID == ID || u.UserName == UserName))
            {
                ViewBag.Message = "User ID or Username already exists.";
                return View();
            }

            // 1. Add to UserPassword
            db.UserPassword.Add(new UserPassword
            {
                ID = ID,
                UserName = UserName,
                EncryptedPassword = Password, // TODO: Add hash later
                PasswordExpiryTime = 90,
                UserAccountExpiryDate = new DateTime(2026, 1, 1),
                SecurityQuestion = SecurityQuestion,
                SecurityAnswer = SecurityAnswer
            });

            // 2. Add to either Admin or NonAdmin
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
