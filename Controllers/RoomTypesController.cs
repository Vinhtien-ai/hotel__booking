using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;

namespace HotelBoking.Controllers
{
    public class RoomTypesController : Controller
    {
        DBHotelBookingEntities db = new DBHotelBookingEntities();

        // Kiểm tra quyền Admin
        private bool IsAdmin()
        {
            return Session["Role"] != null &&
                   Session["Role"].ToString() == "Admin";
        }

        //
        // Admin và Customer đều được xem
        public ActionResult Index()
        {
            if (!IsAdmin()) return new HttpStatusCodeResult(403);
            return View(db.RoomTypes.ToList());
        }

        // RoomTypes/Details/DLX
        // Admin và Customer đều được xem
        public ActionResult Details(string id)
        {
            if (!IsAdmin()) return new HttpStatusCodeResult(403);
            if (id == null)
            {
                return HttpNotFound();
            }

            RoomType roomType = db.RoomTypes
                .FirstOrDefault(x => x.IDType == id);

            if (roomType == null)
            {
                return HttpNotFound();
            }

            return View(roomType);
        }

        // RoomTypes/Edit/DLX
        // Chỉ Admin
        public ActionResult Edit(string id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                return HttpNotFound();
            }

            RoomType roomType = db.RoomTypes
                .FirstOrDefault(x => x.IDType == id);

            if (roomType == null)
            {
                return HttpNotFound();
            }

            return View(roomType);
        }

        //  RoomTypes/Edit
        // Chỉ Admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(RoomType roomType)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                RoomType oldRoomType = db.RoomTypes
                    .FirstOrDefault(x => x.IDType == roomType.IDType);

                if (oldRoomType == null)
                {
                    return HttpNotFound();
                }

                oldRoomType.Id = roomType.Id;
                oldRoomType.NameType = roomType.NameType;

                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(roomType);
        }

        //  RoomTypes/Create
        // Chỉ Admin
        public ActionResult Create()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        //  RoomTypes/Create
        // Chỉ Admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RoomType roomType)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                bool exists = db.RoomTypes
                    .Any(x => x.IDType == roomType.IDType);

                if (exists)
                {
                    ModelState.AddModelError(
                        "IDType",
                        "Mã loại phòng đã tồn tại.");

                    return View(roomType);
                }

                db.RoomTypes.Add(roomType);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(roomType);
        }

        // RoomTypes/Delete/DLX
        // Chỉ Admin
        public ActionResult Delete(string id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                return HttpNotFound();
            }

            RoomType roomType = db.RoomTypes
                .FirstOrDefault(x => x.IDType == id);

            if (roomType == null)
            {
                return HttpNotFound();
            }

            return View(roomType);
        }

        // RoomTypes/Delete/DLX
        // Chỉ Admin
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            RoomType roomType = db.RoomTypes
                .FirstOrDefault(x => x.IDType == id);

            if (roomType == null)
            {
                return HttpNotFound();
            }

            bool hasRooms = db.HotelRooms.Any(x => x.RoomType == id);
            if (hasRooms)
            {
                TempData["Error"] = "Không thể xóa loại phòng đang được sử dụng bởi phòng khách sạn.";
                return RedirectToAction("Index");
            }

            db.RoomTypes.Remove(roomType);
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