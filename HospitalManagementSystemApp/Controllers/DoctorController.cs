using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HospitalMS.Models;

namespace HospitalMS.Controllers
{
    [Authorize]
    public class DoctorController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // ── GET: Doctor/Index ────────────────────────────────────
        public ActionResult Index(string search = "", int branchId = 0, int deptId = 0)
        {
            ViewBag.ActiveMenu = "Doctor";

            var query = db.Doctors.Include("Branch").Include("Department").AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(d =>
                    d.FullName.Contains(search) ||
                    d.Specialization.Contains(search) ||
                    d.Phone.Contains(search) ||
                    d.Email.Contains(search));

            if (branchId > 0) query = query.Where(d => d.BranchId == branchId);
            if (deptId > 0) query = query.Where(d => d.DepartmentId == deptId);

            ViewBag.BranchList = new SelectList(db.Branches.Where(b => b.IsActive).OrderBy(b => b.Name).ToList(), "Id", "Name", branchId);
            ViewBag.DeptList = new SelectList(db.Departments.Where(d => d.IsActive).OrderBy(d => d.Name).ToList(), "Id", "Name", deptId);
            ViewBag.SearchVal = search;
            ViewBag.BranchIdVal = branchId;
            ViewBag.DeptIdVal = deptId;

            ViewBag.TotalDoctors = db.Doctors.Count();
            ViewBag.ActiveDoctors = db.Doctors.Count(d => d.IsActive);
            ViewBag.InActive = db.Doctors.Count(d => !d.IsActive);

            return View(query.OrderBy(d => d.FullName).ToList());
        }

        // ── GET: Doctor/Create ───────────────────────────────────
        public ActionResult Create()
        {
            ViewBag.ActiveMenu = "Doctor";
            LoadDropdowns();
            return View(new Doctor { IsActive = true, ConsultationFee = 0 });
        }

        // ── POST: Doctor/Create ──────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(Doctor model, HttpPostedFileBase photoFile, string[] availableDays)
        {
            ModelState.Remove("PhotoPath");
            ModelState.Remove("CreatedAt");

            if (!string.IsNullOrEmpty(model.Email) && db.Doctors.Any(d => d.Email == model.Email))
                ModelState.AddModelError("Email", "A doctor with this email already exists.");

            model.AvailableDays = (availableDays != null && availableDays.Length > 0)
                ? string.Join(", ", availableDays) : "";

            if (ModelState.IsValid)
            {
                model.PhotoPath = SavePhoto(photoFile);
                model.CreatedAt = DateTime.Now;
                db.Doctors.Add(model);
                db.SaveChanges();
                TempData["Success"] = "Dr. " + model.FullName + " added successfully!";
                return RedirectToAction("Index");
            }

            LoadDropdowns(model);
            ViewBag.SelectedDays = availableDays ?? new string[0];
            return View(model);
        }

        // ── GET: Doctor/Edit/5 ───────────────────────────────────
        public ActionResult Edit(int id)
        {
            ViewBag.ActiveMenu = "Doctor";
            var doctor = db.Doctors.Find(id);
            if (doctor == null) { TempData["Error"] = "Doctor not found!"; return RedirectToAction("Index"); }

            LoadDropdowns(doctor);
            ViewBag.SelectedDays = string.IsNullOrEmpty(doctor.AvailableDays)
                ? new string[0]
                : doctor.AvailableDays.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            return View(doctor);
        }

        // ── POST: Doctor/Edit/5 ──────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(Doctor model, HttpPostedFileBase photoFile, string[] availableDays)
        {
            ModelState.Remove("PhotoPath");
            ModelState.Remove("CreatedAt");

            if (!string.IsNullOrEmpty(model.Email) && db.Doctors.Any(d => d.Email == model.Email && d.Id != model.Id))
                ModelState.AddModelError("Email", "Another doctor with this email already exists.");

            model.AvailableDays = (availableDays != null && availableDays.Length > 0)
                ? string.Join(", ", availableDays) : "";

            if (ModelState.IsValid)
            {
                var newPhoto = SavePhoto(photoFile);
                if (!string.IsNullOrEmpty(newPhoto)) model.PhotoPath = newPhoto;

                db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Dr. " + model.FullName + " updated successfully!";
                return RedirectToAction("Index");
            }

            LoadDropdowns(model);
            ViewBag.SelectedDays = availableDays ?? new string[0];
            return View(model);
        }

        // ── GET: Doctor/Details/5 ────────────────────────────────
        public ActionResult Details(int id)
        {
            ViewBag.ActiveMenu = "Doctor";
            var doctor = db.Doctors.Include("Branch").Include("Department").FirstOrDefault(d => d.Id == id);
            if (doctor == null) { TempData["Error"] = "Doctor not found!"; return RedirectToAction("Index"); }

            ViewBag.TotalAppointments = db.Appointments.Count(a => a.DoctorId == id);
            ViewBag.TodayAppointments = db.Appointments.Count(a => a.DoctorId == id && a.AppointmentDate == DateTime.Today);
            ViewBag.CompletedAppointments = db.Appointments.Count(a => a.DoctorId == id && a.Status == "Completed");
            ViewBag.PendingAppointments = db.Appointments.Count(a => a.DoctorId == id && a.Status == "Pending");
            return View(doctor);
        }

        // ── GET: Doctor/Delete/5 ─────────────────────────────────
        public ActionResult Delete(int id)
        {
            var doctor = db.Doctors.Find(id);
            if (doctor == null) { TempData["Error"] = "Doctor not found!"; return RedirectToAction("Index"); }

            if (db.Appointments.Any(a => a.DoctorId == id))
            { TempData["Error"] = "Cannot delete! Dr. " + doctor.FullName + " has existing appointments."; return RedirectToAction("Index"); }

            db.Doctors.Remove(doctor);
            db.SaveChanges();
            TempData["Success"] = "Dr. " + doctor.FullName + " deleted successfully!";
            return RedirectToAction("Index");
        }

        // ── Toggle Active/Inactive ───────────────────────────────
        public ActionResult ToggleStatus(int id)
        {
            var d = db.Doctors.Find(id);
            if (d != null) { d.IsActive = !d.IsActive; db.SaveChanges(); TempData["Success"] = "Status updated!"; }
            return RedirectToAction("Index");
        }

        // ── Helpers ──────────────────────────────────────────────
        private string SavePhoto(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0) return null;
            string ext = Path.GetExtension(file.FileName).ToLower();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png") return null;
            string dir = Server.MapPath("~/Uploads/Doctors/");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            string fn = "doc_" + Guid.NewGuid().ToString("N") + ext;
            file.SaveAs(Path.Combine(dir, fn));
            return "/Uploads/Doctors/" + fn;
        }

        private void LoadDropdowns(Doctor m = null)
        {
            ViewBag.BranchId = new SelectList(db.Branches.Where(b => b.IsActive).OrderBy(b => b.Name).ToList(), "Id", "Name", m?.BranchId);
            ViewBag.DepartmentId = new SelectList(db.Departments.Where(d => d.IsActive).OrderBy(d => d.Name).ToList(), "Id", "Name", m?.DepartmentId);
        }

        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}
