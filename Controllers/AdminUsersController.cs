using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity;
using Group19_iFINANCEAPP.Models;

namespace Group19_iFINANCEAPP.Controllers
{
    // Controller to manage Users (Admin view only)
    public class AdminUsersController : Controller
    {
        private Group19_iFINANCEDBEntities db = new Group19_iFINANCEDBEntities();

        // Check if the current user is an Admin
        private bool IsAdmin()
        {
            return Session["UserRole"]?.ToString() == "Admin";
        }

        // Display the list of all users with optional search filter
        public ActionResult Index(string searchString)
        {
            if (Session["UserID"] == null || !IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.CurrentFilter = searchString;

            var users = db.UserPassword.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(u => u.UserName.Contains(searchString));
            }

            return View(users.ToList());
        }

        // AJAX: Search users dynamically
        public ActionResult Search(string searchString)
        {
            if (Session["UserID"] == null || !IsAdmin())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            var users = db.UserPassword.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(u => u.UserName.Contains(searchString));
            }

            return PartialView("_UserTable", users.ToList());
        }

        // Display user details
        public ActionResult Details(string id)
        {
            if (!IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            if (string.IsNullOrEmpty(id)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var user = db.UserPassword
                         .Include(u => u.Administrator)
                         .Include(u => u.NonAdminUser)
                         .FirstOrDefault(u => u.ID == id);

            if (user == null) return HttpNotFound();

            return View(user);
        }

        // Show the form to edit a user
        public ActionResult Edit(string id)
        {
            if (!IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            if (string.IsNullOrEmpty(id)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var user = db.UserPassword
                         .Include(u => u.Administrator)
                         .Include(u => u.NonAdminUser)
                         .FirstOrDefault(u => u.ID == id);

            if (user == null) return HttpNotFound();

            return View(user);
        }

        // Handle editing of a user (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,UserName,EncryptedPassword,PasswordExpiryTime,UserAccountExpiryDate")] UserPassword user)
        {
            if (!IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (ModelState.IsValid)
            {
                var existing = db.UserPassword.Find(user.ID);
                if (existing == null)
                {
                    ModelState.AddModelError("", "User not found.");
                    return View(user);
                }

                existing.UserName = user.UserName;
                existing.EncryptedPassword = user.EncryptedPassword;
                existing.PasswordExpiryTime = user.PasswordExpiryTime;
                existing.UserAccountExpiryDate = user.UserAccountExpiryDate;

                db.SaveChanges();
                TempData["Saved"] = "User updated successfully.";
                return RedirectToAction("Index");
            }

            TempData["Saved"] = "Edit failed — invalid data.";
            return View(user);
        }

        // Show the delete confirmation page for a user
        public ActionResult Delete(string id)
        {
            if (!IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            if (string.IsNullOrEmpty(id)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var user = db.UserPassword.Find(id);
            if (user == null) return HttpNotFound();

            return View(user);
        }

        // Handle deletion of a user (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            if (!IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            var user = db.UserPassword.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var admin = db.Administrator.Find(id);
            if (admin != null)
            {
                db.Administrator.Remove(admin);
            }

            var nonAdmin = db.NonAdminUser.Find(id);
            if (nonAdmin != null)
            {
                db.NonAdminUser.Remove(nonAdmin);
            }

            db.UserPassword.Remove(user);
            db.SaveChanges();

            TempData["Saved"] = "User deleted.";
            return RedirectToAction("Index");
        }
    }
}
