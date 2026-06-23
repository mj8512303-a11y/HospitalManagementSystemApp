using System;
using System.Linq;
using System.Web.Mvc;
using HospitalMS.Models;

namespace HospitalMS.Controllers
{
    [Authorize]
    public class DepartmentController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            ViewBag.ActiveMenu = "Department";
            var list = db.Departments.Include("Branch").OrderBy(d => d.Name).ToList();
            ViewBag.TotalDepts = list.Count;
            ViewBag.ActiveDepts = list.Count(d => d.IsActive);
            ViewBag.InactiveDepts = list.Count(d => !d.IsActive);
            return View(list);
        }

        public ActionResult Create()
        {
            ViewBag.ActiveMenu = "Department";
            ViewBag.BranchId = new SelectList(
                db.Branches.Where(b => b.IsActive).OrderBy(b => b.Name).ToList(), "Id", "Name");
            return View(new Department { IsActive = true });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(Department model)
        {
            if (db.Departments.Any(d => d.Name == model.Name && d.BranchId == model.BranchId))
                ModelState.AddModelError("Name", "This department already exists in selected branch.");

            if (ModelState.IsValid)
            {
                model.CreatedAt = DateTime.Now;
                db.Departments.Add(model);
                db.SaveChanges();
                TempData["Success"] = "Department \"" + model.Name + "\" added successfully!";
                return RedirectToAction("Index");
            }
            ViewBag.ActiveMenu = "Department";
            ViewBag.BranchId = new SelectList(
                db.Branches.Where(b => b.IsActive).OrderBy(b => b.Name).ToList(), "Id", "Name", model.BranchId);
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            ViewBag.ActiveMenu = "Department";
            var dept = db.Departments.Find(id);
            if (dept == null) { TempData["Error"] = "Department not found!"; return RedirectToAction("Index"); }
            ViewBag.BranchId = new SelectList(
                db.Branches.Where(b => b.IsActive).OrderBy(b => b.Name).ToList(), "Id", "Name", dept.BranchId);
            return View(dept);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(Department model)
        {
            if (db.Departments.Any(d => d.Name == model.Name && d.BranchId == model.BranchId && d.Id != model.Id))
                ModelState.AddModelError("Name", "This department already exists in selected branch.");

            if (ModelState.IsValid)
            {
                db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Department updated successfully!";
                return RedirectToAction("Index");
            }
            ViewBag.ActiveMenu = "Department";
            ViewBag.BranchId = new SelectList(
                db.Branches.Where(b => b.IsActive).OrderBy(b => b.Name).ToList(), "Id", "Name", model.BranchId);
            return View(model);
        }

        public ActionResult Details(int id)
        {
            ViewBag.ActiveMenu = "Department";
            var dept = db.Departments.Include("Branch").FirstOrDefault(d => d.Id == id);
            if (dept == null) { TempData["Error"] = "Department not found!"; return RedirectToAction("Index"); }
            ViewBag.StaffCount = db.Staffs.Count(s => s.DepartmentId == id);
            ViewBag.DoctorCount = db.Doctors.Count(d => d.DepartmentId == id);
            return View(dept);
        }

        public ActionResult Delete(int id)
        {
            var dept = db.Departments.Find(id);
            if (dept == null) { TempData["Error"] = "Department not found!"; return RedirectToAction("Index"); }
            bool hasStaff = db.Staffs.Any(s => s.DepartmentId == id);
            bool hasDoctors = db.Doctors.Any(d => d.DepartmentId == id);
            if (hasStaff || hasDoctors)
            {
                TempData["Error"] = "Cannot delete! Staff or doctors are assigned to this department.";
                return RedirectToAction("Index");
            }
            db.Departments.Remove(dept);
            db.SaveChanges();
            TempData["Success"] = "Department deleted successfully!";
            return RedirectToAction("Index");
        }

        public ActionResult ToggleStatus(int id)
        {
            var dept = db.Departments.Find(id);
            if (dept != null) { dept.IsActive = !dept.IsActive; db.SaveChanges(); TempData["Success"] = "Status updated!"; }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}
