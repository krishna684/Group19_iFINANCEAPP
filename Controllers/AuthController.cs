using System;
using System.Linq;
using System.Web.Mvc;
using Group19_iFINANCEAPP.Models;

namespace Group19_iFINANCEAPP.Controllers
{
    public class AuthController : Controller
    {
        private Group19_iFINANCEDBEntities db = new Group19_iFINANCEDBEntities();

        // GET: Auth/Login
        public ActionResult Login()
        {
            return View();
        }

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

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        // GET: Auth/ForgotPassword
        public ActionResult ForgotPassword()
        {
            return View();
        }

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
                // Show security question
                ViewBag.SecurityQuestion = user.SecurityQuestion;
                ViewBag.UserName = user.UserName;
                return View();
            }

            if (!string.Equals(user.SecurityAnswer, Answer, StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.Message = "Incorrect answer.";
                return View();
            }

            // Answer is correct → Redirect to reset
            return RedirectToAction("ResetPassword", new { id = user.ID });
        }

        public ActionResult ResetPassword(string id)
        {
            ViewBag.UserID = id;
            return View();
        }

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
