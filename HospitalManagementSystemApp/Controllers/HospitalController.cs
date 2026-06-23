//using System;
//using System.IO;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;
//using HospitalMS.Models;

//namespace HospitalMS.Controllers
//{
//    [Authorize(Roles = "SuperAdmin")]
//    public class HospitalController : Controller
//    {
//        private ApplicationDbContext db = new ApplicationDbContext();

//        public ActionResult Index()
//        {
//            var hospitals = db.Hospitals.ToList();
//            return View(hospitals);
//        }

//        public ActionResult Create()
//        {
//            return View();
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public ActionResult Create(Hospital model, HttpPostedFileBase logoFile)
//        {
//            if (ModelState.IsValid)
//            {
//                if (logoFile != null && logoFile.ContentLength > 0)
//                {
//                    string fileName = Path.GetFileName(logoFile.FileName);
//                    string path = Path.Combine(Server.MapPath("~/Uploads/Logos"), fileName);
//                    logoFile.SaveAs(path);
//                    model.LogoPath = "/Uploads/Logos/" + fileName;
//                }
//                db.Hospitals.Add(model);
//                db.SaveChanges();
//                TempData["Success"] = "Hospital added successfully!";
//                return RedirectToAction("Index");
//            }
//            return View(model);
//        }

//        public ActionResult Edit(int id)
//        {
//            var hospital = db.Hospitals.Find(id);
//            if (hospital == null) return HttpNotFound();
//            return View(hospital);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public ActionResult Edit(Hospital model, HttpPostedFileBase logoFile)
//        {
//            if (ModelState.IsValid)
//            {
//                if (logoFile != null && logoFile.ContentLength > 0)
//                {
//                    string fileName = Path.GetFileName(logoFile.FileName);
//                    string path = Path.Combine(Server.MapPath("~/Uploads/Logos"), fileName);
//                    logoFile.SaveAs(path);
//                    model.LogoPath = "/Uploads/Logos/" + fileName;
//                }
//                db.Entry(model).State = System.Data.Entity.EntityState.Modified;
//                db.SaveChanges();
//                TempData["Success"] = "Hospital updated successfully!";
//                return RedirectToAction("Index");
//            }
//            return View(model);
//        }

//        public ActionResult Details(int id)
//        {
//            var hospital = db.Hospitals.Find(id);
//            if (hospital == null) return HttpNotFound();
//            return View(hospital);
//        }

//        public ActionResult Delete(int id)
//        {
//            var hospital = db.Hospitals.Find(id);
//            if (hospital == null) return HttpNotFound();
//            db.Hospitals.Remove(hospital);
//            db.SaveChanges();
//            TempData["Success"] = "Hospital deleted successfully!";
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
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HospitalMS.Models;

namespace HospitalMS.Controllers
{
    [Authorize]
    public class HospitalController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            ViewBag.ActiveMenu = "Hospital";
            return View(db.Hospitals.ToList());
        }

        public ActionResult Create()
        {
            ViewBag.ActiveMenu = "Hospital";
            return View(new Hospital { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Hospital model, HttpPostedFileBase logoFile)
        {
            // ?? Manually clear fields that are NOT from form ??
            ModelState.Remove("LogoPath");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("Branches");

            // ?? Duplicate check ??
            if (db.Hospitals.Any(h => h.Name.Trim() == model.Name.Trim()))
                ModelState.AddModelError("Name", "Hospital with this name already exists.");

            if (ModelState.IsValid)
            {
                // Logo upload
                if (logoFile != null && logoFile.ContentLength > 0)
                {
                    string ext = Path.GetExtension(logoFile.FileName).ToLower();
                    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png")
                    {
                        string dir = Server.MapPath("~/Uploads/Logos/");
                        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                        string fn = "logo_" + Guid.NewGuid().ToString("N") + ext;
                        logoFile.SaveAs(Path.Combine(dir, fn));
                        model.LogoPath = "/Uploads/Logos/" + fn;
                    }
                }

                model.CreatedAt = DateTime.Now;
                db.Hospitals.Add(model);
                db.SaveChanges();
                TempData["Success"] = "Hospital \"" + model.Name + "\" added successfully!";
                return RedirectToAction("Index");
            }

            // ?? Show errors in view ??
            ViewBag.ActiveMenu = "Hospital";
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            ViewBag.ActiveMenu = "Hospital";
            var h = db.Hospitals.Find(id);
            if (h == null) { TempData["Error"] = "Hospital not found!"; return RedirectToAction("Index"); }
            return View(h);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Hospital model, HttpPostedFileBase logoFile)
        {
            ModelState.Remove("LogoPath");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("Branches");

            if (db.Hospitals.Any(h => h.Name.Trim() == model.Name.Trim() && h.Id != model.Id))
                ModelState.AddModelError("Name", "Another hospital with this name already exists.");

            if (ModelState.IsValid)
            {
                if (logoFile != null && logoFile.ContentLength > 0)
                {
                    string ext = Path.GetExtension(logoFile.FileName).ToLower();
                    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png")
                    {
                        string dir = Server.MapPath("~/Uploads/Logos/");
                        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                        string fn = "logo_" + Guid.NewGuid().ToString("N") + ext;
                        logoFile.SaveAs(Path.Combine(dir, fn));
                        model.LogoPath = "/Uploads/Logos/" + fn;
                    }
                }

                db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Hospital updated successfully!";
                return RedirectToAction("Index");
            }

            ViewBag.ActiveMenu = "Hospital";
            return View(model);
        }

        public ActionResult Details(int id)
        {
            ViewBag.ActiveMenu = "Hospital";
            var h = db.Hospitals.Include("Branches").FirstOrDefault(x => x.Id == id);
            if (h == null) { TempData["Error"] = "Hospital not found!"; return RedirectToAction("Index"); }
            ViewBag.TotalBranches = db.Branches.Count(b => b.HospitalId == id);
            ViewBag.TotalDoctors = db.Doctors.Count(d => d.Branch.HospitalId == id);
            ViewBag.TotalStaff = db.Staffs.Count(s => s.Branch.HospitalId == id);
            ViewBag.TotalPatients = db.Patients.Count(p => p.Branch.HospitalId == id);
            return View(h);
        }

        public ActionResult Delete(int id)
        {
            var h = db.Hospitals.Find(id);
            if (h == null) { TempData["Error"] = "Not found!"; return RedirectToAction("Index"); }
            if (db.Branches.Any(b => b.HospitalId == id))
            { TempData["Error"] = "Cannot delete! Branches are linked to this hospital."; return RedirectToAction("Index"); }
            db.Hospitals.Remove(h);
            db.SaveChanges();
            TempData["Success"] = "Hospital deleted!";
            return RedirectToAction("Index");
        }

        public ActionResult ToggleStatus(int id)
        {
            var h = db.Hospitals.Find(id);
            if (h != null) { h.IsActive = !h.IsActive; db.SaveChanges(); TempData["Success"] = "Status updated!"; }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}
