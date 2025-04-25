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
    // Controller to manage User Passwords (Admin Only)
    public class UserPasswordsController : Controller
    {
        private Group19_iFINANCEDBEntities db = new Group19_iFINANCEDBEntities();

        // Check if current user is admin
        private bool IsAdmin()
        {
            return Session["UserRole"] != null && Session["UserRole"].ToString() == "Admin";
        }

        // Display all user passwords
        public ActionResult Index()
        {
            if (!IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            var userPasswords = db.UserPassword.Include(u => u.Administrator).Include(u => u.NonAdminUser);
            return View(userPasswords.ToList());
        }

        // Show details of a specific user password
        public ActionResult Details(string id)
        {
            if (!IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            if (string.IsNullOrEmpty(id)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var userPassword = db.UserPassword.Find(id);

            if (userPassword == null)
            {
                return HttpNotFound();
            }

            return View(userPassword);
        }

        // Show create new user password page
        public ActionResult Create()
        {
            if (!IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            ViewBag.AdminUsers = new SelectList(db.Administrator, "ID", "Name");
            ViewBag.NonAdminUsers = new SelectList(db.NonAdminUser, "ID", "Name");
            return View();
        }

        // Handle creating a new user password (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,UserName,EncryptedPassword,PasswordExpiryTime,UserAccountExpiryDate")] UserPassword userPassword)
        {
            if (!IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (ModelState.IsValid)
            {
                db.UserPassword.Add(userPassword);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AdminUsers = new SelectList(db.Administrator, "ID", "Name", userPassword.ID);
            ViewBag.NonAdminUsers = new SelectList(db.NonAdminUser, "ID", "Name", userPassword.ID);
            return View(userPassword);
        }

        // Show edit user password page
        public ActionResult Edit(string id)
        {
            if (!IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            if (string.IsNullOrEmpty(id)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var userPassword = db.UserPassword.Find(id);

            if (userPassword == null)
            {
                return HttpNotFound();
            }

            ViewBag.AdminUsers = new SelectList(db.Administrator, "ID", "Name", userPassword.ID);
            ViewBag.NonAdminUsers = new SelectList(db.NonAdminUser, "ID", "Name", userPassword.ID);
            return View(userPassword);
        }

        // Handle editing a user password (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,UserName,EncryptedPassword,PasswordExpiryTime,UserAccountExpiryDate")] UserPassword userPassword)
        {
            if (!IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (ModelState.IsValid)
            {
                db.Entry(userPassword).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AdminUsers = new SelectList(db.Administrator, "ID", "Name", userPassword.ID);
            ViewBag.NonAdminUsers = new SelectList(db.NonAdminUser, "ID", "Name", userPassword.ID);
            return View(userPassword);
        }

        // Show delete confirmation page
        public ActionResult Delete(string id)
        {
            if (!IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            if (string.IsNullOrEmpty(id)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var userPassword = db.UserPassword.Find(id);

            if (userPassword == null)
            {
                return HttpNotFound();
            }

            return View(userPassword);
        }

        // Handle deleting a user password (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            if (!IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            var userPassword = db.UserPassword.Find(id);

            if (userPassword != null)
            {
                db.UserPassword.Remove(userPassword);
                db.SaveChanges();
            }

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
