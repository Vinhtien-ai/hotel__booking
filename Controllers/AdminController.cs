using System.Linq;
using System.Web.Mvc;

namespace HotelBoking.Controllers
{
    public class AdminController : Controller
    {
        private readonly DBHotelBookingEntities db = new DBHotelBookingEntities();

        private bool IsAdmin()
        {
            return Session["Role"] != null
                && Session["Role"].ToString() == "Admin";
        }

        // Admin/Index   trang cho admin
        public ActionResult Index()
        {
            if (!IsAdmin())
            {
                return new HttpStatusCodeResult(403);
            }

            
           
            ViewBag.TotalRooms = db.HotelRooms.Count();
            ViewBag.TotalRoomTypes = db.RoomTypes.Count();
            ViewBag.TotalCustomers = db.Customers.Count();
            ViewBag.TotalBookings = db.Bookings.Count();

            return View();
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
