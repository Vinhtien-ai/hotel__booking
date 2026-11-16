using System.Linq;
using System.Web.Mvc;

namespace HotelBoking.Controllers
{
    public class SignupController : Controller
    {
        DBHotelBookingEntities db = new DBHotelBookingEntities();

        // Kiểm tra đã đăng nhập
        private bool IsLoggedIn()
        {
            return Session["Role"] != null;
        }

        // Signup
        public ActionResult Index()
        {
            if (IsLoggedIn())
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // Signup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(Customer customer, string confirmPassword)
        {
            if (IsLoggedIn())
            {
                return RedirectToAction("Index", "Home");
            }

            // Kiểm tra dữ liệu nhập
            if (string.IsNullOrWhiteSpace(customer.NameCus))
            {
                ModelState.AddModelError(
                    "NameCus",
                    "Vui lòng nhập họ và tên."
                );
            }

            if (string.IsNullOrWhiteSpace(customer.EmailCus))
            {
                ModelState.AddModelError(
                    "EmailCus",
                    "Vui lòng nhập email."
                );
            }

            if (string.IsNullOrWhiteSpace(customer.PhoneCus))
            {
                ModelState.AddModelError(
                    "PhoneCus",
                    "Vui lòng nhập số điện thoại."
                );
            }

            if (string.IsNullOrWhiteSpace(customer.PasswordCus))
            {
                ModelState.AddModelError(
                    "PasswordCus",
                    "Vui lòng nhập mật khẩu."
                );
            }

            // Kiểm tra xác nhận mật khẩu
            if (customer.PasswordCus != confirmPassword)
            {
                ModelState.AddModelError(
                    "PasswordCus",
                    "Mật khẩu xác nhận không khớp."
                );
            }

            if (ModelState.IsValid)
            {
                string email = customer.EmailCus.Trim();

                // Kiểm tra email đã tồn tại
                bool emailExists = db.Customers
                    .Any(x => x.EmailCus == email);

                if (emailExists)
                {
                    ModelState.AddModelError(
                        "EmailCus",
                        "Email này đã được đăng ký."
                    );

                    return View(customer);
                }

                customer.EmailCus = email;

                // Lưu tài khoản Customer vào Database
                db.Customers.Add(customer);
                db.SaveChanges();

                TempData["Success"] =
                    "Đăng ký tài khoản thành công!";

                return RedirectToAction("Index", "Login");
            }

            return View(customer);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}