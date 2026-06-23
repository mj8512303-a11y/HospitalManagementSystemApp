using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using HospitalMS.Models;

namespace HospitalMS.Controllers
{
    [Authorize]
    public class PharmacyController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            ViewBag.ActiveMenu = "Pharmacy";
            ViewBag.BranchList = new SelectList(
                db.Branches.Where(b => b.IsActive).OrderBy(b => b.Name).ToList(), "Id", "Name");

            DateTime today = DateTime.Today;
            DateTime after30 = today.AddDays(30);

            ViewBag.TotalMedicines = db.Medicines.Count();
            ViewBag.LowStock = db.Medicines.Count(m => m.StockQuantity <= m.MinStockAlert && m.IsActive);
            ViewBag.ExpiringSoon = db.Medicines.Count(m => m.ExpiryDate <= after30 && m.ExpiryDate >= today && m.IsActive);
            ViewBag.TotalLabTests = db.LabTests.Count();
            ViewBag.PendingTests = db.LabTests.Count(l => l.Status == "Pending");
            ViewBag.CompletedTests = db.LabTests.Count(l => l.Status == "Completed");

            return View();
        }

        [HttpGet]
        public JsonResult GetMedicines(string search = "", int branchId = 0, string category = "")
        {
            var query = db.Medicines.Include("Branch").AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(m => m.Name.Contains(search) || m.Manufacturer.Contains(search));
            if (branchId > 0) query = query.Where(m => m.BranchId == branchId);
            if (!string.IsNullOrEmpty(category)) query = query.Where(m => m.Category == category);

            DateTime today = DateTime.Today;
            DateTime after30 = today.AddDays(30);

            var list = query.OrderBy(m => m.Name).ToList().Select(m => new {
                m.Id,
                m.Name,
                m.Category,
                m.Manufacturer,
                m.StockQuantity,
                m.UnitPrice,
                m.MinStockAlert,
                m.IsActive,
                ExpiryDate = m.ExpiryDate.ToString("dd MMM yyyy"),
                ExpiryDateRaw = m.ExpiryDate.ToString("yyyy-MM-dd"),
                BranchName = m.Branch != null ? m.Branch.Name : "N/A",
                m.BranchId,
                IsLowStock = m.StockQuantity <= m.MinStockAlert,
                IsExpiringSoon = m.ExpiryDate <= after30 && m.ExpiryDate >= today,
                IsExpired = m.ExpiryDate < today
            }).ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetMedicine(int id)
        {
            var m = db.Medicines.Include("Branch").FirstOrDefault(x => x.Id == id);
            if (m == null) return Json(new { success = false }, JsonRequestBehavior.AllowGet);

            DateTime today = DateTime.Today;
            DateTime after30 = today.AddDays(30);

            return Json(new
            {
                success = true,
                m.Id,
                m.Name,
                m.Category,
                m.Manufacturer,
                m.StockQuantity,
                m.UnitPrice,
                m.MinStockAlert,
                m.IsActive,
                ExpiryDate = m.ExpiryDate.ToString("yyyy-MM-dd"),
                ExpiryDateDisp = m.ExpiryDate.ToString("dd MMM yyyy"),
                BranchName = m.Branch != null ? m.Branch.Name : "N/A",
                m.BranchId,
                IsLowStock = m.StockQuantity <= m.MinStockAlert,
                IsExpiringSoon = m.ExpiryDate <= after30 && m.ExpiryDate >= today,
                IsExpired = m.ExpiryDate < today
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveMedicine(Medicine model)
        {
            try
            {
                ModelState.Remove("Branch"); ModelState.Remove("CreatedAt");
                if (string.IsNullOrWhiteSpace(model.Name))
                    return Json(new { success = false, message = "Medicine name is required!" });
                if (model.BranchId == 0)
                    return Json(new { success = false, message = "Please select a branch!" });

                if (model.Id == 0)
                {
                    model.CreatedAt = DateTime.Now;
                    db.Medicines.Add(model);
                    db.SaveChanges();
                    return Json(new { success = true, message = "Medicine \"" + model.Name + "\" added!" });
                }
                else
                {
                    var ex = db.Medicines.Find(model.Id);
                    if (ex == null) return Json(new { success = false, message = "Not found!" });
                    ex.Name = model.Name; ex.Category = model.Category;
                    ex.Manufacturer = model.Manufacturer; ex.StockQuantity = model.StockQuantity;
                    ex.UnitPrice = model.UnitPrice; ex.ExpiryDate = model.ExpiryDate;
                    ex.MinStockAlert = model.MinStockAlert; ex.BranchId = model.BranchId;
                    ex.IsActive = model.IsActive;
                    db.SaveChanges();
                    return Json(new { success = true, message = "Medicine updated!" });
                }
            }
            catch (Exception ex) { return Json(new { success = false, message = "Error: " + ex.Message }); }
        }

        [HttpPost]
        public JsonResult DeleteMedicine(int id)
        {
            var m = db.Medicines.Find(id);
            if (m == null) return Json(new { success = false, message = "Not found!" });
            string name = m.Name;
            db.Medicines.Remove(m); db.SaveChanges();
            return Json(new { success = true, message = "\"" + name + "\" deleted!" });
        }

        [HttpGet]
        public JsonResult GetLabTests(string search = "", int branchId = 0, string status = "")
        {
            var query = db.LabTests.Include("Branch").AsQueryable();
            if (!string.IsNullOrWhiteSpace(search)) query = query.Where(l => l.TestName.Contains(search));
            if (branchId > 0) query = query.Where(l => l.BranchId == branchId);
            if (!string.IsNullOrEmpty(status)) query = query.Where(l => l.Status == status);

            var list = query.OrderByDescending(l => l.TestDate).ToList().Select(l => new {
                l.Id,
                l.PatientId,
                l.DoctorId,
                l.TestName,
                l.Charges,
                l.Result,
                l.Status,
                TestDate = l.TestDate.ToString("dd MMM yyyy"),
                BranchName = l.Branch != null ? l.Branch.Name : "N/A",
                l.BranchId
            }).ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetLabTest(int id)
        {
            var l = db.LabTests.Include("Branch").FirstOrDefault(x => x.Id == id);
            if (l == null) return Json(new { success = false }, JsonRequestBehavior.AllowGet);

            string patientName = "", doctorName = "";
            var patient = db.Patients.Find(l.PatientId);
            if (patient != null) patientName = patient.FullName + " (P-" + patient.Id.ToString("D4") + ")";
            var doctor = db.Doctors.Find(l.DoctorId);
            if (doctor != null) doctorName = "Dr. " + doctor.FullName;

            return Json(new
            {
                success = true,
                l.Id,
                l.PatientId,
                l.DoctorId,
                l.TestName,
                l.Charges,
                l.Result,
                l.Status,
                l.BranchId,
                TestDate = l.TestDate.ToString("yyyy-MM-dd"),
                TestDateDisp = l.TestDate.ToString("dd MMM yyyy"),
                BranchName = l.Branch != null ? l.Branch.Name : "N/A",
                PatientName = patientName,
                DoctorName = doctorName
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveLabTest(LabTest model)
        {
            try
            {
                ModelState.Remove("Branch"); ModelState.Remove("CreatedAt");
                if (string.IsNullOrWhiteSpace(model.TestName))
                    return Json(new { success = false, message = "Test name is required!" });

                if (model.Id == 0)
                {
                    model.CreatedAt = DateTime.Now;
                    db.LabTests.Add(model);
                    db.SaveChanges();
                    return Json(new { success = true, message = "Lab test \"" + model.TestName + "\" added!" });
                }
                else
                {
                    var ex = db.LabTests.Find(model.Id);
                    if (ex == null) return Json(new { success = false, message = "Not found!" });
                    ex.TestName = model.TestName; ex.PatientId = model.PatientId;
                    ex.DoctorId = model.DoctorId; ex.BranchId = model.BranchId;
                    ex.Charges = model.Charges; ex.Result = model.Result;
                    ex.Status = model.Status; ex.TestDate = model.TestDate;
                    db.SaveChanges();
                    return Json(new { success = true, message = "Lab test updated!" });
                }
            }
            catch (Exception ex) { return Json(new { success = false, message = "Error: " + ex.Message }); }
        }

        [HttpPost]
        public JsonResult DeleteLabTest(int id)
        {
            var l = db.LabTests.Find(id);
            if (l == null) return Json(new { success = false, message = "Not found!" });
            string name = l.TestName;
            db.LabTests.Remove(l); db.SaveChanges();
            return Json(new { success = true, message = "\"" + name + "\" deleted!" });
        }

        [HttpGet]
        public JsonResult GetPatients()
        {
            var list = db.Patients.OrderBy(p => p.FullName)
                .Select(p => new { p.Id, Name = p.FullName }).ToList()
                .Select(p => new { p.Id, Name = p.Name + " (P-" + p.Id.ToString("D4") + ")" }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetDoctors()
        {
            var list = db.Doctors.Where(d => d.IsActive).OrderBy(d => d.FullName)
                .Select(d => new { d.Id, Name = "Dr. " + d.FullName }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}