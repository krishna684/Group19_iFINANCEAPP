using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Group19_iFINANCEAPP.Models;

namespace Group19_iFINANCEAPP.Controllers
{
    public class TransactionHeadersController : Controller
    {
        private Group19_iFINANCEDBEntities db = new Group19_iFINANCEDBEntities();

        private bool IsLoggedIn() => Session["UserID"] != null;
        private bool IsAdmin() => Session["UserRole"]?.ToString() == "Admin";
        private string CurrentUserID() => Session["UserID"].ToString();

        private string GenerateTransactionID(string userID)
        {
            var last = db.TransactionHeader
                         .Where(t => t.NonAdminUserID == userID && t.ID.StartsWith(userID + "_TRX"))
                         .OrderByDescending(t => t.ID)
                         .FirstOrDefault();

            int next = 1;
            if (last != null && last.ID.Length > userID.Length + 4)
            {
                string numPart = last.ID.Substring(userID.Length + 4);
                if (int.TryParse(numPart, out int lastNum)) next = lastNum + 1;
            }
            return $"{userID}_TRX{next:D4}";
        }

        public ActionResult Index()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

            string uid = CurrentUserID();
            var transactions = db.TransactionHeader
                                 .Include(t => t.TransactionLine)
                                 .Where(t => t.NonAdminUserID == uid)
                                 .ToList();

            return View(transactions);
        }

        public ActionResult Details(string id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            string userID = CurrentUserID(); // ✅ cache it first
            var header = db.TransactionHeader
                           .Include(t => t.TransactionLine)
                           .FirstOrDefault(t => t.ID == id);

            if (header == null || header.NonAdminUserID != userID)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            return View(header);
        }


        public ActionResult Create()
        {
            if (!IsLoggedIn() || IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            string userID = CurrentUserID();

            ViewBag.MasterAccounts = db.MasterAccount
                .Where(m => m.UserID == userID)
                .Select(m => new SelectListItem { Value = m.ID, Text = m.Name })
                .ToList();

            var header = new TransactionHeader
            {
                ID = GenerateTransactionID(userID),
                Date = DateTime.Now
            };

            return View(header);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TransactionHeader header, List<TransactionLine> Lines)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            string userID = CurrentUserID();

            if (Lines == null || Lines.Count < 2)
                ModelState.AddModelError("", "❌ At least two transaction lines are required.");

            decimal totalDebit = 0m, totalCredit = 0m;
            bool allLinesValid = true;

            foreach (var line in Lines)
            {
                bool valid = (line.DebitedAmount > 0 && line.CreditedAmount == 0)
                          || (line.DebitedAmount == 0 && line.CreditedAmount > 0);

                if (!valid)
                {
                    ModelState.AddModelError("", "❌ Each line must have either a debit or a credit, not both.");
                    allLinesValid = false;
                    break;
                }

                totalDebit += line.DebitedAmount;
                totalCredit += line.CreditedAmount;
            }

            if (totalDebit != totalCredit)
            {
                ModelState.AddModelError("", "❌ Total debits must equal total credits.");
                allLinesValid = false;
            }

            if (ModelState.IsValid && allLinesValid)
            {
                header.NonAdminUserID = userID;
                db.TransactionHeader.Add(header);
                db.SaveChanges();

                foreach (var line in Lines)
                {
                    line.ID = Guid.NewGuid().ToString();
                    line.TransactionID = header.ID;
                    db.TransactionLine.Add(line);
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.MasterAccounts = db.MasterAccount
                .Where(m => m.UserID == userID)
                .Select(m => new SelectListItem { Value = m.ID, Text = m.Name })
                .ToList();

            return View(header);
        }

        public ActionResult Edit(string id)
        {
            if (!IsLoggedIn() || IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var header = db.TransactionHeader.Include(t => t.TransactionLine).FirstOrDefault(t => t.ID == id);
            if (header == null || header.NonAdminUserID != CurrentUserID()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            string userID = CurrentUserID();
            ViewBag.MasterAccounts = db.MasterAccount
                .Where(m => m.UserID == userID)
                .Select(m => new SelectListItem { Value = m.ID, Text = m.Name })
                .ToList();

            return View(header);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(TransactionHeader header, List<TransactionLine> Lines)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            string userID = Session["UserID"].ToString();

            if (Lines == null || Lines.Count < 2)
                ModelState.AddModelError("", "❌ At least two transaction lines are required.");

            decimal totalDebit = 0m, totalCredit = 0m;
            bool allLinesValid = true;

            foreach (var line in Lines)
            {
                bool validLine = (line.DebitedAmount > 0 && line.CreditedAmount == 0)
                              || (line.DebitedAmount == 0 && line.CreditedAmount > 0);

                if (!validLine)
                {
                    ModelState.AddModelError("", "❌ Each line must have either debit or credit — not both or none.");
                    allLinesValid = false;
                    break;
                }

                totalDebit += line.DebitedAmount;
                totalCredit += line.CreditedAmount;
            }

            if (totalDebit != totalCredit)
            {
                ModelState.AddModelError("", "❌ Total debit must equal total credit.");
                allLinesValid = false;
            }

            if (ModelState.IsValid && allLinesValid)
            {
                header.NonAdminUserID = userID;
                db.Entry(header).State = EntityState.Modified;

                // Delete existing lines
                var oldLines = db.TransactionLine.Where(t => t.TransactionID == header.ID).ToList();
                foreach (var old in oldLines) db.TransactionLine.Remove(old);
                db.SaveChanges();

                // Add new lines
                foreach (var line in Lines)
                {
                    line.ID = Guid.NewGuid().ToString();
                    line.TransactionID = header.ID;
                    db.TransactionLine.Add(line);
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // ✅ Preserve user's lines on error
            header.TransactionLine = Lines;

            ViewBag.MasterAccounts = db.MasterAccount
                .Where(m => m.UserID == userID)
                .Select(m => new SelectListItem { Value = m.ID, Text = m.Name })
                .ToList();

            return View(header);
        }

        public ActionResult Delete(string id)
        {
            if (!IsLoggedIn() || IsAdmin()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var header = db.TransactionHeader.Include(t => t.TransactionLine).FirstOrDefault(t => t.ID == id);
            if (header == null || header.NonAdminUserID != CurrentUserID()) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            return View(header);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            string userID = CurrentUserID();

            var transaction = db.TransactionHeader.Include(t => t.TransactionLine).FirstOrDefault(t => t.ID == id);
            if (transaction == null || transaction.NonAdminUserID != userID)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            bool isLocked = false;
            if (isLocked)
            {
                TempData["DeleteError"] = "❌ This transaction is locked and cannot be deleted.";
                return RedirectToAction("Delete", new { id = id });
            }

            foreach (var line in transaction.TransactionLine.ToList())
            {
                db.TransactionLine.Remove(line);
            }

            db.TransactionHeader.Remove(transaction);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
