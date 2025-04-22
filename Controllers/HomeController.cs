using System.Web.Mvc;

namespace Group19_iFINANCEAPP.Controllers
{
    public class HomeController : Controller
    {
        // Main entry point – redirect based on role or to login if not authenticated
        public ActionResult Index()
        {
            if (Session["UserID"] == null || Session["UserRole"] == null)
            {
                // User is not logged in → redirect to Login page
                return RedirectToAction("Login", "Auth");
            }

            if (Session["UserRole"].ToString() == "Admin")
            {
                return View("AdminHome");
            }

            if (Session["UserRole"].ToString() == "User")
            {
                return View("UserHome");
            }

            // Default fallback
            return RedirectToAction("Login", "Auth");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
    }
}
