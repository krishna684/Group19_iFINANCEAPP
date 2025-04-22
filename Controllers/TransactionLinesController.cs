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
    public class TransactionLinesController : Controller
    {
        private Group19_iFINANCEDBEntities db = new Group19_iFINANCEDBEntities();

        // GET: TransactionLines
        public ActionResult Index()
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Auth");
            if (Session["UserRole"].ToString() == "Admin")
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }


            string userID = Session["UserID"].ToString();
            string userRole = Session["UserRole"].ToString();

            var transactionLine = db.TransactionLine
                .Include(t => t.MasterAccount)
                .Include(t => t.TransactionHeader);

            if (userRole != "Admin")
            {
                transactionLine = transactionLine
                    .Where(t => t.TransactionHeader.NonAdminUserID == userID);
            }

            return View(transactionLine.ToList());
        }

        // GET: TransactionLines/Details/5
        public ActionResult Details(string id)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Auth");
            if (Session["UserRole"].ToString() == "Admin")
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }


            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            TransactionLine transactionLine = db.TransactionLine.Find(id);
            if (transactionLine == null) return HttpNotFound();

            if (Session["UserRole"].ToString() != "Admin" &&
                transactionLine.TransactionHeader.NonAdminUserID != Session["UserID"].ToString())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            return View(transactionLine);
        }

        // GET: TransactionLines/Create
        public ActionResult Create()
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Auth");
            if (Session["UserRole"].ToString() == "Admin")
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }


            string userID = Session["UserID"].ToString();
            string userRole = Session["UserRole"].ToString();

            ViewBag.AccountID = new SelectList(db.MasterAccount, "ID", "Name");

            if (userRole == "Admin")
            {
                ViewBag.TransactionID = new SelectList(db.TransactionHeader, "ID", "Description");
            }
            else
            {
                ViewBag.TransactionID = new SelectList(db.TransactionHeader
                    .Where(t => t.NonAdminUserID == userID), "ID", "Description");
            }

            return View();
        }

        // POST: TransactionLines/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,TransactionID,AccountID,CreditedAmount,DebitedAmount,Comments")] TransactionLine transactionLine)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Auth");

            string userID = Session["UserID"].ToString();
            string userRole = Session["UserRole"].ToString();

            var transaction = db.TransactionHeader.Find(transactionLine.TransactionID);

            if (transaction == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (userRole != "Admin" && transaction.NonAdminUserID != userID)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (ModelState.IsValid)
            {
                db.TransactionLine.Add(transactionLine);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AccountID = new SelectList(db.MasterAccount, "ID", "Name", transactionLine.AccountID);
            ViewBag.TransactionID = new SelectList(db.TransactionHeader
                .Where(t => userRole == "Admin" || t.NonAdminUserID == userID), "ID", "Description", transactionLine.TransactionID);

            return View(transactionLine);
        }

        // GET: TransactionLines/Edit/5
        public ActionResult Edit(string id)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Auth");
            if (Session["UserRole"].ToString() == "Admin")
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }


            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            TransactionLine transactionLine = db.TransactionLine.Find(id);
            if (transactionLine == null) return HttpNotFound();

            string userID = Session["UserID"].ToString();
            string userRole = Session["UserRole"].ToString();

            if (userRole != "Admin" && transactionLine.TransactionHeader.NonAdminUserID != userID)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            ViewBag.AccountID = new SelectList(db.MasterAccount, "ID", "Name", transactionLine.AccountID);
            ViewBag.TransactionID = new SelectList(db.TransactionHeader
                .Where(t => userRole == "Admin" || t.NonAdminUserID == userID), "ID", "Description", transactionLine.TransactionID);

            return View(transactionLine);
        }

        // POST: TransactionLines/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,TransactionID,AccountID,CreditedAmount,DebitedAmount,Comments")] TransactionLine transactionLine)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Auth");

            var transaction = db.TransactionHeader.Find(transactionLine.TransactionID);
            if (transaction == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            string userID = Session["UserID"].ToString();
            string userRole = Session["UserRole"].ToString();

            if (userRole != "Admin" && transaction.NonAdminUserID != userID)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (ModelState.IsValid)
            {
                db.Entry(transactionLine).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AccountID = new SelectList(db.MasterAccount, "ID", "Name", transactionLine.AccountID);
            ViewBag.TransactionID = new SelectList(db.TransactionHeader
                .Where(t => userRole == "Admin" || t.NonAdminUserID == userID), "ID", "Description", transactionLine.TransactionID);

            return View(transactionLine);
        }

        // GET: TransactionLines/Delete/5
        public ActionResult Delete(string id)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Auth");
            if (Session["UserRole"].ToString() == "Admin")
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }


            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            TransactionLine transactionLine = db.TransactionLine.Find(id);
            if (transactionLine == null) return HttpNotFound();

            if (Session["UserRole"].ToString() != "Admin" &&
                transactionLine.TransactionHeader.NonAdminUserID != Session["UserID"].ToString())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            return View(transactionLine);
        }

        // POST: TransactionLines/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            TransactionLine transactionLine = db.TransactionLine.Find(id);

            if (Session["UserRole"].ToString() != "Admin" &&
                transactionLine.TransactionHeader.NonAdminUserID != Session["UserID"].ToString())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            db.TransactionLine.Remove(transactionLine);
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
