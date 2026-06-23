using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using HospitalMS.Models;

namespace HospitalMS.Controllers
{
    [Authorize]
    public class SalaryController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // ── GET: Salary/Index ────────────────────────────────────
        public ActionResult Index(string month = "", string year = "",
                                  string empType = "", string status = "")
        {
            ViewBag.ActiveMenu = "Salary";

            int m = string.IsNullOrEmpty(month) ? DateTime.Today.Month : int.Parse(month);
            int y = string.IsNullOrEmpty(year) ? DateTime.Today.Year : int.Parse(year);

            var query = db.Salaries.Where(s => s.Month == m && s.Year == y);

            if (!string.IsNullOrEmpty(empType)) query = query.Where(s => s.EmployeeType == empType);
            if (status == "Paid") query = query.Where(s => s.IsPaid);
            if (status == "Unpaid") query = query.Where(s => !s.IsPaid);

            ViewBag.Month = m;
            ViewBag.Year = y;
            ViewBag.MonthName = new DateTime(y, m, 1).ToString("MMMM yyyy");
            ViewBag.EmpType = empType;
            ViewBag.StatusVal = status;

            var list = query.OrderBy(s => s.EmployeeName).ToList();

            // Stats
            ViewBag.TotalRecords = list.Count;
            ViewBag.TotalPaid = list.Count(s => s.IsPaid);
            ViewBag.TotalUnpaid = list.Count(s => !s.IsPaid);
            ViewBag.TotalAmount = list.Sum(s => s.NetSalary);
            ViewBag.PaidAmount = list.Where(s => s.IsPaid).Sum(s => s.NetSalary);
            ViewBag.UnpaidAmount = list.Where(s => !s.IsPaid).Sum(s => s.NetSalary);

            return View(list);
        }

