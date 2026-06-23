using System;
using System.Linq;
using System.Web.Mvc;
using HospitalMS.Models;

namespace HospitalMS.Controllers
{
    [Authorize]
    public class AppointmentController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // Time slots list
        private readonly string[] TimeSlots = {
            "08:00 AM", "08:30 AM", "09:00 AM", "09:30 AM",
            "10:00 AM", "10:30 AM", "11:00 AM", "11:30 AM",
            "12:00 PM", "12:30 PM", "01:00 PM", "01:30 PM",
            "02:00 PM", "02:30 PM", "03:00 PM", "03:30 PM",
            "04:00 PM", "04:30 PM", "05:00 PM", "05:30 PM",
            "06:00 PM", "06:30 PM", "07:00 PM", "07:30 PM"
        };

        // ── GET: Appointment/Index ───────────────────────────────
        public ActionResult Index(string search = "", int doctorId = 0,
                                  string status = "", string date = "",
                                  int branchId = 0)
        {
            ViewBag.ActiveMenu = "Appointment";

            var query = db.Appointments
                .Include("Patient").Include("Doctor").Include("Branch")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(a =>
                    a.Patient.FullName.Contains(search) ||
                    a.Doctor.FullName.Contains(search));

            if (doctorId > 0) query = query.Where(a => a.DoctorId == doctorId);
            if (branchId > 0) query = query.Where(a => a.BranchId == branchId);
            if (!string.IsNullOrEmpty(status)) query = query.Where(a => a.Status == status);

            if (!string.IsNullOrEmpty(date))
            {
                DateTime dt;
                if (DateTime.TryParse(date, out dt))
                    query = query.Where(a => a.AppointmentDate == dt.Date);
            }

            ViewBag.DoctorList = new SelectList(db.Doctors.Where(d => d.IsActive).OrderBy(d => d.FullName).ToList(), "Id", "FullName", doctorId);
            ViewBag.BranchList = new SelectList(db.Branches.Where(b => b.IsActive).OrderBy(b => b.Name).ToList(), "Id", "Name", branchId);
            ViewBag.SearchVal = search;
            ViewBag.DoctorIdVal = doctorId;
            ViewBag.BranchIdVal = branchId;
            ViewBag.StatusVal = status;
            ViewBag.DateVal = date;

            // Stats
            ViewBag.Total = db.Appointments.Count();
            ViewBag.Today = db.Appointments.Count(a => a.AppointmentDate == DateTime.Today);
            ViewBag.Pending = db.Appointments.Count(a => a.Status == "Pending");
            ViewBag.Approved = db.Appointments.Count(a => a.Status == "Approved");
            ViewBag.Completed = db.Appointments.Count(a => a.Status == "Completed");
            ViewBag.Cancelled = db.Appointments.Count(a => a.Status == "Cancelled");

            return View(query.OrderByDescending(a => a.AppointmentDate)
                             .ThenBy(a => a.TimeSlot).ToList());
        }

        // ── GET: Appointment/Create ──────────────────────────────
        public ActionResult Create(int patientId = 0)
        {
            ViewBag.ActiveMenu = "Appointment";
            LoadDropdowns();
            ViewBag.TimeSlots = new SelectList(TimeSlots);

            var model = new Appointment
            {
                AppointmentDate = DateTime.Today.AddDays(1),
                Status = "Pending",
                BookedBy = Session["UserName"] != null ? Session["UserName"].ToString() : "Admin"
            };

            // Pre-select patient if coming from patient details page
            if (patientId > 0)
            {
                model.PatientId = patientId;
                var patient = db.Patients.Find(patientId);
                if (patient != null) ViewBag.SelectedPatient = patient.FullName;
            }

            return View(model);
        }

        // ── POST: Appointment/Create ─────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(Appointment model)
        {
            ModelState.Remove("CreatedAt");
            ModelState.Remove("Patient");
            ModelState.Remove("Doctor");
            ModelState.Remove("Branch");

            // Check if same doctor already has appointment at same time
            if (db.Appointments.Any(a =>
                a.DoctorId == model.DoctorId &&
                a.AppointmentDate == model.AppointmentDate &&
                a.TimeSlot == model.TimeSlot &&
                a.Status != "Cancelled"))
                ModelState.AddModelError("TimeSlot", "This doctor already has an appointment at this time slot.");

            // Appointment date must not be in the past
            if (model.AppointmentDate.Date < DateTime.Today)
                ModelState.AddModelError("AppointmentDate", "Appointment date cannot be in the past.");

            if (ModelState.IsValid)
            {
                model.CreatedAt = DateTime.Now;
                db.Appointments.Add(model);
                db.SaveChanges();
                TempData["Success"] = "Appointment booked successfully! ID: APT-" + model.Id.ToString("D4");
                return RedirectToAction("Index");
            }

            LoadDropdowns(model);
            ViewBag.TimeSlots = new SelectList(TimeSlots, model.TimeSlot);
            return View(model);
        }

