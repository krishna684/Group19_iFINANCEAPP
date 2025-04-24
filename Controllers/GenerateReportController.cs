using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Group19_iFINANCEAPP.Models;
using Rotativa;

namespace Group19_iFINANCEAPP.Controllers
{
    public class GenerateReportController : Controller
    {
        private Group19_iFINANCEDBEntities db = new Group19_iFINANCEDBEntities();

        public ActionResult Index()
        {
            if (Session["UserRole"]?.ToString() != "User")
                return new HttpStatusCodeResult(403);

            return View();
        }

        public ActionResult Generate(string reportType, DateTime? fromDate, DateTime? toDate)
        {
            if (string.IsNullOrEmpty(reportType)) return RedirectToAction("Index");

            return RedirectToAction(reportType, new { fromDate, toDate });
        }

        public ActionResult TrialBalance(DateTime? fromDate, DateTime? toDate)
        {
            var model = GetTrialBalanceData(fromDate, toDate);
            return View(model);
        }

        public ActionResult TrialBalancePdf()
        {
            var model = GetTrialBalanceData(null, null); // 👈 no filtering
            return new Rotativa.ViewAsPdf("TrialBalance", model)
            {
                FileName = "TrialBalance.pdf"
            };
        }

        public ActionResult BalanceSheet(DateTime? fromDate, DateTime? toDate)
        {
            var model = GetBalanceSheetData(fromDate, toDate);
            return View(model);
        }

        public ActionResult BalanceSheetPdf()
        {
            var model = GetBalanceSheetData(null, null); // 👈 no filtering
            return new Rotativa.ViewAsPdf("BalanceSheet", model)
            {
                FileName = "BalanceSheet.pdf"
            };
        }

        private List<TrialBalanceEntry> GetTrialBalanceData(DateTime? from, DateTime? to)
        {
            string userId = Session["UserID"].ToString();
            var accounts = db.MasterAccount.Where(m => m.UserID == userId).ToList();
            var lines = db.TransactionLine
                          .Where(t => t.TransactionHeader.NonAdminUserID == userId)
                          .ToList();

            if (from.HasValue) lines = lines.Where(t => t.TransactionHeader.Date >= from.Value).ToList();
            if (to.HasValue) lines = lines.Where(t => t.TransactionHeader.Date <= to.Value).ToList();

            var result = accounts.Select(account => new TrialBalanceEntry
            {
                AccountName = account.Name,
                Debit = lines.Where(l => l.AccountID == account.ID).Sum(l => l.DebitedAmount),
                Credit = lines.Where(l => l.AccountID == account.ID).Sum(l => l.CreditedAmount)
            }).ToList();

            return result;
        }

        private List<BalanceSheetEntry> GetBalanceSheetData(DateTime? from, DateTime? to)
        {
            string userId = Session["UserID"].ToString();
            var accounts = db.MasterAccount.Where(m => m.UserID == userId).ToList();
            var groups = db.AccountGroup.Where(g => g.UserID == userId).ToList();
            var lines = db.TransactionLine
                          .Where(t => t.TransactionHeader.NonAdminUserID == userId)
                          .ToList();

            if (from.HasValue) lines = lines.Where(t => t.TransactionHeader.Date >= from.Value).ToList();
            if (to.HasValue) lines = lines.Where(t => t.TransactionHeader.Date <= to.Value).ToList();

            var result = groups.Select(group =>
            {
                var groupAccounts = accounts.Where(a => a.GroupID == group.ID).Select(a => a.ID).ToList();
                decimal debit = lines.Where(l => groupAccounts.Contains(l.AccountID)).Sum(l => l.DebitedAmount);
                decimal credit = lines.Where(l => groupAccounts.Contains(l.AccountID)).Sum(l => l.CreditedAmount);
                decimal net = debit - credit;

                return new BalanceSheetEntry
                {
                    Category = group.Name,
                    Type = group.ElementType,
                    Amount = net
                };
            }).ToList();

            return result;
        }
    }

    public class TrialBalanceEntry
    {
        public string AccountName { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    }

    public class BalanceSheetEntry
    {
        public string Category { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }
    }
}
