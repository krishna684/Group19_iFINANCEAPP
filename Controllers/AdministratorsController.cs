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
    // Controller to manage Administrator information
    public class AdministratorsController : Controller
    {
        private Group19_iFINANCEDBEntities db = new Group19_iFINANCEDBEntities();

        // Display the list of administrators
        public ActionResult Index()
        {
            var administrators = db.Administrator.Include(a => a.UserPassword);
            return View(administrators.ToList());
        }

        // Display the details of a specific administrator
        public ActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var administrator = db.Administrator.Find(id);
            if (administrator == null)
            {
                return HttpNotFound();
            }

            return View(administrator);
        }

        // Show the form to create a new administrator
        public ActionResult Create()
        {
            ViewBag.ID = new SelectList(db.UserPassword, "ID", "UserName");
            return View();
        }

        // Handle the creation of a new administrator (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Name,DateHired,DateFinished")] Administrator administrator)
        {
            if (ModelState.IsValid)
            {
                db.Administrator.Add(administrator);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ID = new SelectList(db.UserPassword, "ID", "UserName", administrator.ID);
            return View(administrator);
        }

        // Show the form to edit an existing administrator
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var administrator = db.Administrator.Find(id);
            if (administrator == null)
            {
                return HttpNotFound();
            }

            ViewBag.ID = new SelectList(db.UserPassword, "ID", "UserName", administrator.ID);
            return View(administrator);
        }

        // Handle the editing of an administrator (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Name,DateHired,DateFinished")] Administrator administrator)
        {
            if (ModelState.IsValid)
            {
                db.Entry(administrator).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ID = new SelectList(db.UserPassword, "ID", "UserName", administrator.ID);
            return View(administrator);
        }

        // Show the delete confirmation page for an administrator
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var administrator = db.Administrator.Find(id);
            if (administrator == null)
            {
                return HttpNotFound();
            }

            return View(administrator);
        }

        // Handle the deletion of an administrator (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            var administrator = db.Administrator.Find(id);

            if (administrator != null)
            {
                db.Administrator.Remove(administrator);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        // Dispose the database context when the controller is disposed
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
