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
    public class NonAdminUsersController : Controller
    {
        private Group19_iFINANCEDBEntities db = new Group19_iFINANCEDBEntities();

        // GET: NonAdminUsers
        public ActionResult Index()
        {
            var nonAdminUser = db.NonAdminUser.Include(n => n.UserPassword);
            return View(nonAdminUser.ToList());
        }

        // GET: NonAdminUsers/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NonAdminUser nonAdminUser = db.NonAdminUser.Find(id);
            if (nonAdminUser == null)
            {
                return HttpNotFound();
            }
            return View(nonAdminUser);
        }

        // GET: NonAdminUsers/Create
        public ActionResult Create()
        {
            ViewBag.ID = new SelectList(db.UserPassword, "ID", "UserName");
            return View();
        }

        // POST: NonAdminUsers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: NonAdminUsers/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NonAdminUser nonAdminUser = db.NonAdminUser.Find(id);
            if (nonAdminUser == null)
            {
                return HttpNotFound();
            }
            ViewBag.ID = new SelectList(db.UserPassword, "ID", "UserName", nonAdminUser.ID);
            return View(nonAdminUser);
        }

        // POST: NonAdminUsers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: NonAdminUsers/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NonAdminUser nonAdminUser = db.NonAdminUser.Find(id);
            if (nonAdminUser == null)
            {
                return HttpNotFound();
            }
            return View(nonAdminUser);
        }

        // POST: NonAdminUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            NonAdminUser nonAdminUser = db.NonAdminUser.Find(id);
            db.NonAdminUser.Remove(nonAdminUser);
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
