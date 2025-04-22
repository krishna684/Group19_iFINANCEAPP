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
    public class MasterAccountsController : Controller
    {
        private Group19_iFINANCEDBEntities db = new Group19_iFINANCEDBEntities();

        // GET: MasterAccounts
        public ActionResult Index()
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Auth");
            if (Session["UserRole"].ToString() == "Admin")
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }


            string userID = Session["UserID"].ToString();
            string userRole = Session["UserRole"].ToString();

            var masterAccount = db.MasterAccount.Include(m => m.AccountGroup);

            if (userRole != "Admin")
            {
                masterAccount = masterAccount.Where(m => m.UserID == userID);
            }

            return View(masterAccount.ToList());
        }

        // GET: MasterAccounts/Details/5
        public ActionResult Details(string id)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Auth");
            if (Session["UserRole"].ToString() == "Admin")
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }


            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            MasterAccount masterAccount = db.MasterAccount.Find(id);
            if (masterAccount == null) return HttpNotFound();

            if (Session["UserRole"].ToString() != "Admin" && masterAccount.UserID != Session["UserID"].ToString())
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            return View(masterAccount);
        }

        // GET: MasterAccounts/Create
        public ActionResult Create()
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Auth");
            if (Session["UserRole"].ToString() == "Admin")
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }


            ViewBag.GroupID = new SelectList(db.AccountGroup, "ID", "Name");
            return View();
        }

        // POST: MasterAccounts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Name,OpeningAmount,ClosingAmount,GroupID")] MasterAccount masterAccount)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Auth");

            if (ModelState.IsValid)
            {
                masterAccount.UserID = Session["UserID"].ToString(); // assign owner
                db.MasterAccount.Add(masterAccount);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.GroupID = new SelectList(db.AccountGroup, "ID", "Name", masterAccount.GroupID);
            return View(masterAccount);
        }

        // GET: MasterAccounts/Edit/5
        public ActionResult Edit(string id)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Auth");
            if (Session["UserRole"].ToString() == "Admin")
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }


            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            MasterAccount masterAccount = db.MasterAccount.Find(id);
            if (masterAccount == null) return HttpNotFound();

            if (Session["UserRole"].ToString() != "Admin" && masterAccount.UserID != Session["UserID"].ToString())
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            ViewBag.GroupID = new SelectList(db.AccountGroup, "ID", "Name", masterAccount.GroupID);
            return View(masterAccount);
        }

        // POST: MasterAccounts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Name,OpeningAmount,ClosingAmount,GroupID")] MasterAccount masterAccount)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Auth");

            if (ModelState.IsValid)
            {
                masterAccount.UserID = Session["UserID"].ToString(); // ensure owner is preserved
                db.Entry(masterAccount).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.GroupID = new SelectList(db.AccountGroup, "ID", "Name", masterAccount.GroupID);
            return View(masterAccount);
        }

        // GET: MasterAccounts/Delete/5
        public ActionResult Delete(string id)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Auth");
            if (Session["UserRole"].ToString() == "Admin")
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }


            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            MasterAccount masterAccount = db.MasterAccount.Find(id);
            if (masterAccount == null) return HttpNotFound();

            if (Session["UserRole"].ToString() != "Admin" && masterAccount.UserID != Session["UserID"].ToString())
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            return View(masterAccount);
        }

        // POST: MasterAccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            MasterAccount masterAccount = db.MasterAccount.Find(id);

            if (Session["UserRole"].ToString() != "Admin" && masterAccount.UserID != Session["UserID"].ToString())
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            db.MasterAccount.Remove(masterAccount);
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
