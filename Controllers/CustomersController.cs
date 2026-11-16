using System.Linq;
using System.Web.Mvc;

namespace HotelBoking.Controllers
{
    public class CustomersController : Controller
    {
        DBHotelBookingEntities db = new DBHotelBookingEntities();

        // Kiểm tra quyền Admin
        private bool IsAdmin()
        {
            return Session["Role"] != null
                && Session["Role"].ToString() == "Admin";
        }

        // Customers
        public ActionResult Index()
        {
            if (!IsAdmin())
            {
                return new HttpStatusCodeResult(403);
            }

            return View(db.Customers.ToList());
        }

        //  Customers/Details/5
        public ActionResult Details(int? id)
        {
            if (!IsAdmin())
            {
                return new HttpStatusCodeResult(403);
            }

            if (id == null)
            {
                return HttpNotFound();
            }

            Customer customer = db.Customers
                .FirstOrDefault(x => x.IDCus == id);

            if (customer == null)
            {
                return HttpNotFound();
            }

            return View(customer);
        }

        // ustomers/Create
        public ActionResult Create()
        {
            if (!IsAdmin())
            {
                return new HttpStatusCodeResult(403);
            }

            return View();
        }

        // Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Customer customer)
        {
            if (!IsAdmin())
            {
                return new HttpStatusCodeResult(403);
            }

            if (ModelState.IsValid)
            {
                db.Customers.Add(customer);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(customer);
        }

        // Customers/Edit/
        public ActionResult Edit(int? id)
        {
            if (!IsAdmin())
            {
                return new HttpStatusCodeResult(403);
            }

            if (id == null)
            {
                return HttpNotFound();
            }

            Customer customer = db.Customers
                .FirstOrDefault(x => x.IDCus == id);

            if (customer == null)
            {
                return HttpNotFound();
            }

            return View(customer);
        }

        //  Customers/Edit/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Customer customer)
        {
            if (!IsAdmin())
            {
                return new HttpStatusCodeResult(403);
            }

            if (ModelState.IsValid)
            {
                Customer oldCustomer = db.Customers
                    .FirstOrDefault(x => x.IDCus == customer.IDCus);

                if (oldCustomer == null)
                {
                    return HttpNotFound();
                }

                oldCustomer.NameCus = customer.NameCus;
                oldCustomer.PhoneCus = customer.PhoneCus;
                oldCustomer.EmailCus = customer.EmailCus;

                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(customer);
        }

        //  Customers/Delete/
        public ActionResult Delete(int? id)
        {
            if (!IsAdmin())
            {
                return new HttpStatusCodeResult(403);
            }

            if (id == null)
            {
                return HttpNotFound();
            }

            Customer customer = db.Customers
                .FirstOrDefault(x => x.IDCus == id);

            if (customer == null)
            {
                return HttpNotFound();
            }

            return View(customer);
        }

        //  Customers/Delete/
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!IsAdmin())
            {
                return new HttpStatusCodeResult(403);
            }

            Customer customer = db.Customers
                .FirstOrDefault(x => x.IDCus == id);

            if (customer == null)
            {
                return HttpNotFound();
            }

            bool hasBooking = db.Bookings
                .Any(x => x.IDCus == id);

            if (hasBooking)
            {
                TempData["Error"] =
                    "Không thể xóa khách hàng đã có đơn đặt phòng.";

                return RedirectToAction("Index");
            }

            db.Customers.Remove(customer);
            db.SaveChanges();

            return RedirectToAction("Index");
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