using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HospitalMS.Models;

namespace HospitalMS.Controllers
{
    [Authorize]
    public class AttendanceController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // ── GET: Attendance/Index ────────────────────────────────
        public ActionResult Index(string date = "", int branchId = 0,
                                  string empType = "", string status = "")
        {
            ViewBag.ActiveMenu = "Attendance";

            DateTime selectedDate = string.IsNullOrEmpty(date)
                ? DateTime.Today : DateTime.Parse(date);

            var query = db.Attendances.AsQueryable();
            query = query.Where(a => a.Date == selectedDate.Date);

            if (!string.IsNullOrEmpty(empType)) query = query.Where(a => a.EmployeeType == empType);
            if (!string.IsNullOrEmpty(status)) query = query.Where(a => a.Status == status);

            ViewBag.SelectedDate = selectedDate.ToString("yyyy-MM-dd");
            ViewBag.DisplayDate = selectedDate.ToString("dd MMMM yyyy");
            ViewBag.EmpTypeVal = empType;
            ViewBag.StatusVal = status;
            ViewBag.BranchList = new SelectList(db.Branches.Where(b => b.IsActive).OrderBy(b => b.Name).ToList(), "Id", "Name", branchId);

            // Stats for selected date
            var dayAttendance = query.ToList();
            ViewBag.TotalPresent = dayAttendance.Count(a => a.Status == "Present");
            ViewBag.TotalAbsent = dayAttendance.Count(a => a.Status == "Absent");
            ViewBag.TotalLate = dayAttendance.Count(a => a.Status == "Late");
            ViewBag.TotalHalfDay = dayAttendance.Count(a => a.Status == "HalfDay");
            ViewBag.TotalLeave = dayAttendance.Count(a => a.Status == "Leave");

            return View(dayAttendance.OrderBy(a => a.EmployeeType).ThenBy(a => a.EmployeeId).ToList());
        }

        // ── GET: Attendance/BiometricTerminal ────────────────────
        // Biometric Machine Style — Employee ID enter karo
        public ActionResult BiometricTerminal()
        {
            ViewBag.ActiveMenu = "Attendance";
            ViewBag.CurrentTime = DateTime.Now.ToString("hh:mm:ss tt");
            ViewBag.CurrentDate = DateTime.Now.ToString("dddd, dd MMMM yyyy");
            return View();
        }