        // ── GET: Salary/Generate ─────────────────────────────────
        // Auto-generate salary records for all active staff/doctors for a month
        public ActionResult Generate()
        {
            ViewBag.ActiveMenu = "Salary";
            ViewBag.Month = DateTime.Today.Month;
            ViewBag.Year = DateTime.Today.Year;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Generate(int month, int year, string empType)
        {
            int created = 0, skipped = 0;

            if (empType == "Staff" || empType == "Both")
            {
                var staffList = db.Staffs.Include("Designation")
                    .Where(s => s.Status == "Active").ToList();

                foreach (var s in staffList)
                {
                    if (db.Salaries.Any(sal => sal.EmployeeId == s.Id && sal.EmployeeType == "Staff"
                        && sal.Month == month && sal.Year == year))
                    { skipped++; continue; }

                    // Calculate attendance based deductions
                    var attendance = db.Attendances.Where(a =>
                        a.EmployeeId == s.Id && a.EmployeeType == "Staff" &&
                        a.Date.Month == month && a.Date.Year == year).ToList();

                    int presentDays = attendance.Count(a => a.Status == "Present" || a.Status == "Late");
                    int absentDays = attendance.Count(a => a.Status == "Absent");

                    decimal basic = s.Designation != null ? s.Designation.BasicSalary : 0;
                    decimal perDaySalary = basic / 30;
                    decimal deduction = absentDays * perDaySalary;

                    db.Salaries.Add(new Salary
                    {
                        EmployeeId = s.Id,
                        EmployeeType = "Staff",
                        EmployeeName = s.FullName,
                        Month = month,
                        Year = year,
                        BasicSalary = basic,
                        Allowances = 0,
                        Bonus = 0,
                        Deductions = Math.Round(deduction, 0),
                        OvertimeAmount = 0,
                        NetSalary = Math.Round(basic - deduction, 0),
                        PresentDays = presentDays,
                        AbsentDays = absentDays,
                        IsPaid = false,
                        CreatedAt = DateTime.Now
                    });
                    created++;
                }
            }

            if (empType == "Doctor" || empType == "Both")
            {
                var docList = db.Doctors.Where(d => d.IsActive).ToList();
                foreach (var d in docList)
                {
                    if (db.Salaries.Any(sal => sal.EmployeeId == d.Id && sal.EmployeeType == "Doctor"
                        && sal.Month == month && sal.Year == year))
                    { skipped++; continue; }

                    var attendance = db.Attendances.Where(a =>
                        a.EmployeeId == d.Id && a.EmployeeType == "Doctor" &&
                        a.Date.Month == month && a.Date.Year == year).ToList();

                    int presentDays = attendance.Count(a => a.Status == "Present" || a.Status == "Late");
                    int absentDays = attendance.Count(a => a.Status == "Absent");

                    // For doctors, base on consultation fee * estimated patients (simple base salary placeholder)
                    decimal basic = 0; // Doctors often paid per consultation - set 0, admin can edit manually

                    db.Salaries.Add(new Salary
                    {
                        EmployeeId = d.Id,
                        EmployeeType = "Doctor",
                        EmployeeName = "Dr. " + d.FullName,
                        Month = month,
                        Year = year,
                        BasicSalary = basic,
                        Allowances = 0,
                        Bonus = 0,
                        Deductions = 0,
                        OvertimeAmount = 0,
                        NetSalary = basic,
                        PresentDays = presentDays,
                        AbsentDays = absentDays,
                        IsPaid = false,
                        CreatedAt = DateTime.Now
                    });
                    created++;
                }
            }

            db.SaveChanges();
            TempData["Success"] = created + " salary records generated for " +
                new DateTime(year, month, 1).ToString("MMMM yyyy") +
                (skipped > 0 ? " (" + skipped + " already existed)" : "");
            return RedirectToAction("Index", new { month = month, year = year });
        }

        // ── GET: Salary/Edit/5 ────────────────────────────────────
        public ActionResult Edit(int id)
        {
            ViewBag.ActiveMenu = "Salary";
            var sal = db.Salaries.Find(id);
            if (sal == null) { TempData["Error"] = "Salary record not found!"; return RedirectToAction("Index"); }
            return View(sal);
        }

        // ── POST: Salary/Edit/5 ───────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(Salary model)
        {
            ModelState.Remove("CreatedAt");
            ModelState.Remove("EmployeeName");

            if (ModelState.IsValid)
            {
                // Recalculate net salary
                model.NetSalary = model.BasicSalary + model.Allowances + model.Bonus
                                  + model.OvertimeAmount - model.Deductions;

                var existing = db.Salaries.Find(model.Id);
                if (existing == null) { TempData["Error"] = "Not found!"; return RedirectToAction("Index"); }

                existing.BasicSalary = model.BasicSalary;
                existing.Allowances = model.Allowances;
                existing.Bonus = model.Bonus;
                existing.Deductions = model.Deductions;
                existing.OvertimeAmount = model.OvertimeAmount;
                existing.NetSalary = model.NetSalary;
                existing.Remarks = model.Remarks;
                existing.IsPaid = model.IsPaid;
                if (model.IsPaid && existing.PaymentDate == null)
                    existing.PaymentDate = DateTime.Now;
                if (!model.IsPaid)
                    existing.PaymentDate = null;

                db.SaveChanges();
                TempData["Success"] = "Salary record updated successfully!";
                return RedirectToAction("Index", new { month = existing.Month, year = existing.Year });
            }

            return View(model);
        }

        // ── GET: Salary/Details/5 ─────────────────────────────────
        public ActionResult Details(int id)
        {
            ViewBag.ActiveMenu = "Salary";
            var sal = db.Salaries.Find(id);
            if (sal == null) { TempData["Error"] = "Not found!"; return RedirectToAction("Index"); }

            // Get employee photo
            string photo = "";
            string designation = "";
            if (sal.EmployeeType == "Staff")
            {
                var staff = db.Staffs.Include("Designation").FirstOrDefault(s => s.Id == sal.EmployeeId);
                if (staff != null) { photo = staff.PhotoPath; designation = staff.Designation != null ? staff.Designation.Name : ""; }
            }
            else
            {
                var doc = db.Doctors.FirstOrDefault(d => d.Id == sal.EmployeeId);
                if (doc != null) { photo = doc.PhotoPath; designation = doc.Specialization; }
            }
            ViewBag.Photo = photo;
            ViewBag.Designation = designation;

            return View(sal);
        }

        // ── Mark as Paid ───────────────────────────────────────────
        public ActionResult MarkPaid(int id)
        {
            var sal = db.Salaries.Find(id);
            if (sal != null)
            {
                sal.IsPaid = true;
                sal.PaymentDate = DateTime.Now;
                db.SaveChanges();
                TempData["Success"] = sal.EmployeeName + "'s salary marked as PAID!";
            }
            return RedirectToAction("Index", new { month = sal?.Month, year = sal?.Year });
        }

        // ── Mark as Unpaid ──────────────────────────────────────────
        public ActionResult MarkUnpaid(int id)
        {
            var sal = db.Salaries.Find(id);
            if (sal != null)
            {
                sal.IsPaid = false;
                sal.PaymentDate = null;
                db.SaveChanges();
                TempData["Success"] = "Marked as Unpaid!";
            }
            return RedirectToAction("Index", new { month = sal?.Month, year = sal?.Year });
        }

        // ── Bulk pay all ────────────────────────────────────────────
        public ActionResult PayAll(int month, int year)
        {
            var unpaid = db.Salaries.Where(s => s.Month == month && s.Year == year && !s.IsPaid).ToList();
            foreach (var s in unpaid)
            {
                s.IsPaid = true;
                s.PaymentDate = DateTime.Now;
            }
            db.SaveChanges();
            TempData["Success"] = unpaid.Count + " salaries marked as PAID!";
            return RedirectToAction("Index", new { month = month, year = year });
        }

        // ── Delete ──────────────────────────────────────────────────
        public ActionResult Delete(int id)
        {
            var sal = db.Salaries.Find(id);
            if (sal == null) { TempData["Error"] = "Not found!"; return RedirectToAction("Index"); }
            int m = sal.Month, y = sal.Year;
            db.Salaries.Remove(sal);
            db.SaveChanges();
            TempData["Success"] = "Salary record deleted!";
            return RedirectToAction("Index", new { month = m, year = y });
        }

        // ── GET: Salary/Payslip/5 ─────────────────────────────────
        public ActionResult Payslip(int id)
        {
            ViewBag.ActiveMenu = "Salary";
            var sal = db.Salaries.Find(id);
            if (sal == null) { TempData["Error"] = "Not found!"; return RedirectToAction("Index"); }

            string photo = "", designation = "", phone = "", email = "";
            if (sal.EmployeeType == "Staff")
            {
                var staff = db.Staffs.Include("Designation").Include("Department").FirstOrDefault(s => s.Id == sal.EmployeeId);
                if (staff != null)
                {
                    photo = staff.PhotoPath;
                    designation = staff.Designation != null ? staff.Designation.Name : "";
                    phone = staff.Phone; email = staff.Email;
                }
            }
            else
            {
                var doc = db.Doctors.FirstOrDefault(d => d.Id == sal.EmployeeId);
                if (doc != null) { photo = doc.PhotoPath; designation = doc.Specialization; phone = doc.Phone; email = doc.Email; }
            }
            ViewBag.Photo = photo;
            ViewBag.Designation = designation;
            ViewBag.Phone = phone;
            ViewBag.Email = email;
            ViewBag.Hospital = db.Hospitals.FirstOrDefault(h => h.IsActive);

            return View(sal);
        }

        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}
