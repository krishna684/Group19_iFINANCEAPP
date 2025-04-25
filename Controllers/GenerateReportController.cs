using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Group19_iFINANCEAPP.Models;
using Rotativa;

namespace Group19_iFINANCEAPP.Controllers
{
    // Controller to generate financial reports (Trial Balance and Balance Sheet)
    public class GenerateReportController : Controller
    {
        private Group19_iFINANCEDBEntities db = new Group19_iFINANCEDBEntities();

        // Show report options page
        public ActionResult Index()
        {
            if (Session["UserRole"]?.ToString() != "User")
            {
                return new HttpStatusCodeResult(403);
            }

            return View();
        }

        // Redirect based on report type selection
        public ActionResult Generate(string reportType, DateTime? fromDate, DateTime? toDate)
        {
            if (string.IsNullOrEmpty(reportType))
            {
                return RedirectToAction("Index");
            }

            return RedirectToAction(reportType, new { fromDate, toDate });
        }

        // Show Trial Balance report on screen
        public ActionResult TrialBalance(DateTime? fromDate, DateTime? toDate)
        {
            var model = GetTrialBalanceData(fromDate, toDate);
            return View(model);
        }

        // Generate Trial Balance report as PDF
        public ActionResult TrialBalancePdf()
        {
            var model = GetTrialBalanceData(null, null);
            return new ViewAsPdf("TrialBalance", model)
            {
                FileName = "TrialBalance.pdf"
            };
        }

        // Show Balance Sheet report on screen
        public ActionResult BalanceSheet(DateTime? fromDate, DateTime? toDate)
        {
            var model = GetBalanceSheetData(fromDate, toDate);
            return View(model);
        }

        // Generate Balance Sheet report as PDF
        public ActionResult BalanceSheetPdf()
        {
            var model = GetBalanceSheetData(null, null);
            return new ViewAsPdf("BalanceSheet", model)
            {
                FileName = "BalanceSheet.pdf"
            };
        }

        // Retrieve Trial Balance data for a user
        private List<TrialBalanceEntry> GetTrialBalanceData(DateTime? from, DateTime? to)
        {
            string userId = Session["UserID"].ToString();

            var accounts = db.MasterAccount.Where(m => m.UserID == userId).ToList();
            var lines = db.TransactionLine
                          .Where(t => t.TransactionHeader.NonAdminUserID == userId)
                          .ToList();

            if (from.HasValue)
            {
                lines = lines.Where(t => t.TransactionHeader.Date >= from.Value).ToList();
            }

            if (to.HasValue)
            {
                lines = lines.Where(t => t.TransactionHeader.Date <= to.Value).ToList();
            }

            var result = accounts.Select(account => new TrialBalanceEntry
            {
                AccountName = account.Name,
                Debit = lines.Where(l => l.AccountID == account.ID).Sum(l => l.DebitedAmount),
                Credit = lines.Where(l => l.AccountID == account.ID).Sum(l => l.CreditedAmount)
            }).ToList();

            return result;
        }

        // Retrieve Balance Sheet data for a user
        private List<BalanceSheetEntry> GetBalanceSheetData(DateTime? from, DateTime? to)
        {
            string userId = Session["UserID"].ToString();

            var accounts = db.MasterAccount.Where(m => m.UserID == userId).ToList();
            var lines = db.TransactionLine
                          .Where(t => t.TransactionHeader.NonAdminUserID == userId)
                          .ToList();

            if (from.HasValue)
            {
                lines = lines.Where(t => t.TransactionHeader.Date >= from.Value).ToList();
            }

            if (to.HasValue)
            {
                lines = lines.Where(t => t.TransactionHeader.Date <= to.Value).ToList();
            }

            // Calculate total debits and credits for each account
            var accountBalances = accounts.Select(account => new
            {
                AccountName = account.Name,
                GroupType = account.AccountGroup.ElementType, // Assets, Liabilities, Income, Expenses
                Debit = lines.Where(l => l.AccountID == account.ID).Sum(l => l.DebitedAmount),
                Credit = lines.Where(l => l.AccountID == account.ID).Sum(l => l.CreditedAmount)
            }).ToList();

            // Aggregate totals
            decimal totalAssets = accountBalances
                                    .Where(a => a.GroupType == "Assets")
                                    .Sum(a => a.Debit - a.Credit);

            decimal totalLiabilities = accountBalances
                                        .Where(a => a.GroupType == "Liabilities")
                                        .Sum(a => a.Credit - a.Debit);

            decimal totalIncome = accountBalances
                                    .Where(a => a.GroupType == "Income")
                                    .Sum(a => a.Credit - a.Debit);

            decimal totalExpenses = accountBalances
                                    .Where(a => a.GroupType == "Expenses")
                                    .Sum(a => a.Debit - a.Credit);

            decimal netProfitLoss = totalIncome - totalExpenses;

            // Build balance sheet entries
            var result = new List<BalanceSheetEntry>
            {
                new BalanceSheetEntry { Category = "Total Assets", Type = "Assets", Amount = totalAssets },
                new BalanceSheetEntry { Category = "Total Liabilities", Type = "Liabilities", Amount = totalLiabilities },
                new BalanceSheetEntry { Category = "Net Profit/Loss", Type = "Income-Expense", Amount = netProfitLoss },
            };

            return result;
        }
    }

    // Model for Trial Balance report
    public class TrialBalanceEntry
    {
        public string AccountName { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    }

    // Model for Balance Sheet report
    public class BalanceSheetEntry
    {
        public string Category { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }
    }
}
