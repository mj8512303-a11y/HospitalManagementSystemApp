using System;
using System.Linq;
using System.Web.Mvc;
using HospitalMS.Models;

namespace HospitalMS.Controllers
{
    [Authorize]
    public class BillingController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // ── GET: Billing/Index ───────────────────────────────────
        public ActionResult Index(string search = "", int branchId = 0,
                                  string status = "", string dateFrom = "",
                                  string dateTo = "")
        {
            ViewBag.ActiveMenu = "Billing";

            var query = db.Billings.Include("Patient").Include("Branch").AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(b =>
                    b.Patient.FullName.Contains(search) ||
                    b.InvoiceNo.Contains(search));

            if (branchId > 0) query = query.Where(b => b.BranchId == branchId);
            if (!string.IsNullOrEmpty(status)) query = query.Where(b => b.Status == status);

            DateTime df, dt;
            if (DateTime.TryParse(dateFrom, out df)) query = query.Where(b => b.BillingDate >= df.Date);
            if (DateTime.TryParse(dateTo, out dt)) query = query.Where(b => b.BillingDate <= dt.Date);

            ViewBag.BranchList = new SelectList(db.Branches.Where(b => b.IsActive).OrderBy(b => b.Name).ToList(), "Id", "Name", branchId);
            ViewBag.SearchVal = search;
            ViewBag.BranchIdVal = branchId;
            ViewBag.StatusVal = status;
            ViewBag.DateFromVal = dateFrom;
            ViewBag.DateToVal = dateTo;

            // Stats
            ViewBag.TotalBills = db.Billings.Count();
            ViewBag.TotalAmount = db.Billings.Sum(b => (decimal?)b.TotalAmount) ?? 0;
            ViewBag.PaidAmount = db.Billings.Sum(b => (decimal?)b.PaidAmount) ?? 0;
            ViewBag.PendingAmount = ViewBag.TotalAmount - ViewBag.PaidAmount;
            ViewBag.PaidBills = db.Billings.Count(b => b.Status == "Paid");
            ViewBag.PartialBills = db.Billings.Count(b => b.Status == "Partial");
            ViewBag.UnpaidBills = db.Billings.Count(b => b.Status == "Pending");

            return View(query.OrderByDescending(b => b.BillingDate).ThenByDescending(b => b.Id).ToList());
        }

        // ── GET: Billing/Create ──────────────────────────────────
        public ActionResult Create(int patientId = 0)
        {
            ViewBag.ActiveMenu = "Billing";
            LoadDropdowns();

            var model = new Billing
            {
                BillingDate = DateTime.Today,
                Status = "Pending",
                PaymentMode = "Cash"
            };

            if (patientId > 0)
            {
                model.PatientId = patientId;
                var patient = db.Patients.Find(patientId);
                if (patient != null)
                {
                    ViewBag.SelectedPatient = patient.FullName;
                    model.BranchId = patient.BranchId;
                }
            }

            // Generate Invoice Number
            string invoiceNo = "INV-" + DateTime.Now.Year.ToString() +
                               DateTime.Now.Month.ToString("D2") + "-" +
                               (db.Billings.Count() + 1).ToString("D4");
            ViewBag.InvoiceNo = invoiceNo;

            return View(model);
        }

        // ── POST: Billing/Create ─────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(Billing model)
        {
            ModelState.Remove("CreatedAt");
            ModelState.Remove("Patient");
            ModelState.Remove("Branch");

            if (ModelState.IsValid)
            {
                // Auto-calculate totals
                model.TotalAmount = model.ConsultationFee + model.TestCharges +
                                    model.MedicineCharges + model.OtherCharges - model.Discount;
                model.RemainingAmount = model.TotalAmount - model.PaidAmount;

                // Auto-set status
                if (model.PaidAmount <= 0) model.Status = "Pending";
                else if (model.PaidAmount >= model.TotalAmount) model.Status = "Paid";
                else model.Status = "Partial";

                // Generate Invoice No if empty
                if (string.IsNullOrEmpty(model.InvoiceNo))
                    model.InvoiceNo = "INV-" + DateTime.Now.Year +
                                      DateTime.Now.Month.ToString("D2") + "-" +
                                      (db.Billings.Count() + 1).ToString("D4");

                model.CreatedAt = DateTime.Now;
                db.Billings.Add(model);
                db.SaveChanges();
                TempData["Success"] = "Bill created! Invoice: " + model.InvoiceNo;
                return RedirectToAction("Index");
            }

            LoadDropdowns(model);
            return View(model);
        }

