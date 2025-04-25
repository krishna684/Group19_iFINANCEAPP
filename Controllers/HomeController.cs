using System.Web.Mvc;

namespace Group19_iFINANCEAPP.Controllers
{
    // Controller for handling home page redirects based on user roles
    public class HomeController : Controller
    {
        // Main page – redirect to appropriate home based on user role
        public ActionResult Index()
        {
            if (Session["UserID"] == null || Session["UserRole"] == null)
            {
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

            return RedirectToAction("Login", "Auth");
        }

        // Show About page
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        // Show Contact page
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
    }
}
