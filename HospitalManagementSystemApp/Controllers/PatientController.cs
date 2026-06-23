using System;
using System.Linq;
using System.Web.Mvc;
using HospitalMS.Models;

namespace HospitalMS.Controllers
{
    [Authorize]
    public class PatientController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // ── GET: Patient/Index ───────────────────────────────────
        public ActionResult Index(string search = "", int branchId = 0,
                                  string gender = "", string bloodGroup = "",
                                  string patientType = "")
        {
            ViewBag.ActiveMenu = "Patient";

            var query = db.Patients.Include("Branch").AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p =>
                    p.FullName.Contains(search) ||
                    p.Phone.Contains(search) ||
                    p.Email.Contains(search) ||
                    p.EmergencyContact.Contains(search));

            if (branchId > 0) query = query.Where(p => p.BranchId == branchId);
            if (!string.IsNullOrEmpty(gender)) query = query.Where(p => p.Gender == gender);
            if (!string.IsNullOrEmpty(bloodGroup)) query = query.Where(p => p.BloodGroup == bloodGroup);
            if (!string.IsNullOrEmpty(patientType)) query = query.Where(p => p.PatientType == patientType);

            // Filter dropdowns
            ViewBag.BranchList = new SelectList(db.Branches.Where(b => b.IsActive).OrderBy(b => b.Name).ToList(), "Id", "Name", branchId);
            ViewBag.SearchVal = search;
            ViewBag.BranchIdVal = branchId;
            ViewBag.GenderVal = gender;
            ViewBag.BloodVal = bloodGroup;
            ViewBag.TypeVal = patientType;

            // Stats
            ViewBag.TotalPatients = db.Patients.Count();
            ViewBag.OPDPatients = db.Patients.Count(p => p.PatientType == "OPD");
            ViewBag.IPDPatients = db.Patients.Count(p => p.PatientType == "IPD");
            ViewBag.TodayReg = db.Patients.Count(p =>
                p.RegistrationDate.Year == DateTime.Today.Year &&
                p.RegistrationDate.Month == DateTime.Today.Month &&
                p.RegistrationDate.Day == DateTime.Today.Day);

            return View(query.OrderByDescending(p => p.RegistrationDate).ToList());
        }

        // ── GET: Patient/Create ──────────────────────────────────
        public ActionResult Create()
        {
            ViewBag.ActiveMenu = "Patient";
            LoadDropdowns();
            return View(new Patient
            {
                RegistrationDate = DateTime.Today,
                PatientType = "OPD"
            });
        }

        // ── POST: Patient/Create ─────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(Patient model)
        {
            ModelState.Remove("CreatedAt");
            ModelState.Remove("Appointments");
            ModelState.Remove("Billings");

            // Phone duplicate check
            if (!string.IsNullOrEmpty(model.Phone) &&
                db.Patients.Any(p => p.Phone == model.Phone))
                ModelState.AddModelError("Phone", "A patient with this phone number already exists.");

            if (ModelState.IsValid)
            {
                model.CreatedAt = DateTime.Now;
                db.Patients.Add(model);
                db.SaveChanges();
                TempData["Success"] = "Patient \"" + model.FullName + "\" registered successfully! ID: P-" + model.Id.ToString("D4");
                return RedirectToAction("Index");
            }

            LoadDropdowns(model);
            return View(model);
        }

        // ── GET: Patient/Edit/5 ──────────────────────────────────
        public ActionResult Edit(int id)
        {
            ViewBag.ActiveMenu = "Patient";
            var patient = db.Patients.Find(id);
            if (patient == null) { TempData["Error"] = "Patient not found!"; return RedirectToAction("Index"); }
            LoadDropdowns(patient);
            return View(patient);
        }

        // ── POST: Patient/Edit/5 ─────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(Patient model)
        {
            ModelState.Remove("CreatedAt");
            ModelState.Remove("Appointments");
            ModelState.Remove("Billings");

            if (!string.IsNullOrEmpty(model.Phone) &&
                db.Patients.Any(p => p.Phone == model.Phone && p.Id != model.Id))
                ModelState.AddModelError("Phone", "Another patient with this phone already exists.");

            if (ModelState.IsValid)
            {
                db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Patient updated successfully!";
                return RedirectToAction("Index");
            }

            LoadDropdowns(model);
            return View(model);
        }

        // ── GET: Patient/Details/5 ───────────────────────────────
        public ActionResult Details(int id)
        {
            ViewBag.ActiveMenu = "Patient";
            var patient = db.Patients.Include("Branch").FirstOrDefault(p => p.Id == id);
            if (patient == null) { TempData["Error"] = "Patient not found!"; return RedirectToAction("Index"); }

            ViewBag.TotalAppointments = db.Appointments.Count(a => a.PatientId == id);
            ViewBag.PendingAppointments = db.Appointments.Count(a => a.PatientId == id && a.Status == "Pending");
            ViewBag.CompletedAppointments = db.Appointments.Count(a => a.PatientId == id && a.Status == "Completed");
            ViewBag.TotalBills = db.Billings.Count(b => b.PatientId == id);
            ViewBag.TotalBillAmount = db.Billings.Where(b => b.PatientId == id).Sum(b => (decimal?)b.TotalAmount) ?? 0;
            ViewBag.PaidAmount = db.Billings.Where(b => b.PatientId == id).Sum(b => (decimal?)b.PaidAmount) ?? 0;
            ViewBag.PendingAmount = ViewBag.TotalBillAmount - ViewBag.PaidAmount;

            // Recent appointments
            ViewBag.RecentAppointments = db.Appointments
                .Include("Doctor")
                .Where(a => a.PatientId == id)
                .OrderByDescending(a => a.AppointmentDate)
                .Take(5).ToList();

            return View(patient);
        }

        // ── GET: Patient/Delete/5 ────────────────────────────────
        public ActionResult Delete(int id)
        {
            var patient = db.Patients.Find(id);
            if (patient == null) { TempData["Error"] = "Patient not found!"; return RedirectToAction("Index"); }

            if (db.Appointments.Any(a => a.PatientId == id))
            { TempData["Error"] = "Cannot delete! Patient has existing appointments."; return RedirectToAction("Index"); }

            if (db.Billings.Any(b => b.PatientId == id))
            { TempData["Error"] = "Cannot delete! Patient has existing billing records."; return RedirectToAction("Index"); }

            db.Patients.Remove(patient);
            db.SaveChanges();
            TempData["Success"] = "Patient deleted successfully!";
            return RedirectToAction("Index");
        }

        // ── Helpers ──────────────────────────────────────────────
        private void LoadDropdowns(Patient m = null)
        {
            ViewBag.BranchId = new SelectList(
                db.Branches.Where(b => b.IsActive).OrderBy(b => b.Name).ToList(),
                "Id", "Name", m?.BranchId);
        }

        protected override void Dispose(bool disposing)
        { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}
