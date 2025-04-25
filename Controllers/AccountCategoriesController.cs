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
    /// <summary>
    /// Controller for managing Account Categories (Assets, Liabilities, Income, Expenses)
    /// </summary>
    public class AccountCategoriesController : Controller
    {
        private Group19_iFINANCEDBEntities db = new Group19_iFINANCEDBEntities();

        /// <summary>
        /// Displays a list of all account categories.
        /// </summary>
        public ActionResult Index()
        {
            var categories = db.AccountCategory.ToList();
            return View(categories);
        }

        /// <summary>
        /// Displays the details of a specific account category.
        /// </summary>
        public ActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var accountCategory = db.AccountCategory.Find(id);

            if (accountCategory == null)
            {
                return HttpNotFound();
            }

            return View(accountCategory);
        }

        /// <summary>
        /// Displays the form to create a new account category.
        /// </summary>
        public ActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Creates a new account category and saves it to the database.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Name,Type")] AccountCategory accountCategory)
        {
            if (ModelState.IsValid)
            {
                db.AccountCategory.Add(accountCategory);
                db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View(accountCategory);
        }

        /// <summary>
        /// Displays the form to edit an existing account category.
        /// </summary>
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var accountCategory = db.AccountCategory.Find(id);

            if (accountCategory == null)
            {
                return HttpNotFound();
            }

            return View(accountCategory);
        }

        /// <summary>
        /// Updates an existing account category.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Name,Type")] AccountCategory accountCategory)
        {
            if (ModelState.IsValid)
            {
                db.Entry(accountCategory).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View(accountCategory);
        }

        /// <summary>
        /// Displays the confirmation page for deleting an account category.
        /// </summary>
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var accountCategory = db.AccountCategory.Find(id);

            if (accountCategory == null)
            {
                return HttpNotFound();
            }

            return View(accountCategory);
        }

        /// <summary>
        /// Deletes an account category from the database.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            var accountCategory = db.AccountCategory.Find(id);

            if (accountCategory != null)
            {
                db.AccountCategory.Remove(accountCategory);
                db.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Disposes the database context to free resources.
        /// </summary>
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
