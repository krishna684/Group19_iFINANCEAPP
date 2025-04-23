using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Group19_iFINANCEAPP.Models;

namespace Group19_iFINANCEAPP.Controllers
{
    public class GenerateReportController : Controller
    {
        private Group19_iFINANCEDBEntities db = new Group19_iFINANCEDBEntities();

        private bool IsUser()
        {
            return Session["UserRole"]?.ToString() == "User";
        }

        // GET: /GenerateReport/TrialBalance
        public ActionResult TrialBalance()
        {
            if (Session["UserID"] == null || !IsUser())
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);

            string userID = Session["UserID"].ToString();

            var trialBalance = db.MasterAccount
                .Where(m => m.UserID == userID)
                .Select(m => new TrialBalanceEntry
                {
                    AccountName = m.Name,
                    Debit = db.TransactionLine
                                .Where(tl => tl.AccountID == m.ID)
                                .Sum(tl => (decimal?)tl.DebitedAmount) ?? 0,
                    Credit = db.TransactionLine
                                .Where(tl => tl.AccountID == m.ID)
                                .Sum(tl => (decimal?)tl.CreditedAmount) ?? 0
                }).ToList();

            return View(trialBalance);
        }

        // GET: /GenerateReport/BalanceSheet
        public ActionResult BalanceSheet()
        {
            if (Session["UserID"] == null || !IsUser())
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);

            string userID = Session["UserID"].ToString();

            var balanceSheet = db.MasterAccount
                .Where(m => m.UserID == userID)
                .Select(m => new BalanceSheetEntry
                {
                    AccountName = m.Name,
                    Group = m.AccountGroup.Name,
                    Type = m.AccountGroup.ElementType,
                    Balance = m.ClosingAmount
                }).ToList();

            return View(balanceSheet);
        }
    }

    // DTOs for views
    public class TrialBalanceEntry
    {
        public string AccountName { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    }

    public class BalanceSheetEntry
    {
        public string AccountName { get; set; }
        public string Group { get; set; }
        public string Type { get; set; }
        public decimal Balance { get; set; }
    }
}
