
using System.Linq;
using System.Data.Entity;
using System.Web.Mvc;

namespace HotelBoking.Controllers
{
    public class RoomController : Controller
    {
        DBHotelBookingEntities db = new DBHotelBookingEntities();

        // Room
        public ActionResult Index()
        {
            var hotelRooms = db.HotelRooms
                .Include("RoomType1")
                .ToList();

            return View(hotelRooms);
        }

        // Room/Details
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            HotelRoom hotelRoom = db.HotelRooms
                .Include("RoomType1")
                .FirstOrDefault(x => x.RoomID == id);

            if (hotelRoom == null)
            {
                return HttpNotFound();
            }

            return View(hotelRoom);
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
