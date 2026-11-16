using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;

namespace HotelBoking.Controllers
{
    public class HotelRoomsController : Controller
    {
        DBHotelBookingEntities db = new DBHotelBookingEntities();

        // Kiểm tra quyền Admin
        private bool IsAdmin()
        {
            return Session["Role"] != null &&
                   Session["Role"].ToString() == "Admin";
        }

        // HotelRooms
        // Admin và Customer đều được xem
        public ActionResult Index()
        {
            if (!IsAdmin())
            {
                return new HttpStatusCodeResult(403);
            }

            var hotelRooms = db.HotelRooms
                .Include("RoomType1")
                .ToList();

            return View(hotelRooms);
        }

        // HotelRooms/Details/5
        // Admin và Customer đều được xem
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

            HotelRoom hotelRoom = db.HotelRooms
                .Include("RoomType1")
                .FirstOrDefault(x => x.RoomID == id);

            if (hotelRoom == null)
            {
                return HttpNotFound();
            }

            return View(hotelRoom);
        }

        // HotelRooms/Create
        // Chỉ Admin
        public ActionResult Create()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.RoomType = new SelectList(
                db.RoomTypes.ToList(),
                "IDType",
                "NameType"
            );

            return View();
        }

        // HotelRooms/Create
        // Chỉ Admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(HotelRoom hotelRoom)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                db.HotelRooms.Add(hotelRoom);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.RoomType = new SelectList(
                db.RoomTypes.ToList(),
                "IDType",
                "NameType",
                hotelRoom.RoomType
            );

            return View(hotelRoom);
        }

        // HotelRooms/Edit
        // Chỉ Admin
        public ActionResult Edit(int? id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                return HttpNotFound();
            }

            HotelRoom hotelRoom = db.HotelRooms
                .FirstOrDefault(x => x.RoomID == id);

            if (hotelRoom == null)
            {
                return HttpNotFound();
            }

            ViewBag.RoomType = new SelectList(
                db.RoomTypes.ToList(),
                "IDType",
                "NameType",
                hotelRoom.RoomType
            );

            return View(hotelRoom);
        }

        //  HotelRooms/Edit/5
        // Chỉ Admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(HotelRoom hotelRoom)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                HotelRoom oldRoom = db.HotelRooms
                    .FirstOrDefault(x => x.RoomID == hotelRoom.RoomID);

                if (oldRoom == null)
                {
                    return HttpNotFound();
                }

                oldRoom.NameRoom = hotelRoom.NameRoom;
                oldRoom.DescriptionRoom = hotelRoom.DescriptionRoom;
                oldRoom.RoomType = hotelRoom.RoomType;
                oldRoom.Price = hotelRoom.Price;
                oldRoom.ImageRoom = hotelRoom.ImageRoom;

                db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.RoomType = new SelectList(
                db.RoomTypes.ToList(),
                "IDType",
                "NameType",
                hotelRoom.RoomType
            );

            return View(hotelRoom);
        }

        // HotelRooms/Delete/
        // Chỉ Admin
        public ActionResult Delete(int? id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

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

        // POST: HotelRooms/Delete/5
        // Chỉ Admin
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            HotelRoom hotelRoom = db.HotelRooms
                .FirstOrDefault(x => x.RoomID == id);

            if (hotelRoom == null)
            {
                return HttpNotFound();
            }

            bool hasBookings = db.BookingDetails.Any(x => x.IDRoom == id);
            if (hasBookings)
            {
                TempData["Error"] = "Không thể xóa phòng đã phát sinh đặt phòng.";
                return RedirectToAction("Index");
            }

            db.HotelRooms.Remove(hotelRoom);
            db.SaveChanges();

            return RedirectToAction("Index");
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}