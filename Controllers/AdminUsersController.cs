using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Group19_iFINANCEAPP.Models;

namespace Group19_iFINANCEAPP.Controllers
{
    public class AdminUsersController : Controller
    {
        private Group19_iFINANCEDBEntities db = new Group19_iFINANCEDBEntities();

        private bool IsAdmin()
        {
            return Session["UserRole"]?.ToString() == "Admin";
        }

        // GET: AdminUsers
        public ActionResult Index()
        {
            if (!IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            var users = db.UserPassword.ToList();
            return View(users);
        }

        // GET: AdminUsers/Details/5
        public ActionResult Details(string id)
        {
            if (!IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var user = db.UserPassword.Find(id);
            if (user == null) return HttpNotFound();

            return View(user);
        }

        // GET: AdminUsers/Edit/5
        public ActionResult Edit(string id)
        {
            if (!IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var user = db.UserPassword.Find(id);
            if (user == null) return HttpNotFound();

            return View(user);
        }

        // POST: AdminUsers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,UserName,EncryptedPassword,PasswordExpiryTime,UserAccountExpiryDate")] UserPassword user)
        {
            if (!IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (ModelState.IsValid)
            {
                db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(user);
        }

        // GET: AdminUsers/Delete/5
        public ActionResult Delete(string id)
        {
            if (!IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var user = db.UserPassword.Find(id);
            if (user == null) return HttpNotFound();

            return View(user);
        }

        // POST: AdminUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            var user = db.UserPassword.Find(id);

            if (!IsAdmin() || user == null)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            // Remove from Admin or NonAdmin first
            var admin = db.Administrator.Find(id);
            if (admin != null) db.Administrator.Remove(admin);

            var nonAdmin = db.NonAdminUser.Find(id);
            if (nonAdmin != null) db.NonAdminUser.Remove(nonAdmin);

            db.UserPassword.Remove(user);
            db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
