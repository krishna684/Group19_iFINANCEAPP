using System;
using System.Linq;
using System.Web.Mvc;
using Group19_iFINANCEAPP.Models;

namespace Group19_iFINANCEAPP.Controllers
{
    // Controller to manage Authentication (Login, Logout, Forgot/Reset Password)
    public class AuthController : Controller
    {
        private Group19_iFINANCEDBEntities db = new Group19_iFINANCEDBEntities();

        // Show the login page
        public ActionResult Login()
        {
            return View();
        }

        // Handle user login (POST)
        [HttpPost]
        public ActionResult Login(string UserName, string Password)
        {
            var user = db.UserPassword.FirstOrDefault(u => u.UserName == UserName);

            if (user != null && user.EncryptedPassword == Password)
            {
                Session["UserID"] = user.ID;
                Session["UserName"] = user.UserName;
                Session["UserRole"] = db.Administrator.Any(a => a.ID == user.ID) ? "Admin" : "User";

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Message = "Invalid username or password.";
            return View();
        }

        // Log out the current user
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        // Show forgot password page
        public ActionResult ForgotPassword()
        {
            return View();
        }

        // Handle forgot password logic (POST)
        [HttpPost]
        public ActionResult ForgotPassword(string UserName, string Answer)
        {
            var user = db.UserPassword.FirstOrDefault(u => u.UserName == UserName);

            if (user == null)
            {
                ViewBag.Message = "User not found.";
                return View();
            }

            if (string.IsNullOrEmpty(Answer))
            {
                // Show security question if no answer yet
                ViewBag.SecurityQuestion = user.SecurityQuestion;
                ViewBag.UserName = user.UserName;
                return View();
            }

            if (!string.Equals(user.SecurityAnswer, Answer, StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.Message = "Incorrect answer.";
                return View();
            }

            // Correct answer, redirect to reset password
            return RedirectToAction("ResetPassword", new { id = user.ID });
        }

        // Show reset password page
        public ActionResult ResetPassword(string id)
        {
            ViewBag.UserID = id;
            return View();
        }

        // Handle resetting password (POST)
        [HttpPost]
        public ActionResult ResetPassword(string id, string NewPassword, string ConfirmPassword)
        {
            if (NewPassword != ConfirmPassword)
            {
                ViewBag.Message = "Passwords do not match.";
                ViewBag.UserID = id;
                return View();
            }

            var user = db.UserPassword.Find(id);

            if (user != null)
            {
                user.EncryptedPassword = NewPassword;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Password successfully updated. You can now log in.";
            }

            return RedirectToAction("Login");
        }
    }
}