        // ── GET: Billing/Edit/5 ──────────────────────────────────
        public ActionResult Edit(int id)
        {
            ViewBag.ActiveMenu = "Billing";
            var bill = db.Billings.Find(id);
            if (bill == null) { TempData["Error"] = "Bill not found!"; return RedirectToAction("Index"); }
            LoadDropdowns(bill);
            return View(bill);
        }

        // ── POST: Billing/Edit/5 ─────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(Billing model)
        {
            ModelState.Remove("CreatedAt");
            ModelState.Remove("Patient");
            ModelState.Remove("Branch");

            if (ModelState.IsValid)
            {
                model.TotalAmount = model.ConsultationFee + model.TestCharges +
                                        model.MedicineCharges + model.OtherCharges - model.Discount;
                model.RemainingAmount = model.TotalAmount - model.PaidAmount;

                if (model.PaidAmount <= 0) model.Status = "Pending";
                else if (model.PaidAmount >= model.TotalAmount) model.Status = "Paid";
                else model.Status = "Partial";

                db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Bill updated successfully!";
                return RedirectToAction("Index");
            }

            LoadDropdowns(model);
            return View(model);
        }

        // ── GET: Billing/Details/5 ───────────────────────────────
        public ActionResult Details(int id)
        {
            ViewBag.ActiveMenu = "Billing";
            var bill = db.Billings.Include("Patient").Include("Branch")
                          .FirstOrDefault(b => b.Id == id);
            if (bill == null) { TempData["Error"] = "Bill not found!"; return RedirectToAction("Index"); }
            return View(bill);
        }

        // ── GET: Billing/Invoice/5 (Printable) ───────────────────
        public ActionResult Invoice(int id)
        {
            ViewBag.ActiveMenu = "Billing";
            var bill = db.Billings.Include("Patient").Include("Branch")
                          .FirstOrDefault(b => b.Id == id);
            if (bill == null) { TempData["Error"] = "Bill not found!"; return RedirectToAction("Index"); }
            ViewBag.Hospital = db.Hospitals.FirstOrDefault(h => h.IsActive);
            return View(bill);
        }

        // ── Quick Pay (add payment) ──────────────────────────────
        [HttpPost]
        public ActionResult QuickPay(int id, decimal amount)
        {
            var bill = db.Billings.Find(id);
            if (bill == null) { TempData["Error"] = "Not found!"; return RedirectToAction("Index"); }

            bill.PaidAmount += amount;
            if (bill.PaidAmount > bill.TotalAmount) bill.PaidAmount = bill.TotalAmount;
            bill.RemainingAmount = bill.TotalAmount - bill.PaidAmount;

            if (bill.PaidAmount <= 0) bill.Status = "Pending";
            else if (bill.PaidAmount >= bill.TotalAmount) bill.Status = "Paid";
            else bill.Status = "Partial";

            db.SaveChanges();
            TempData["Success"] = "Payment of Rs. " + amount.ToString("N0") + " recorded!";
            return RedirectToAction("Details", new { id = id });
        }

        // ── Delete ────────────────────────────────────────────────
        public ActionResult Delete(int id)
        {
            var bill = db.Billings.Find(id);
            if (bill == null) { TempData["Error"] = "Not found!"; return RedirectToAction("Index"); }
            db.Billings.Remove(bill);
            db.SaveChanges();
            TempData["Success"] = "Bill deleted!";
            return RedirectToAction("Index");
        }

        // ── Helpers ───────────────────────────────────────────────
        private void LoadDropdowns(Billing m = null)
        {
            ViewBag.PatientId = new SelectList(
                db.Patients.OrderBy(p => p.FullName).ToList(), "Id", "FullName", m?.PatientId);
            ViewBag.BranchId = new SelectList(
                db.Branches.Where(b => b.IsActive).OrderBy(b => b.Name).ToList(), "Id", "Name", m?.BranchId);
        }

        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}