using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Group19_iFINANCEAPP.Models;

namespace Group19_iFINANCEAPP.Controllers
{
    public class AccountGroupsController : Controller
    {
        private Group19_iFINANCEDBEntities db = new Group19_iFINANCEDBEntities();

        private bool IsAdmin()
        {
            return Session["UserRole"]?.ToString() == "Admin";
        }

        private bool IsLoggedIn()
        {
            return Session["UserID"] != null;
        }

        private bool IsAuthorized(AccountGroup group)
        {
            string userId = Session["UserID"]?.ToString();
            return group != null && group.UserID == userId;
        }

        // GET: AccountGroups
        public ActionResult Index()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            string userID = Session["UserID"].ToString();
            var groups = db.AccountGroup.Include(a => a.AccountGroup2)
                                        .Where(g => g.UserID == userID);

            return View(groups.ToList());
        }

        // GET: AccountGroups/Details/5
        public ActionResult Details(string id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            AccountGroup accountGroup = db.AccountGroup.Find(id);
            if (accountGroup == null || !IsAuthorized(accountGroup)) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            return View(accountGroup);
        }

        // GET: AccountGroups/Create
        public ActionResult Create()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            string userID = Session["UserID"].ToString();
            ViewBag.ParentGroupID = new SelectList(
                db.AccountGroup.Where(g => g.UserID == userID), "ID", "Name"
            );

            // 🔑 Auto-generate new group ID
            string newID = GenerateUserScopedGroupID(userID);
            var model = new AccountGroup { ID = newID };

            return View(model);
        }

        private string GenerateUserScopedGroupID(string userID)
        {
            // Get latest group ID for this user
            var lastGroup = db.AccountGroup
                              .Where(g => g.UserID == userID && g.ID.StartsWith(userID + "_GRP"))
                              .OrderByDescending(g => g.ID)
                              .FirstOrDefault();

            int nextNumber = 1;

            if (lastGroup != null && lastGroup.ID.Length > userID.Length + 4)
            {
                string numericPart = lastGroup.ID.Substring(userID.Length + 4); // After 'USR123_GRP'
                if (int.TryParse(numericPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{userID}_GRP{nextNumber:D4}";  // e.g., USR123_GRP0001
        }


        // POST: AccountGroups/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Name,ParentGroupID,ElementType")] AccountGroup accountGroup)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (ModelState.IsValid)
            {
                accountGroup.UserID = Session["UserID"].ToString();
                db.AccountGroup.Add(accountGroup);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            string userID = Session["UserID"].ToString();
            ViewBag.ParentGroupID = new SelectList(
                db.AccountGroup.Where(g => g.UserID == userID), "ID", "Name", accountGroup.ParentGroupID
            );
            return View(accountGroup);
        }

        // GET: AccountGroups/Edit/5
        public ActionResult Edit(string id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            AccountGroup accountGroup = db.AccountGroup.Find(id);
            if (accountGroup == null || !IsAuthorized(accountGroup)) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            string userID = Session["UserID"].ToString();
            ViewBag.ParentGroupID = new SelectList(
                db.AccountGroup.Where(g => g.UserID == userID), "ID", "Name", accountGroup.ParentGroupID
            );
            return View(accountGroup);
        }

        // POST: AccountGroups/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Name,ParentGroupID,ElementType")] AccountGroup accountGroup)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (ModelState.IsValid)
            {
                accountGroup.UserID = Session["UserID"].ToString(); // Maintain ownership
                db.Entry(accountGroup).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            string userID = Session["UserID"].ToString();
            ViewBag.ParentGroupID = new SelectList(
                db.AccountGroup.Where(g => g.UserID == userID), "ID", "Name", accountGroup.ParentGroupID
            );
            return View(accountGroup);
        }

        // GET: AccountGroups/Delete/5
        public ActionResult Delete(string id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            AccountGroup accountGroup = db.AccountGroup.Find(id);
            if (accountGroup == null || !IsAuthorized(accountGroup)) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            return View(accountGroup);
        }

        // POST: AccountGroups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            var accountGroup = db.AccountGroup.Find(id);

            // 🔒 Check for sub-groups
            if (db.AccountGroup.Any(g => g.ParentGroupID == id))
            {
                TempData["DeleteError"] = "⚠️ Cannot delete this group because it has linked sub-groups. Please delete or reassign those first.";
                return RedirectToAction("Delete", new { id = id });
            }

            if (accountGroup == null || !IsAuthorized(accountGroup))
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            db.AccountGroup.Remove(accountGroup);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

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
