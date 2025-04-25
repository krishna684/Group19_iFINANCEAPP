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
    // Controller to manage Account Groups (Create, Edit, Delete, View for non-admin users)
    public class AccountGroupsController : Controller
    {
        private Group19_iFINANCEDBEntities db = new Group19_iFINANCEDBEntities();

        // Check if the current user is an Admin
        private bool IsAdmin()
        {
            return Session["UserRole"]?.ToString() == "Admin";
        }

        // Check if the user is logged in
        private bool IsLoggedIn()
        {
            return Session["UserID"] != null;
        }

        // Check if the user is authorized to access the given group
        private bool IsAuthorized(AccountGroup group)
        {
            string userId = Session["UserID"]?.ToString();
            return group != null && group.UserID == userId;
        }

        // Display all account groups for the logged-in non-admin user
        public ActionResult Index()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            string userId = Session["UserID"].ToString();
            var groups = db.AccountGroup.Include(a => a.AccountGroup2)
                                        .Where(g => g.UserID == userId);

            return View(groups.ToList());
        }

        // Display details of a specific account group
        public ActionResult Details(string id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (string.IsNullOrEmpty(id)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var accountGroup = db.AccountGroup.Find(id);
            if (accountGroup == null || !IsAuthorized(accountGroup))
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            return View(accountGroup);
        }

        // Show the create account group form
        public ActionResult Create()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            string userId = Session["UserID"].ToString();
            ViewBag.ParentGroupID = new SelectList(
                db.AccountGroup.Where(g => g.UserID == userId), "ID", "Name"
            );

            string newID = GenerateUserScopedGroupID(userId);
            var model = new AccountGroup { ID = newID };

            return View(model);
        }

        // Generate a new group ID scoped to the current user
        private string GenerateUserScopedGroupID(string userId)
        {
            var lastGroup = db.AccountGroup
                              .Where(g => g.UserID == userId && g.ID.StartsWith(userId + "_GRP"))
                              .OrderByDescending(g => g.ID)
                              .FirstOrDefault();

            int nextNumber = 1;

            if (lastGroup != null && lastGroup.ID.Length > userId.Length + 4)
            {
                string numericPart = lastGroup.ID.Substring(userId.Length + 4);
                if (int.TryParse(numericPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{userId}_GRP{nextNumber:D4}";
        }

        // Handle account group creation (POST)
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

            string userId = Session["UserID"].ToString();
            ViewBag.ParentGroupID = new SelectList(
                db.AccountGroup.Where(g => g.UserID == userId), "ID", "Name", accountGroup.ParentGroupID
            );
            return View(accountGroup);
        }

        // Show the edit account group form
        public ActionResult Edit(string id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (string.IsNullOrEmpty(id)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var accountGroup = db.AccountGroup.Find(id);
            if (accountGroup == null || !IsAuthorized(accountGroup))
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            string userId = Session["UserID"].ToString();
            ViewBag.ParentGroupID = new SelectList(
                db.AccountGroup.Where(g => g.UserID == userId), "ID", "Name", accountGroup.ParentGroupID
            );
            return View(accountGroup);
        }

        // Handle account group editing (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Name,ParentGroupID,ElementType")] AccountGroup accountGroup)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (ModelState.IsValid)
            {
                accountGroup.UserID = Session["UserID"].ToString();
                db.Entry(accountGroup).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            string userId = Session["UserID"].ToString();
            ViewBag.ParentGroupID = new SelectList(
                db.AccountGroup.Where(g => g.UserID == userId), "ID", "Name", accountGroup.ParentGroupID
            );
            return View(accountGroup);
        }

        // Show the delete confirmation page
        public ActionResult Delete(string id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (string.IsNullOrEmpty(id)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var accountGroup = db.AccountGroup.Find(id);
            if (accountGroup == null || !IsAuthorized(accountGroup))
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            return View(accountGroup);
        }

        // Handle account group deletion (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            var accountGroup = db.AccountGroup.Find(id);

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

        // Dispose database context
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
