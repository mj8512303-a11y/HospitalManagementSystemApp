using System;
using System.Linq;
using System.Web.Mvc;
using HospitalMS.Models;

namespace HospitalMS.Controllers
{
    [Authorize]
    public class ShiftController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            ViewBag.ActiveMenu = "Shift";
            var list = db.Shifts.OrderBy(s => s.StartTime).ToList();
            ViewBag.TotalShifts = list.Count;
            ViewBag.ActiveShifts = list.Count(s => s.IsActive);
            return View(list);
        }

        public ActionResult Create()
        {
            ViewBag.ActiveMenu = "Shift";
            return View(new Shift { IsActive = true });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(Shift model)
        {
            if (db.Shifts.Any(s => s.Name == model.Name))
                ModelState.AddModelError("Name", "A shift with this name already exists.");
            if (model.EndTime <= model.StartTime)
                ModelState.AddModelError("EndTime", "End time must be later than start time.");

            if (ModelState.IsValid)
            {
                model.CreatedAt = DateTime.Now;
                db.Shifts.Add(model);
                db.SaveChanges();
                TempData["Success"] = "Shift \"" + model.Name + "\" added successfully!";
                return RedirectToAction("Index");
            }
            ViewBag.ActiveMenu = "Shift";
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            ViewBag.ActiveMenu = "Shift";
            var shift = db.Shifts.Find(id);
            if (shift == null) { TempData["Error"] = "Shift not found!"; return RedirectToAction("Index"); }
            return View(shift);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(Shift model)
        {
            if (db.Shifts.Any(s => s.Name == model.Name && s.Id != model.Id))
                ModelState.AddModelError("Name", "A shift with this name already exists.");
            if (model.EndTime <= model.StartTime)
                ModelState.AddModelError("EndTime", "End time must be later than start time.");

            if (ModelState.IsValid)
            {
                db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Shift updated successfully!";
                return RedirectToAction("Index");
            }
            ViewBag.ActiveMenu = "Shift";
            return View(model);
        }

        public ActionResult Delete(int id)
        {
            var shift = db.Shifts.Find(id);
            if (shift == null) { TempData["Error"] = "Shift not found!"; return RedirectToAction("Index"); }
            if (db.Staffs.Any(s => s.ShiftId == id))
            {
                TempData["Error"] = "Cannot delete! Staff members are assigned to this shift.";
                return RedirectToAction("Index");
            }
            db.Shifts.Remove(shift);
            db.SaveChanges();
            TempData["Success"] = "Shift deleted successfully!";
            return RedirectToAction("Index");
        }

        public ActionResult ToggleStatus(int id)
        {
            var shift = db.Shifts.Find(id);
            if (shift != null) { shift.IsActive = !shift.IsActive; db.SaveChanges(); TempData["Success"] = "Status updated!"; }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}
