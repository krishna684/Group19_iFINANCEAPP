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
    public class TransactionHeadersController : Controller
    {
        private Group19_iFINANCEDBEntities db = new Group19_iFINANCEDBEntities();

        // GET: TransactionHeaders
        public ActionResult Index()
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Auth");

            var transactions = db.TransactionHeader.Include(t => t.NonAdminUser);
            return View(transactions.ToList());
        }

        // GET: TransactionHeaders/Details/5
        public ActionResult Details(string id)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Auth");
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            TransactionHeader transactionHeader = db.TransactionHeader.Find(id);
            if (transactionHeader == null) return HttpNotFound();

            return View(transactionHeader);
        }

        // GET: TransactionHeaders/Create
        public ActionResult Create()
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Auth");

            ViewBag.NonAdminUserID = new SelectList(db.NonAdminUser, "ID", "Name");
            return View();
        }

        // POST: TransactionHeaders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Date,Description,NonAdminUserID")] TransactionHeader transactionHeader)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Auth");

            if (ModelState.IsValid)
            {
                db.TransactionHeader.Add(transactionHeader);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.NonAdminUserID = new SelectList(db.NonAdminUser, "ID", "Name", transactionHeader.NonAdminUserID);
            return View(transactionHeader);
        }

        // GET: TransactionHeaders/Edit/5
        public ActionResult Edit(string id)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Auth");
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            TransactionHeader transactionHeader = db.TransactionHeader.Find(id);
            if (transactionHeader == null) return HttpNotFound();

            ViewBag.NonAdminUserID = new SelectList(db.NonAdminUser, "ID", "Name", transactionHeader.NonAdminUserID);
            return View(transactionHeader);
        }

        // POST: TransactionHeaders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Date,Description,NonAdminUserID")] TransactionHeader transactionHeader)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Auth");

            if (ModelState.IsValid)
            {
                db.Entry(transactionHeader).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.NonAdminUserID = new SelectList(db.NonAdminUser, "ID", "Name", transactionHeader.NonAdminUserID);
            return View(transactionHeader);
        }

        // GET: TransactionHeaders/Delete/5
        public ActionResult Delete(string id)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Auth");
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            TransactionHeader transactionHeader = db.TransactionHeader.Find(id);
            if (transactionHeader == null) return HttpNotFound();

            return View(transactionHeader);
        }

        // POST: TransactionHeaders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Auth");

            TransactionHeader transactionHeader = db.TransactionHeader.Find(id);
            db.TransactionHeader.Remove(transactionHeader);
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
