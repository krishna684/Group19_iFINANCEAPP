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
    public class AccountGroupsController : Controller
    {
        private Group19_iFINANCEDBEntities db = new Group19_iFINANCEDBEntities();

        // GET: AccountGroups
        public ActionResult Index()
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Auth");
            if (Session["UserRole"].ToString() == "Admin")
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }


            string userID = Session["UserID"].ToString();
            string userRole = Session["UserRole"].ToString();

            var groups = db.AccountGroup.Include(a => a.AccountGroup2);

            if (userRole != "Admin")
            {
                groups = groups.Where(g => g.UserID == userID);
            }

            return View(groups.ToList());
        }

        // GET: AccountGroups/Details/5
        public ActionResult Details(string id)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Auth");
            if (Session["UserRole"].ToString() == "Admin")
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }


            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            AccountGroup accountGroup = db.AccountGroup.Find(id);

            if (accountGroup == null) return HttpNotFound();

            // Non-admins can only access their own
            if (Session["UserRole"].ToString() != "Admin" && accountGroup.UserID != Session["UserID"].ToString())
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            return View(accountGroup);
        }

        // GET: AccountGroups/Create
        public ActionResult Create()
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Auth");
            if (Session["UserRole"].ToString() == "Admin")
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }


            ViewBag.ParentGroupID = new SelectList(db.AccountGroup, "ID", "Name");
            return View();
        }

        // POST: AccountGroups/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Name,ParentGroupID,ElementType")] AccountGroup accountGroup)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Auth");

            if (ModelState.IsValid)
            {
                accountGroup.UserID = Session["UserID"].ToString(); // Assign ownership
                db.AccountGroup.Add(accountGroup);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ParentGroupID = new SelectList(db.AccountGroup, "ID", "Name", accountGroup.ParentGroupID);
            return View(accountGroup);
        }

        // GET: AccountGroups/Edit/5
        public ActionResult Edit(string id)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Auth");

            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            AccountGroup accountGroup = db.AccountGroup.Find(id);
            if (accountGroup == null) return HttpNotFound();

            if (Session["UserRole"].ToString() != "Admin" && accountGroup.UserID != Session["UserID"].ToString())
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            ViewBag.ParentGroupID = new SelectList(db.AccountGroup, "ID", "Name", accountGroup.ParentGroupID);
            return View(accountGroup);
        }

        // POST: AccountGroups/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Name,ParentGroupID,ElementType")] AccountGroup accountGroup)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Auth");

            if (ModelState.IsValid)
            {
                accountGroup.UserID = Session["UserID"].ToString(); // Preserve ownership
                db.Entry(accountGroup).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ParentGroupID = new SelectList(db.AccountGroup, "ID", "Name", accountGroup.ParentGroupID);
            return View(accountGroup);
        }

        // GET: AccountGroups/Delete/5
        public ActionResult Delete(string id)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Auth");
            if (Session["UserRole"].ToString() == "Admin")
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }


            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            AccountGroup accountGroup = db.AccountGroup.Find(id);
            if (accountGroup == null) return HttpNotFound();

            if (Session["UserRole"].ToString() != "Admin" && accountGroup.UserID != Session["UserID"].ToString())
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            return View(accountGroup);
        }

        // POST: AccountGroups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            AccountGroup accountGroup = db.AccountGroup.Find(id);

            if (Session["UserRole"].ToString() != "Admin" && accountGroup.UserID != Session["UserID"].ToString())
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            db.AccountGroup.Remove(accountGroup);
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
