using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HospitalMS.Models;

namespace HospitalMS.Controllers
{
    [Authorize]
    public class StaffController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index(string search = "", int branchId = 0, int deptId = 0, string status = "")
        {
            ViewBag.ActiveMenu = "Staff";

            var query = db.Staffs
                .Include("Branch").Include("Department")
                .Include("Designation").Include("Shift")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(s =>
                    s.FullName.Contains(search) ||
                    s.CNIC.Contains(search) ||
                    s.Phone.Contains(search) ||
                    s.Email.Contains(search));

            if (branchId > 0) query = query.Where(s => s.BranchId == branchId);
            if (deptId > 0) query = query.Where(s => s.DepartmentId == deptId);
            if (!string.IsNullOrEmpty(status)) query = query.Where(s => s.Status == status);

            ViewBag.BranchList = new SelectList(db.Branches.Where(b => b.IsActive).OrderBy(b => b.Name).ToList(), "Id", "Name", branchId);
            ViewBag.DeptList = new SelectList(db.Departments.Where(d => d.IsActive).OrderBy(d => d.Name).ToList(), "Id", "Name", deptId);
            ViewBag.SearchVal = search;
            ViewBag.BranchIdVal = branchId;
            ViewBag.DeptIdVal = deptId;
            ViewBag.StatusVal = status;

            ViewBag.TotalStaff = db.Staffs.Count();
            ViewBag.ActiveStaff = db.Staffs.Count(s => s.Status == "Active");
            ViewBag.OnLeave = db.Staffs.Count(s => s.Status == "OnLeave");
            ViewBag.Terminated = db.Staffs.Count(s => s.Status == "Terminated");

            return View(query.OrderBy(s => s.FullName).ToList());
        }

        public ActionResult Create()
        {
            ViewBag.ActiveMenu = "Staff";
            LoadDropdowns();
            return View(new Staff { JoinDate = DateTime.Today, Status = "Active" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(Staff model, HttpPostedFileBase photoFile, HttpPostedFileBase docFile)
        {
            if (db.Staffs.Any(s => s.CNIC == model.CNIC))
                ModelState.AddModelError("CNIC", "A staff member with this CNIC already exists.");

            if (ModelState.IsValid)
            {
                model.PhotoPath = SaveFile(photoFile, "~/Uploads/Staff/", new[] { ".jpg", ".jpeg", ".png" }, "photo");
                model.DocumentPath = SaveFile(docFile, "~/Uploads/Docs/", new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" }, "doc");
                model.CreatedAt = DateTime.Now;

                db.Staffs.Add(model);
                db.SaveChanges();
                TempData["Success"] = "Staff member \"" + model.FullName + "\" added successfully!";
                return RedirectToAction("Index");
            }
            ViewBag.ActiveMenu = "Staff";
            LoadDropdowns(model);
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            ViewBag.ActiveMenu = "Staff";
            var staff = db.Staffs.Find(id);
            if (staff == null) { TempData["Error"] = "Staff not found!"; return RedirectToAction("Index"); }
            LoadDropdowns(staff);
            return View(staff);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(Staff model, HttpPostedFileBase photoFile, HttpPostedFileBase docFile)
        {
            if (db.Staffs.Any(s => s.CNIC == model.CNIC && s.Id != model.Id))
                ModelState.AddModelError("CNIC", "Another staff member with this CNIC already exists.");

            if (ModelState.IsValid)
            {
                var newPhoto = SaveFile(photoFile, "~/Uploads/Staff/", new[] { ".jpg", ".jpeg", ".png" }, "photo");
                if (!string.IsNullOrEmpty(newPhoto)) model.PhotoPath = newPhoto;

                var newDoc = SaveFile(docFile, "~/Uploads/Docs/", new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" }, "doc");
                if (!string.IsNullOrEmpty(newDoc)) model.DocumentPath = newDoc;

                db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Staff member updated successfully!";
                return RedirectToAction("Index");
            }
            ViewBag.ActiveMenu = "Staff";
            LoadDropdowns(model);
            return View(model);
        }

        public ActionResult Details(int id)
        {
            ViewBag.ActiveMenu = "Staff";
            var staff = db.Staffs
                .Include("Branch").Include("Department")
                .Include("Designation").Include("Shift")
                .FirstOrDefault(s => s.Id == id);
            if (staff == null) { TempData["Error"] = "Staff not found!"; return RedirectToAction("Index"); }
            return View(staff);
        }

        public ActionResult Delete(int id)
        {
            var staff = db.Staffs.Find(id);
            if (staff == null) { TempData["Error"] = "Staff not found!"; return RedirectToAction("Index"); }
            db.Staffs.Remove(staff);
            db.SaveChanges();
            TempData["Success"] = "Staff member deleted successfully!";
            return RedirectToAction("Index");
        }

        public ActionResult ToggleStatus(int id)
        {
            var staff = db.Staffs.Find(id);
            if (staff != null)
            {
                staff.Status = staff.Status == "Active" ? "OnLeave" : "Active";
                db.SaveChanges();
                TempData["Success"] = "Staff status updated!";
            }
            return RedirectToAction("Index");
        }

        // ?? helpers ??????????????????????????????????????????????
        private string SaveFile(HttpPostedFileBase file, string folder, string[] allowed, string prefix)
        {
            if (file == null || file.ContentLength == 0) return null;
            string ext = Path.GetExtension(file.FileName).ToLower();
            if (!Array.Exists(allowed, e => e == ext)) return null;
            string dir = Server.MapPath(folder);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            string name = prefix + "_" + Guid.NewGuid().ToString("N") + ext;
            file.SaveAs(Path.Combine(dir, name));
            return folder.Replace("~", "") + name;
        }

        private void LoadDropdowns(Staff m = null)
        {
            ViewBag.BranchId = new SelectList(db.Branches.Where(b => b.IsActive).OrderBy(b => b.Name).ToList(), "Id", "Name", m?.BranchId);
            ViewBag.DepartmentId = new SelectList(db.Departments.Where(d => d.IsActive).OrderBy(d => d.Name).ToList(), "Id", "Name", m?.DepartmentId);
            ViewBag.DesignationId = new SelectList(db.Designations.Where(d => d.IsActive).OrderBy(d => d.Name).ToList(), "Id", "Name", m?.DesignationId);
            ViewBag.ShiftId = new SelectList(db.Shifts.Where(s => s.IsActive).OrderBy(s => s.Name).ToList(), "Id", "Name", m?.ShiftId);
        }

        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}
