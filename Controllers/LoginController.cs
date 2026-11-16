
using System;
using System.Linq;
using System.Web.Mvc;

namespace HotelBoking.Controllers
{
    public class LoginController : Controller
    {
        DBHotelBookingEntities db = new DBHotelBookingEntities();

        // Login
        public ActionResult Index()
        {
            return View();
        }

        // Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui lòng nhập tài khoản và mật khẩu.";
                return View();
            }

            // Kiểm tra tài khoản Admin
            var admin = db.AdminUsers
                .FirstOrDefault(x =>
                    x.NameUser == email &&
                    x.PasswordUser == password);

            if (admin != null)
            {
                Session.Clear();

                Session["IDAdmin"] = admin.ID;
                Session["NameUser"] = admin.NameUser;
                Session["Role"] = "Admin";

                return RedirectToAction("Index", "Home");
            }

            // Kiểm tra tài khoản Customer
            var customer = db.Customers
                .FirstOrDefault(x =>
                    x.EmailCus == email &&
                    x.PasswordCus == password);

            if (customer != null)
            {
                Session.Clear();

                Session["IDCus"] = customer.IDCus;
                Session["NameCus"] = customer.NameCus;
                Session["EmailCus"] = customer.EmailCus;
                Session["Role"] = "Customer";

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Tài khoản hoặc mật khẩu không chính xác.";

            return View();
        }

        // Login/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();

            return RedirectToAction("Index", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
