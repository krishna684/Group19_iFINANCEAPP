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
    // Controller to manage Non-Admin Users
    public class NonAdminUsersController : Controller
    {
        private Group19_iFINANCEDBEntities db = new Group19_iFINANCEDBEntities();

        // Display all non-admin users
        public ActionResult Index()
        {
            var nonAdminUsers = db.NonAdminUser.Include(n => n.UserPassword);
            return View(nonAdminUsers.ToList());
        }

        // Show details of a specific non-admin user
        public ActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var nonAdminUser = db.NonAdminUser.Find(id);

            if (nonAdminUser == null)
            {
                return HttpNotFound();
            }

            return View(nonAdminUser);
        }

        // Show create new non-admin user page
        public ActionResult Create()
        {
            ViewBag.ID = new SelectList(db.UserPassword, "ID", "UserName");
            return View();
        }

        // Handle creation of a new non-admin user (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Name,Address,Email")] NonAdminUser nonAdminUser)
        {
            if (ModelState.IsValid)
            {
                db.NonAdminUser.Add(nonAdminUser);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ID = new SelectList(db.UserPassword, "ID", "UserName", nonAdminUser.ID);
            return View(nonAdminUser);
        }

        // Show edit non-admin user page
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var nonAdminUser = db.NonAdminUser.Find(id);

            if (nonAdminUser == null)
            {
                return HttpNotFound();
            }

            ViewBag.ID = new SelectList(db.UserPassword, "ID", "UserName", nonAdminUser.ID);
            return View(nonAdminUser);
        }

        // Handle editing of a non-admin user (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Name,Address,Email")] NonAdminUser nonAdminUser)
        {
            if (ModelState.IsValid)
            {
                db.Entry(nonAdminUser).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ID = new SelectList(db.UserPassword, "ID", "UserName", nonAdminUser.ID);
            return View(nonAdminUser);
        }

        // Show delete confirmation for a non-admin user
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var nonAdminUser = db.NonAdminUser.Find(id);

            if (nonAdminUser == null)
            {
                return HttpNotFound();
            }

            return View(nonAdminUser);
        }

        // Handle deletion of a non-admin user (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            var nonAdminUser = db.NonAdminUser.Find(id);

            if (nonAdminUser != null)
            {
                db.NonAdminUser.Remove(nonAdminUser);
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
