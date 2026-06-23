using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using HospitalMS.Models;

namespace HospitalMS.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [AllowAnonymous]
        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password)
        {
            //string hash = HashPassword(password);
            //var user = db.AppUsers.FirstOrDefault(u => u.Username == username && u.PasswordHash == hash && u.IsActive);
            var user = db.AppUsers
    .FirstOrDefault(u =>
        u.Username == username &&
        u.PasswordHash == password &&
        u.IsActive);

            if (user != null)
            {
                user.LastLogin = DateTime.Now;
                db.SaveChanges();

                FormsAuthentication.SetAuthCookie(username, false);
                Session["UserName"] = user.FullName;
                Session["UserRole"] = user.Role;
                Session["UserId"] = user.Id;
                Session["BranchId"] = user.BranchId;

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid username or password!";
            return View();
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Login");
        }

        // First-time setup: Create default SuperAdmin
        [AllowAnonymous]
        public ActionResult Setup()
        {
            if (db.AppUsers.Any()) return RedirectToAction("Login");

            var admin = new AppUser
            {
                FullName = "Super Admin",
                Username = "admin",
                Email = "admin@hospital.com",
                PasswordHash = HashPassword("admin123"),
                Role = "SuperAdmin",
                IsActive = true
            };
            db.AppUsers.Add(admin);
            db.SaveChanges();
            TempData["Success"] = "Default admin created! Username: admin | Password: admin123";
            return RedirectToAction("Login");
        }

        private string HashPassword(string password)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
