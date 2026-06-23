//using System.Linq;
//using System.Web.Mvc;
//using HospitalMS.Models;

//namespace HospitalMS.Controllers
//{
//    [Authorize]
//    public class BranchController : Controller
//    {
//        private ApplicationDbContext db = new ApplicationDbContext();

//        public ActionResult Index()
//        {
//            var branches = db.Branches.Include("Hospital").ToList();
//            return View(branches);
//        }

//        public ActionResult Create()
//        {
//            ViewBag.HospitalId = new SelectList(db.Hospitals.Where(h => h.IsActive), "Id", "Name");
//            return View();
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public ActionResult Create(Branch model)
//        {
//            if (ModelState.IsValid)
//            {
//                db.Branches.Add(model);
//                db.SaveChanges();
//                TempData["Success"] = "Branch added successfully!";
//                return RedirectToAction("Index");
//            }
//            ViewBag.HospitalId = new SelectList(db.Hospitals.Where(h => h.IsActive), "Id", "Name", model.HospitalId);
//            return View(model);
//        }

//        public ActionResult Edit(int id)
//        {
//            var branch = db.Branches.Find(id);
//            if (branch == null) return HttpNotFound();
//            ViewBag.HospitalId = new SelectList(db.Hospitals.Where(h => h.IsActive), "Id", "Name", branch.HospitalId);
//            return View(branch);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public ActionResult Edit(Branch model)
//        {
//            if (ModelState.IsValid)
//            {
//                db.Entry(model).State = System.Data.Entity.EntityState.Modified;
//                db.SaveChanges();
//                TempData["Success"] = "Branch updated successfully!";
//                return RedirectToAction("Index");
//            }
//            ViewBag.HospitalId = new SelectList(db.Hospitals.Where(h => h.IsActive), "Id", "Name", model.HospitalId);
//            return View(model);
//        }

//        public ActionResult Details(int id)
//        {
//            var branch = db.Branches.Include("Hospital").FirstOrDefault(b => b.Id == id);
//            if (branch == null) return HttpNotFound();
//            return View(branch);
//        }

//        public ActionResult Delete(int id)
//        {
//            var branch = db.Branches.Find(id);
//            if (branch == null) return HttpNotFound();
//            db.Branches.Remove(branch);
//            db.SaveChanges();
//            TempData["Success"] = "Branch deleted successfully!";
//            return RedirectToAction("Index");
//        }

//        protected override void Dispose(bool disposing)
//        {
//            if (disposing) db.Dispose();
//            base.Dispose(disposing);
//        }
//    }
//}

using System;
using System.Linq;
using System.Data.Entity;
using System.Web.Mvc;
using HospitalMS.Models;

namespace HospitalMS.Controllers
{
    [Authorize]
    public class BranchController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Branch/Index
        public ActionResult Index()
        {
            ViewBag.ActiveMenu = "Branch";
            //var branches = db.Branches.Include("Hospital").ToList();
            var branches = db.Branches.Include(b => b.Hospital).ToList();
            return View(branches);
        }

        // GET: Branch/Create
        public ActionResult Create()
        {
            ViewBag.ActiveMenu = "Branch";
            ViewBag.HospitalId = new SelectList(
                db.Hospitals.Where(h => h.IsActive).ToList(), "Id", "Name");
            return View();
        }

        // POST: Branch/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Branch model)
        {
            if (ModelState.IsValid)
            {
                db.Branches.Add(model);
                db.SaveChanges();
                TempData["Success"] = "Branch successfully added!";
                return RedirectToAction("Index");
            }
            ViewBag.ActiveMenu = "Branch";
            ViewBag.HospitalId = new SelectList(
                db.Hospitals.Where(h => h.IsActive).ToList(), "Id", "Name", model.HospitalId);
            return View(model);
        }

        // GET: Branch/Edit/5
        public ActionResult Edit(int id)
        {
            ViewBag.ActiveMenu = "Branch";
            var branch = db.Branches.Find(id);
            if (branch == null) return HttpNotFound();
            ViewBag.HospitalId = new SelectList(
                db.Hospitals.Where(h => h.IsActive).ToList(), "Id", "Name", branch.HospitalId);
            return View(branch);
        }

        // POST: Branch/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Branch model)
        {
            if (ModelState.IsValid)
            {
                db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Branch successfully updated!";
                return RedirectToAction("Index");
            }
            ViewBag.ActiveMenu = "Branch";
            ViewBag.HospitalId = new SelectList(
                db.Hospitals.Where(h => h.IsActive).ToList(), "Id", "Name", model.HospitalId);
            return View(model);
        }

        // GET: Branch/Details/5
        public ActionResult Details(int id)
        {
            ViewBag.ActiveMenu = "Branch";
            var branch = db.Branches
                .Include("Hospital")
                .Include("Departments")
                .FirstOrDefault(b => b.Id == id);
            if (branch == null) return HttpNotFound();

            ViewBag.TotalDepartments = db.Departments.Count(d => d.BranchId == id);
            ViewBag.TotalStaff = db.Staffs.Count(s => s.BranchId == id);
            ViewBag.TotalDoctors = db.Doctors.Count(d => d.BranchId == id);
            ViewBag.TotalPatients = db.Patients.Count(p => p.BranchId == id);
            return View(branch);
        }

        // GET: Branch/Delete/5
        public ActionResult Delete(int id)
        {
            var branch = db.Branches.Find(id);
            if (branch == null)
            {
                TempData["Error"] = "Branch not found!";
                return RedirectToAction("Index");
            }
            db.Branches.Remove(branch);
            db.SaveChanges();
            TempData["Success"] = "Branch deleted successfully!";
            return RedirectToAction("Index");
        }

        // Toggle Active/Inactive
        public ActionResult ToggleStatus(int id)
        {
            var branch = db.Branches.Find(id);
            if (branch != null)
            {
                branch.IsActive = !branch.IsActive;
                db.SaveChanges();
                TempData["Success"] = "Branch status updated!";
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
