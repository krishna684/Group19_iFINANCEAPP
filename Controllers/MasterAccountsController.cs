using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Group19_iFINANCEAPP.Models;

namespace Group19_iFINANCEAPP.Controllers
{
    // Controller to manage user's Master Accounts
    public class MasterAccountsController : Controller
    {
        private Group19_iFINANCEDBEntities db = new Group19_iFINANCEDBEntities();

        // Check if user is logged in
        private bool IsLoggedIn() => Session["UserID"] != null;

        // Check if user is admin
        private bool IsAdmin() => Session["UserRole"]?.ToString() == "Admin";

        // Get current user ID
        private string CurrentUserID() => Session["UserID"].ToString();

        // Generate new master account ID based on user
        private string GenerateNewMasterAccountID(string userID)
        {
            var lastAccount = db.MasterAccount
                                .Where(m => m.UserID == userID && m.ID.StartsWith(userID + "_ACC"))
                                .OrderByDescending(m => m.ID)
                                .FirstOrDefault();

            int nextNumber = 1;

            if (lastAccount != null && lastAccount.ID.Length > userID.Length + 4)
            {
                string numericPart = lastAccount.ID.Substring(userID.Length + 4);
                if (int.TryParse(numericPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{userID}_ACC{nextNumber:D4}";
        }

        // Display all master accounts for logged-in user
        public ActionResult Index()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            string userID = CurrentUserID();
            var masterAccounts = db.MasterAccount
                                   .Include(m => m.AccountGroup)
                                   .Where(m => m.UserID == userID);

            return View(masterAccounts.ToList());
        }

        // Show details of a specific master account
        public ActionResult Details(string id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            if (string.IsNullOrEmpty(id)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var masterAccount = db.MasterAccount.Find(id);
            if (masterAccount == null) return HttpNotFound();
            if (masterAccount.UserID != CurrentUserID()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            return View(masterAccount);
        }

        // Show create new master account page
        public ActionResult Create()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            string userID = CurrentUserID();
            ViewBag.GroupID = new SelectList(
                db.AccountGroup.Where(g => g.UserID == userID),
                "ID", "Name"
            );

            var model = new MasterAccount
            {
                ID = GenerateNewMasterAccountID(userID)
            };

            return View(model);
        }

        // Handle creating a new master account (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Name,OpeningAmount,ClosingAmount,GroupID")] MasterAccount masterAccount)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

            if (ModelState.IsValid)
            {
                masterAccount.UserID = CurrentUserID();
                db.MasterAccount.Add(masterAccount);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // Re-populate dropdown if validation fails
            string userID = CurrentUserID();
            ViewBag.GroupID = new SelectList(
                db.AccountGroup.Where(g => g.UserID == userID),
                "ID", "Name", masterAccount.GroupID
            );

            return View(masterAccount);
        }

        // Show edit master account page
        public ActionResult Edit(string id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            if (string.IsNullOrEmpty(id)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var masterAccount = db.MasterAccount.Find(id);
            if (masterAccount == null) return HttpNotFound();
            if (masterAccount.UserID != CurrentUserID()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            ViewBag.GroupID = new SelectList(
                db.AccountGroup.Where(g => g.UserID == masterAccount.UserID),
                "ID", "Name", masterAccount.GroupID
            );

            return View(masterAccount);
        }

        // Handle editing a master account (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Name,OpeningAmount,ClosingAmount,GroupID")] MasterAccount masterAccount)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

            if (ModelState.IsValid)
            {
                masterAccount.UserID = CurrentUserID();
                db.Entry(masterAccount).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // Re-populate dropdown if validation fails
            string userID = CurrentUserID();
            ViewBag.GroupID = new SelectList(
                db.AccountGroup.Where(g => g.UserID == userID),
                "ID", "Name", masterAccount.GroupID
            );

            return View(masterAccount);
        }

        // Show delete confirmation for master account
        public ActionResult Delete(string id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            if (string.IsNullOrEmpty(id)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var masterAccount = db.MasterAccount.Find(id);
            if (masterAccount == null) return HttpNotFound();
            if (masterAccount.UserID != CurrentUserID()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            return View(masterAccount);
        }

        // Handle deleting a master account (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            var masterAccount = db.MasterAccount.Find(id);

            if (masterAccount.UserID != CurrentUserID())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            // Prevent deletion if linked to transactions
            bool hasTransactions = db.TransactionLine.Any(t => t.ID == id);

            if (hasTransactions)
            {
                TempData["DeleteError"] = "❌ This account is linked to transactions and cannot be deleted.";
                return RedirectToAction("Delete", new { id = id });
            }

            db.MasterAccount.Remove(masterAccount);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // Dispose the database context
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
