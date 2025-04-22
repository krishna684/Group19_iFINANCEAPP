using System.Linq;
using System.Web.Mvc;
using Group19_iFINANCEAPP.Models; // Use the namespace where your EF model is

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

        // POST: Auth/Login
        [HttpPost]
        public ActionResult Login(string UserName, string Password)
        {
            var user = db.UserPassword.FirstOrDefault(u => u.UserName == UserName);

            if (user != null && user.EncryptedPassword == Password) // Later, use hash check
            {
                Session["UserID"] = user.ID;
                Session["UserName"] = user.UserName;

                bool isAdmin = db.Administrator.Any(a => a.ID == user.ID);
                Session["UserRole"] = isAdmin ? "Admin" : "User";

                return RedirectToAction("Index", "Home"); // or any dashboard page
            }

            ViewBag.Message = "Invalid username or password.";
            return View();
        }

        // GET: Auth/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login", "Auth");
        }
    }
}
