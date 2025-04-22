using System;
using System.Linq;
using System.Web.Mvc;
using Group19_iFINANCEAPP.Models;

namespace Group19_iFINANCEAPP.Controllers
{
    public class ChangePasswordController : Controller
    {
        private Group19_iFINANCEDBEntities db = new Group19_iFINANCEDBEntities();

        // GET: ChangePassword
        public ActionResult Index()
        {
            if (Session["UserID"] == null || Session["UserRole"].ToString() != "User")
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);

            return View();
        }

        // POST: ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string oldPassword, string newPassword, string confirmPassword)
        {
            if (Session["UserID"] == null || Session["UserRole"].ToString() != "User")
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);

            string userID = Session["UserID"].ToString();
            var user = db.UserPassword.FirstOrDefault(u => u.ID == userID);

            if (user == null)
            {
                ViewBag.Message = "User not found.";
                return View();
            }

            if (user.EncryptedPassword != oldPassword) // TODO: Replace with Hash check
            {
                ViewBag.Message = "Old password is incorrect.";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Message = "New passwords do not match.";
                return View();
            }

            user.EncryptedPassword = newPassword; // TODO: Hash it
            db.SaveChanges();
            ViewBag.Message = "Password updated successfully.";
            return View();
        }
    }
}