        // ── POST: Attendance/PunchIn ─────────────────────────────
        [HttpPost]
        public JsonResult PunchIn(int employeeId, string employeeType)
        {
            try
            {
                // Find employee
                string empName = "";
                string empDesig = "";
                string empPhoto = "";

                if (employeeType == "Staff")
                {
                    var staff = db.Staffs.Include("Designation").FirstOrDefault(s => s.Id == employeeId);
                    if (staff == null) return Json(new { success = false, message = "Employee ID not found!" });
                    empName = staff.FullName;
                    empDesig = staff.Designation != null ? staff.Designation.Name : "";
                    empPhoto = staff.PhotoPath;
                }
                else if (employeeType == "Doctor")
                {
                    var doc = db.Doctors.FirstOrDefault(d => d.Id == employeeId);
                    if (doc == null) return Json(new { success = false, message = "Doctor ID not found!" });
                    empName = "Dr. " + doc.FullName;
                    empDesig = doc.Specialization;
                    empPhoto = doc.PhotoPath;
                }
                else return Json(new { success = false, message = "Invalid employee type!" });

                // Check if already punched in today
                var existing = db.Attendances.FirstOrDefault(a =>
                    a.EmployeeId == employeeId &&
                    a.EmployeeType == employeeType &&
                    a.Date == DateTime.Today);

                TimeSpan now = DateTime.Now.TimeOfDay;
                string action = "";

                if (existing == null)
                {
                    // First punch = IN
                    var att = new Attendance
                    {
                        EmployeeId = employeeId,
                        EmployeeType = employeeType,
                        Date = DateTime.Today,
                        InTime = now,
                        Status = now > new TimeSpan(9, 15, 0) ? "Late" : "Present",
                        MarkedBy = "Biometric Terminal",
                        CreatedAt = DateTime.Now
                    };
                    db.Attendances.Add(att);
                    action = "IN";
                }
                else if (existing.OutTime == null)
                {
                    // Second punch = OUT
                    existing.OutTime = now;
                    // Calculate if half day
                    TimeSpan worked = now - existing.InTime.Value;
                    if (worked.TotalHours < 4 && existing.Status != "Late")
                        existing.Status = "HalfDay";
                    action = "OUT";
                }
                else
                {
                    return Json(new { success = false, message = empName + " has already completed attendance today!" });
                }

                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    action = action,
                    name = empName,
                    desig = empDesig,
                    photo = string.IsNullOrEmpty(empPhoto) ? "" : empPhoto,
                    time = DateTime.Now.ToString("hh:mm:ss tt"),
                    message = action == "IN"
                        ? "Welcome! " + empName + " — Check IN successful"
                        : "Goodbye! " + empName + " — Check OUT successful"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // ── GET: Attendance/BulkMark ─────────────────────────────
        public ActionResult BulkMark(int branchId = 0, string empType = "Staff")
        {
            ViewBag.ActiveMenu = "Attendance";
            ViewBag.BranchList = new SelectList(db.Branches.Where(b => b.IsActive).OrderBy(b => b.Name).ToList(), "Id", "Name", branchId);
            ViewBag.EmpType = empType;
            ViewBag.Today = DateTime.Today.ToString("dd MMM yyyy");

            List<BulkAttendanceVM> employees = new List<BulkAttendanceVM>();

            if (branchId > 0)
            {
                if (empType == "Staff")
                {
                    var staffList = db.Staffs.Include("Designation")
                        .Where(s => s.BranchId == branchId && s.Status == "Active")
                        .OrderBy(s => s.FullName).ToList();

                    foreach (var s in staffList)
                    {
                        var existing = db.Attendances.FirstOrDefault(a =>
                            a.EmployeeId == s.Id && a.EmployeeType == "Staff" && a.Date == DateTime.Today);

                        employees.Add(new BulkAttendanceVM
                        {
                            EmployeeId = s.Id,
                            EmployeeType = "Staff",
                            FullName = s.FullName,
                            Designation = s.Designation != null ? s.Designation.Name : "",
                            PhotoPath = s.PhotoPath,
                            Status = existing != null ? existing.Status : "Present",
                            AlreadyMarked = existing != null
                        });
                    }
                }
                else
                {
                    var docList = db.Doctors
                        .Where(d => d.BranchId == branchId && d.IsActive)
                        .OrderBy(d => d.FullName).ToList();

                    foreach (var d in docList)
                    {
                        var existing = db.Attendances.FirstOrDefault(a =>
                            a.EmployeeId == d.Id && a.EmployeeType == "Doctor" && a.Date == DateTime.Today);

                        employees.Add(new BulkAttendanceVM
                        {
                            EmployeeId = d.Id,
                            EmployeeType = "Doctor",
                            FullName = "Dr. " + d.FullName,
                            Designation = d.Specialization,
                            PhotoPath = d.PhotoPath,
                            Status = existing != null ? existing.Status : "Present",
                            AlreadyMarked = existing != null
                        });
                    }
                }
            }

            ViewBag.Employees = employees;
            return View();
        }

        // ── POST: Attendance/BulkMark ────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult BulkMark(FormCollection form, string empType, int branchId)
        {
            int saved = 0, updated = 0;
            int i = 0;
            while (true)
            {
                string empId = form["employees[" + i + "].EmployeeId"];
                string status = form["employees[" + i + "].Status"];
                string remarks = form["employees[" + i + "].Remarks"];
                if (empId == null) break;

                int eid = int.Parse(empId);
                var existing = db.Attendances.FirstOrDefault(a =>
                    a.EmployeeId == eid && a.EmployeeType == empType && a.Date == DateTime.Today);

                if (existing != null)
                {
                    existing.Status = status;
                    existing.Remarks = remarks;
                    updated++;
                }
                else
                {
                    db.Attendances.Add(new Attendance
                    {
                        EmployeeId = eid,
                        EmployeeType = empType,
                        Date = DateTime.Today,
                        Status = status,
                        Remarks = remarks,
                        MarkedBy = Session["UserName"] != null ? Session["UserName"].ToString() : "Admin",
                        CreatedAt = DateTime.Now
                    });
                    saved++;
                }
                i++;
            }

            db.SaveChanges();
            TempData["Success"] = saved + " new attendance records saved, " + updated + " updated!";
            return RedirectToAction("Index");
        }

        // ── GET: Attendance/ImportCSV ────────────────────────────
        public ActionResult ImportCSV()
        {
            ViewBag.ActiveMenu = "Attendance";
            return View();
        }

        // ── POST: Attendance/ImportCSV ───────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ImportCSV(HttpPostedFileBase csvFile, string empType)
        {
            if (csvFile == null || csvFile.ContentLength == 0)
            { TempData["Error"] = "Please select a CSV file!"; return View(); }

            int imported = 0, skipped = 0;
            var errors = new List<string>();

            try
            {
                using (var reader = new StreamReader(csvFile.InputStream))
                {
                    string header = reader.ReadLine(); // skip header
                    string line;
                    int row = 2;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var cols = line.Split(',');
                        if (cols.Length < 4) { skipped++; row++; continue; }

                        int empId;
                        DateTime date;
                        if (!int.TryParse(cols[0].Trim(), out empId) ||
                            !DateTime.TryParse(cols[1].Trim(), out date))
                        { errors.Add("Row " + row + ": Invalid ID or Date"); skipped++; row++; continue; }

                        string status = cols[2].Trim();
                        string inT = cols.Length > 3 ? cols[3].Trim() : "";
                        string outT = cols.Length > 4 ? cols[4].Trim() : "";

                        // Skip if already exists
                        if (db.Attendances.Any(a => a.EmployeeId == empId &&
                            a.EmployeeType == empType && a.Date == date.Date))
                        { skipped++; row++; continue; }

                        TimeSpan? inTime = null;
                        TimeSpan? outTime = null;
                        TimeSpan ts;
                        if (TimeSpan.TryParse(inT, out ts)) inTime = ts;
                        if (TimeSpan.TryParse(outT, out ts)) outTime = ts;

                        db.Attendances.Add(new Attendance
                        {
                            EmployeeId = empId,
                            EmployeeType = empType,
                            Date = date.Date,
                            Status = status,
                            InTime = inTime,
                            OutTime = outTime,
                            MarkedBy = "CSV Import",
                            CreatedAt = DateTime.Now
                        });
                        imported++;
                        row++;
                    }
                }

                db.SaveChanges();
                TempData["Success"] = imported + " records imported successfully! " +
                    (skipped > 0 ? skipped + " rows skipped." : "");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Import failed: " + ex.Message;
            }

            ViewBag.Errors = errors;
            ViewBag.ActiveMenu = "Attendance";
            return View();
        }

        // ── GET: Attendance/Report ───────────────────────────────
        public ActionResult Report(int employeeId = 0, string empType = "Staff",
                                   string month = "", string year = "")
        {
            ViewBag.ActiveMenu = "Attendance";

            int m = string.IsNullOrEmpty(month) ? DateTime.Today.Month : int.Parse(month);
            int y = string.IsNullOrEmpty(year) ? DateTime.Today.Year : int.Parse(year);

            var query = db.Attendances
                .Where(a => a.Date.Month == m && a.Date.Year == y);

            if (!string.IsNullOrEmpty(empType)) query = query.Where(a => a.EmployeeType == empType);
            if (employeeId > 0) query = query.Where(a => a.EmployeeId == employeeId);

            var list = query.OrderBy(a => a.EmployeeId).ThenBy(a => a.Date).ToList();

            ViewBag.Month = m;
            ViewBag.Year = y;
            ViewBag.EmpType = empType;
            ViewBag.EmpId = employeeId;
            ViewBag.MonthName = new DateTime(y, m, 1).ToString("MMMM yyyy");
            ViewBag.TotalDays = DateTime.DaysInMonth(y, m);

            // Summary
            ViewBag.Present = list.Count(a => a.Status == "Present");
            ViewBag.Absent = list.Count(a => a.Status == "Absent");
            ViewBag.Late = list.Count(a => a.Status == "Late");
            ViewBag.HalfDay = list.Count(a => a.Status == "HalfDay");
            ViewBag.Leave = list.Count(a => a.Status == "Leave");

            // Staff list for filter
            ViewBag.StaffList = new SelectList(
                db.Staffs.Where(s => s.Status == "Active").OrderBy(s => s.FullName)
                .Select(s => new { Id = s.Id, Name = s.FullName }).ToList(),
                "Id", "Name", employeeId);

            return View(list);
        }

        // ── Delete single record ─────────────────────────────────
        public ActionResult Delete(int id)
        {
            var att = db.Attendances.Find(id);
            if (att != null) { db.Attendances.Remove(att); db.SaveChanges(); }
            TempData["Success"] = "Attendance record deleted!";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }

    // ── ViewModel for Bulk Attendance ───────────────────────────
    public class BulkAttendanceVM
    {
        public int EmployeeId { get; set; }
        public string EmployeeType { get; set; }
        public string FullName { get; set; }
        public string Designation { get; set; }
        public string PhotoPath { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
        public bool AlreadyMarked { get; set; }
    }
}