        // ── GET: Appointment/Edit/5 ──────────────────────────────
        public ActionResult Edit(int id)
        {
            ViewBag.ActiveMenu = "Appointment";
            var apt = db.Appointments.Find(id);
            if (apt == null) { TempData["Error"] = "Appointment not found!"; return RedirectToAction("Index"); }
            LoadDropdowns(apt);
            ViewBag.TimeSlots = new SelectList(TimeSlots, apt.TimeSlot);
            return View(apt);
        }

        // ── POST: Appointment/Edit/5 ─────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(Appointment model)
        {
            ModelState.Remove("CreatedAt");
            ModelState.Remove("Patient");
            ModelState.Remove("Doctor");
            ModelState.Remove("Branch");

            if (db.Appointments.Any(a =>
                a.DoctorId == model.DoctorId &&
                a.AppointmentDate == model.AppointmentDate &&
                a.TimeSlot == model.TimeSlot &&
                a.Status != "Cancelled" &&
                a.Id != model.Id))
                ModelState.AddModelError("TimeSlot", "This doctor already has an appointment at this time slot.");

            if (ModelState.IsValid)
            {
                db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Appointment updated successfully!";
                return RedirectToAction("Index");
            }

            LoadDropdowns(model);
            ViewBag.TimeSlots = new SelectList(TimeSlots, model.TimeSlot);
            return View(model);
        }

        // ── GET: Appointment/Details/5 ───────────────────────────
        public ActionResult Details(int id)
        {
            ViewBag.ActiveMenu = "Appointment";
            var apt = db.Appointments
                .Include("Patient").Include("Doctor").Include("Branch")
                .FirstOrDefault(a => a.Id == id);
            if (apt == null) { TempData["Error"] = "Appointment not found!"; return RedirectToAction("Index"); }
            return View(apt);
        }

        // ── Update Status (AJAX friendly) ────────────────────────
        public ActionResult UpdateStatus(int id, string status)
        {
            var apt = db.Appointments.Find(id);
            if (apt == null) { TempData["Error"] = "Not found!"; return RedirectToAction("Index"); }

            string[] validStatuses = { "Pending", "Approved", "Completed", "Cancelled" };
            if (!validStatuses.Contains(status))
            { TempData["Error"] = "Invalid status!"; return RedirectToAction("Index"); }

            apt.Status = status;
            db.SaveChanges();
            TempData["Success"] = "Appointment APT-" + apt.Id.ToString("D4") + " marked as " + status + "!";
            return RedirectToAction("Index");
        }

        // ── GET: Appointment/Delete/5 ────────────────────────────
        public ActionResult Delete(int id)
        {
            var apt = db.Appointments.Find(id);
            if (apt == null) { TempData["Error"] = "Not found!"; return RedirectToAction("Index"); }
            db.Appointments.Remove(apt);
            db.SaveChanges();
            TempData["Success"] = "Appointment deleted!";
            return RedirectToAction("Index");
        }

        // ── GET Available Time Slots for doctor on a date (AJAX) ─
        public JsonResult GetBookedSlots(int doctorId, string date)
        {
            DateTime dt;
            if (!DateTime.TryParse(date, out dt))
                return Json(new string[0], JsonRequestBehavior.AllowGet);

            var booked = db.Appointments
                .Where(a => a.DoctorId == doctorId &&
                            a.AppointmentDate == dt.Date &&
                            a.Status != "Cancelled")
                .Select(a => a.TimeSlot).ToList();

            return Json(booked, JsonRequestBehavior.AllowGet);
        }

        // ── Helpers ──────────────────────────────────────────────
        private void LoadDropdowns(Appointment m = null)
        {
            ViewBag.PatientId = new SelectList(db.Patients.OrderBy(p => p.FullName).ToList(), "Id", "FullName", m?.PatientId);
            ViewBag.DoctorId = new SelectList(db.Doctors.Where(d => d.IsActive).OrderBy(d => d.FullName).ToList(), "Id", "FullName", m?.DoctorId);
            ViewBag.BranchId = new SelectList(db.Branches.Where(b => b.IsActive).OrderBy(b => b.Name).ToList(), "Id", "Name", m?.BranchId);
        }

        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}
