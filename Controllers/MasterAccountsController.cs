using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Group19_iFINANCEAPP.Models;

namespace Group19_iFINANCEAPP.Controllers
{
    public class MasterAccountsController : Controller
    {
        private Group19_iFINANCEDBEntities db = new Group19_iFINANCEDBEntities();

        private bool IsLoggedIn() => Session["UserID"] != null;
        private bool IsAdmin() => Session["UserRole"]?.ToString() == "Admin";
        private string CurrentUserID() => Session["UserID"].ToString();

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

        // GET: MasterAccounts
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

        // GET: MasterAccounts/Details/5
        public ActionResult Details(string id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var masterAccount = db.MasterAccount.Find(id);
            if (masterAccount == null) return HttpNotFound();
            if (masterAccount.UserID != CurrentUserID()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            return View(masterAccount);
        }

        // GET: MasterAccounts/Create
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

        // POST: MasterAccounts/Create
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

            // 🛠 Re-populate dropdown if validation fails
            string userID = CurrentUserID();
            ViewBag.GroupID = new SelectList(
                db.AccountGroup.Where(g => g.UserID == userID),
                "ID", "Name", masterAccount.GroupID
            );

            return View(masterAccount);
        }

        // GET: MasterAccounts/Edit/5
        public ActionResult Edit(string id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var masterAccount = db.MasterAccount.Find(id);
            if (masterAccount == null) return HttpNotFound();
            if (masterAccount.UserID != CurrentUserID()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            ViewBag.GroupID = new SelectList(
                db.AccountGroup.Where(g => g.UserID == masterAccount.UserID),
                "ID", "Name", masterAccount.GroupID
            );

            return View(masterAccount);
        }

        // POST: MasterAccounts/Edit/5
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

            // 🛠 Re-populate dropdown if validation fails
            string userID = CurrentUserID();
            ViewBag.GroupID = new SelectList(
                db.AccountGroup.Where(g => g.UserID == userID),
                "ID", "Name", masterAccount.GroupID
            );

            return View(masterAccount);
        }

        // GET: MasterAccounts/Delete/5
        public ActionResult Delete(string id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var masterAccount = db.MasterAccount.Find(id);
            if (masterAccount == null) return HttpNotFound();
            if (masterAccount.UserID != CurrentUserID()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            return View(masterAccount);
        }

        // POST: MasterAccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            var masterAccount = db.MasterAccount.Find(id);
            if (masterAccount.UserID != CurrentUserID()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            // 🛡 Prevent delete if linked to transactions
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

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
