using System;
using System.Linq;
using System.Web.Mvc;
using HospitalMS.Models;

namespace HospitalMS.Controllers
{
    [Authorize]
    public class DesignationController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            ViewBag.ActiveMenu = "Designation";
            var list = db.Designations.OrderBy(d => d.Name).ToList();
            ViewBag.TotalDesig = list.Count;
            ViewBag.ActiveDesig = list.Count(d => d.IsActive);
            ViewBag.InactiveDesig = list.Count(d => !d.IsActive);
            return View(list);
        }

        public ActionResult Create()
        {
            ViewBag.ActiveMenu = "Designation";
            return View(new Designation { IsActive = true });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(Designation model)
        {
            if (db.Designations.Any(d => d.Name == model.Name))
                ModelState.AddModelError("Name", "This designation already exists.");

            if (ModelState.IsValid)
            {
                model.CreatedAt = DateTime.Now;
                db.Designations.Add(model);
                db.SaveChanges();
                TempData["Success"] = "Designation \"" + model.Name + "\" added successfully!";
                return RedirectToAction("Index");
            }
            ViewBag.ActiveMenu = "Designation";
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            ViewBag.ActiveMenu = "Designation";
            var desig = db.Designations.Find(id);
            if (desig == null) { TempData["Error"] = "Designation not found!"; return RedirectToAction("Index"); }
            return View(desig);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(Designation model)
        {
            if (db.Designations.Any(d => d.Name == model.Name && d.Id != model.Id))
                ModelState.AddModelError("Name", "This designation already exists.");

            if (ModelState.IsValid)
            {
                db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Designation updated successfully!";
                return RedirectToAction("Index");
            }
            ViewBag.ActiveMenu = "Designation";
            return View(model);
        }

        public ActionResult Details(int id)
        {
            ViewBag.ActiveMenu = "Designation";
            var desig = db.Designations.Find(id);
            if (desig == null) { TempData["Error"] = "Designation not found!"; return RedirectToAction("Index"); }
            ViewBag.StaffCount = db.Staffs.Count(s => s.DesignationId == id);
            return View(desig);
        }

        public ActionResult Delete(int id)
        {
            var desig = db.Designations.Find(id);
            if (desig == null) { TempData["Error"] = "Designation not found!"; return RedirectToAction("Index"); }
            if (db.Staffs.Any(s => s.DesignationId == id))
            {
                TempData["Error"] = "Cannot delete! Staff members are assigned to this designation.";
                return RedirectToAction("Index");
            }
            db.Designations.Remove(desig);
            db.SaveChanges();
            TempData["Success"] = "Designation deleted successfully!";
            return RedirectToAction("Index");
        }

        public ActionResult ToggleStatus(int id)
        {
            var desig = db.Designations.Find(id);
            if (desig != null) { desig.IsActive = !desig.IsActive; db.SaveChanges(); TempData["Success"] = "Status updated!"; }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}
