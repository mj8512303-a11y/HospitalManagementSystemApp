
using System;
using System.Linq;
using System.Web.Mvc;
using HospitalMS.Models;

namespace HospitalMS.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            ViewBag.ActiveMenu = "Dashboard";

            // ?? Core Counts ??????????????????????????????????????
            ViewBag.TotalBranches = db.Branches.Count(b => b.IsActive);
            ViewBag.TotalDepartments = db.Departments.Count(d => d.IsActive);
            ViewBag.TotalDoctors = db.Doctors.Count(d => d.IsActive);
            ViewBag.TotalStaff = db.Staffs.Count(s => s.Status == "Active");
            ViewBag.TotalPatients = db.Patients.Count();

            // ?? Appointments ?????????????????????????????????????
            ViewBag.TodayAppointments = db.Appointments.Count(a => a.AppointmentDate == DateTime.Today);
            ViewBag.PendingAppointments = db.Appointments.Count(a => a.Status == "Pending");
            ViewBag.CompletedAppointments = db.Appointments.Count(a => a.Status == "Completed");

            // ?? Attendance Today ?????????????????????????????????
            var todayAtt = db.Attendances.Where(a => a.Date == DateTime.Today).ToList();
            ViewBag.PresentToday = todayAtt.Count(a => a.Status == "Present" || a.Status == "Late");
            ViewBag.AbsentToday = todayAtt.Count(a => a.Status == "Absent");
            ViewBag.LeaveToday = todayAtt.Count(a => a.Status == "Leave");

            // ?? Salary / Payroll (current month) ?????????????????
            int curMonth = DateTime.Today.Month, curYear = DateTime.Today.Year;
            var salaries = db.Salaries.Where(s => s.Month == curMonth && s.Year == curYear).ToList();
            ViewBag.TotalPayroll = salaries.Sum(s => (decimal?)s.NetSalary) ?? 0;
            ViewBag.PaidPayroll = salaries.Where(s => s.IsPaid).Sum(s => (decimal?)s.NetSalary) ?? 0;
            ViewBag.UnpaidPayroll = ViewBag.TotalPayroll - ViewBag.PaidPayroll;
            ViewBag.SalaryMonthName = new DateTime(curYear, curMonth, 1).ToString("MMMM yyyy");

            // ?? Today's appointment list ??????????????????????????
            ViewBag.TodayAppointmentList = db.Appointments
                .Include("Patient").Include("Doctor")
                .Where(a => a.AppointmentDate == DateTime.Today)
                .OrderBy(a => a.TimeSlot).Take(6).ToList();

            // ?? Recent Patients ???????????????????????????????????
            ViewBag.RecentPatients = db.Patients
                .OrderByDescending(p => p.CreatedAt).Take(5).ToList();

            // ?? Hospital Info ?????????????????????????????????????
            ViewBag.HospitalInfo = db.Hospitals.FirstOrDefault(h => h.IsActive);

            // ?? Gender distribution for chart ?????????????????????
            ViewBag.MalePatients = db.Patients.Count(p => p.Gender == "Male");
            ViewBag.FemalePatients = db.Patients.Count(p => p.Gender == "Female");
            ViewBag.OtherPatients = db.Patients.Count(p => p.Gender == "Other");

            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
