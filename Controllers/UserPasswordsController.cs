using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Group19_iFINANCEAPP.Models;

namespace Group19_iFINANCEAPP.Controllers
{
    public class UserPasswordsController : Controller
    {
        private Group19_iFINANCEDBEntities db = new Group19_iFINANCEDBEntities();

        // Admin-only guard
        private bool IsAdmin()
        {
            return Session["UserRole"] != null && Session["UserRole"].ToString() == "Admin";
        }

        // GET: UserPasswords
        public ActionResult Index()
        {
            if (!IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            var userPassword = db.UserPassword.Include(u => u.Administrator).Include(u => u.NonAdminUser);
            return View(userPassword.ToList());
        }

        // GET: UserPasswords/Details/5
        public ActionResult Details(string id)
        {
            if (!IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            UserPassword userPassword = db.UserPassword.Find(id);
            if (userPassword == null)
                return HttpNotFound();

            return View(userPassword);
        }

        // GET: UserPasswords/Create
        public ActionResult Create()
        {
            if (!IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            ViewBag.AdminUsers = new SelectList(db.Administrator, "ID", "Name");
            ViewBag.NonAdminUsers = new SelectList(db.NonAdminUser, "ID", "Name");
            return View();
        }

        // POST: UserPasswords/Create
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

        // GET: UserPasswords/Edit/5
        public ActionResult Edit(string id)
        {
            if (!IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            UserPassword userPassword = db.UserPassword.Find(id);
            if (userPassword == null)
                return HttpNotFound();

            ViewBag.AdminUsers = new SelectList(db.Administrator, "ID", "Name", userPassword.ID);
            ViewBag.NonAdminUsers = new SelectList(db.NonAdminUser, "ID", "Name", userPassword.ID);
            return View(userPassword);
        }

        // POST: UserPasswords/Edit/5
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

        // GET: UserPasswords/Delete/5
        public ActionResult Delete(string id)
        {
            if (!IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            UserPassword userPassword = db.UserPassword.Find(id);
            if (userPassword == null)
                return HttpNotFound();

            return View(userPassword);
        }

        // POST: UserPasswords/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            if (!IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            UserPassword userPassword = db.UserPassword.Find(id);
            db.UserPassword.Remove(userPassword);
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
