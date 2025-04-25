using System;
using System.Linq;
using System.Web.Mvc;
using Group19_iFINANCEAPP.Models;

namespace Group19_iFINANCEAPP.Controllers
{
    // Controller to allow non-admin users to change their password
    public class ChangePasswordController : Controller
    {
        private Group19_iFINANCEDBEntities db = new Group19_iFINANCEDBEntities();

        // Show the change password page
        public ActionResult Index()
        {
            if (Session["UserID"] == null || Session["UserRole"]?.ToString() != "User")
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
            }

            return View();
        }

        // Handle password change logic (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string oldPassword, string newPassword, string confirmPassword)
        {
            if (Session["UserID"] == null || Session["UserRole"]?.ToString() != "User")
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
            }

            string userId = Session["UserID"].ToString();
            var user = db.UserPassword.FirstOrDefault(u => u.ID == userId);

            if (user == null)
            {
                ViewBag.Message = "User not found.";
                return View();
            }

            // Check if the old password matches
            if (user.EncryptedPassword != oldPassword) 
            {
                ViewBag.Message = "Old password is incorrect.";
                return View();
            }

            // Check if new password and confirm password match
            if (newPassword != confirmPassword)
            {
                ViewBag.Message = "New passwords do not match.";
                return View();
            }

            // Update password
            user.EncryptedPassword = newPassword;
            db.SaveChanges();

            ViewBag.Message = "Password updated successfully.";
            return View();
        }
    }
}
