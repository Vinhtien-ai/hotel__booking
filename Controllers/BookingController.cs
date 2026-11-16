using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace HotelBoking.Controllers
{
    public class BookingController : Controller
    {
        private DBHotelBookingEntities db = new DBHotelBookingEntities();

        // Kiểm tra quyền Admin
        private bool IsAdmin()
        {
            return Session["Role"] != null
                && Session["Role"].ToString() == "Admin";
        }

        // Kiểm tra quyền Customer
        private bool IsCustomer()
        {
            return Session["Role"] != null
                && Session["Role"].ToString() == "Customer"
                && Session["IDCus"] != null;
        }

        // Kiểm tra phòng đã được đặt trong khoảng ngày bị giao nhau hay chưa
        private bool HasRoomConflict(
            System.Collections.Generic.ICollection<int> roomIds,
            DateTime checkIn,
            DateTime checkOut,
            int? excludeBookingId = null)
        {
            if (roomIds == null || roomIds.Count == 0)
            {
                return false;
            }

            var details = db.BookingDetails.Where(detail =>
                detail.IDRoom.HasValue &&
                roomIds.Contains(detail.IDRoom.Value) &&
                detail.Booking != null &&
                detail.Booking.DateBooking.HasValue &&
                detail.Booking.CheckOut.HasValue &&
                detail.Booking.DateBooking.Value < checkOut.Date &&
                detail.Booking.CheckOut.Value > checkIn.Date);

            if (excludeBookingId.HasValue)
            {
                details = details.Where(detail => detail.IDBooking != excludeBookingId.Value);
            }

            return details.Any();
        }

        //Booking
        public ActionResult Index()
        {
            if (IsCustomer())
            {
                // Đặt phòng là luồng chọn phòng Cart chỉ dành cho menu giỏ đặt phòng
                return RedirectToAction("Index", "Room");
            }

            if (!IsAdmin())
            {
                TempData["Error"] = "Vui lòng đăng nhập để tiếp tục.";
                return RedirectToAction("Index", "Login");
            }

            return RedirectToAction("History");
        }

        //Booking/SubmitBooking
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitBooking(
            DateTime? checkIn,
            DateTime? checkOut,
            string specialRequest)
        {
            // Chỉ Customer được đặt phòng qua Cart
            if (!IsCustomer())
            {
                TempData["Error"] = "Bạn không có quyền đặt phòng.";
                return RedirectToAction("Index", "Login");
            }

            // Kiểm tra điều kieenjn ngày đặt phòng
            if (checkIn == null || checkOut == null)
            {
                TempData["Error"] =
                    "Vui lòng nhập ngày nhận và trả phòng.";

                return RedirectToAction("Index", "Cart");
            }

            if (checkIn.Value.Date < DateTime.Today)
            {
                TempData["Error"] = "Ngày nhận phòng không hợp lệ";
                return RedirectToAction("Index", "Cart");
            }

            if (checkOut.Value.Date <= checkIn.Value.Date)
            {
                TempData["Error"] =
                    "Ngày trả phòng phải sau ngày nhận phòng.";

                return RedirectToAction("Index", "Cart");
            }

            int customerId = Convert.ToInt32(Session["IDCus"]);

            Customer customer = db.Customers
                .FirstOrDefault(x => x.IDCus == customerId);

            if (customer == null)
            {
                TempData["Error"] =
                    "Tài khoản khách hàng không hợp lệ.";

                return RedirectToAction("Index", "Login");
            }

            // Lấy giỏ phòng
            var cart = Session["Cart"]
                as System.Collections.Generic.List<int>;

            if (cart == null || cart.Count == 0)
            {
                TempData["Error"] = "Giỏ đặt phòng đang trống.";
                return RedirectToAction("Index", "Cart");
            }

            // Kiểm tra phòng trong giỏ
            var rooms = db.HotelRooms
                .Where(x => cart.Contains(x.RoomID))
                .ToList();

            if (rooms.Count != cart.Distinct().Count())
            {
                TempData["Error"] =
                    "Có phòng không hợp lệ trong giỏ.";

                return RedirectToAction("Index", "Cart");
            }

            // Tạo Booking
            Booking booking = new Booking
            {
                DateBooking = checkIn.Value.Date,
                CheckOut = checkOut.Value.Date,
                IDCus = customerId,
                AddressDelivery = specialRequest,
                Status = "Chưa thanh toán"
            };

           // kiểm tra điều kiện đặt phòng của kahsch
            using (var transaction = db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                try
                {
                    if (HasRoomConflict(rooms.Select(room => room.RoomID).ToList(), checkIn.Value.Date, checkOut.Value.Date))
                    {
                        transaction.Rollback();
                        TempData["Error"] = "Phòng đã có khách đặt trong khoảng ngày này. Vui lòng chọn phòng hoặc ngày khác.";
                        return RedirectToAction("Index", "Cart");
                    }

                    db.Bookings.Add(booking);
                    db.SaveChanges();

                    foreach (var room in rooms)
                    {
                        BookingDetail detail = new BookingDetail
                        {
                            IDBooking = booking.ID,
                            IDRoom = room.RoomID,
                            Quantity = 1,
                            UnitPrice = room.Price.HasValue ? Convert.ToDouble(room.Price.Value) : 0
                        };
                        db.BookingDetails.Add(detail);
                    }

                    db.SaveChanges();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }

            // Xóa giỏ sau khi đặt thành công
            Session["Cart"] = null;

            TempData["Success"] = "Đã tạo đơn đặt phòng. Vui lòng hoàn tất thanh toán để xác nhận đặt phòng.";

            return RedirectToAction(
                "Details",
                new { id = booking.ID }
            );
        }

        // Booking/Payment/5 trang pay
        public ActionResult Payment(int? id)
        {
            if (!IsCustomer())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Booking booking = db.Bookings
                .Include("BookingDetails.HotelRoom")
                .FirstOrDefault(x => x.ID == id.Value);

            if (booking == null)
            {
                return HttpNotFound();
            }

            int customerId = Convert.ToInt32(Session["IDCus"]);
            if (booking.IDCus != customerId)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            if (booking.Status != "Chưa thanh toán")
            {
                TempData["Success"] = "Đơn đặt phòng này đã được thanh toán hoặc xác nhận.";
                return RedirectToAction("Details", new { id = booking.ID });
            }

            return View(booking);
        }

        // Booking/Pay - xác nhận pay
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Pay(int id)
        {
            if (!IsCustomer())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            Booking booking = db.Bookings.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }

            int customerId = Convert.ToInt32(Session["IDCus"]);
            if (booking.IDCus != customerId)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            if (booking.Status == "Chưa thanh toán")
            {
                booking.Status = "Đã đặt";
                db.SaveChanges();
                TempData["Success"] = "Thanh toán thành công! Đặt phòng của bạn đã được xác nhận.";
            }
            else
            {
                TempData["Success"] = "Đơn đặt phòng này đã được xác nhận thanh toán.";
            }

            return RedirectToAction("Details", new { id = booking.ID });
        }

        // Booking/History
        public ActionResult History()
        {
            if (!IsAdmin() && !IsCustomer())
            {
                TempData["Error"] =
                    "Vui lòng đăng nhập để xem lịch sử đặt phòng.";

                return RedirectToAction("Index", "Login");
            }

            IQueryable<Booking> bookings = db.Bookings
                .Include("Customer")
                .Include("BookingDetails.HotelRoom");

            // Customer chỉ xem booking của mình
            if (IsCustomer())
            {
                int customerId;
                if (!int.TryParse(Convert.ToString(Session["IDCus"]), out customerId))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }

                bookings = bookings.Where(x => x.IDCus == customerId);
            }

            var result = bookings
                .OrderByDescending(x => x.ID)
                .ToList();

            return View("~/Views/Booking/History.cshtml", result);
        }

        //  Booking/Details/5
        public ActionResult Details(int? id)
        {
            if (!IsAdmin() && !IsCustomer())
            {
                TempData["Error"] =
                    "Vui lòng đăng nhập để xem chi tiết.";

                return RedirectToAction("Index", "Login");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest
                );
            }

            Booking booking = db.Bookings
                .Include("Customer")
                .Include("BookingDetails.HotelRoom")
                .FirstOrDefault(x => x.ID == id);

            if (booking == null)
            {
                return HttpNotFound();
            }

            // Customer không được xem booking của người khác
            if (IsCustomer())
            {
                int customerId = Convert.ToInt32(Session["IDCus"]);

                if (booking.IDCus != customerId)
                {
                    return new HttpStatusCodeResult(
                        HttpStatusCode.Forbidden
                    );
                }
            }

            return View("~/Views/Booking/Details.cshtml", booking);
        }

        // GET: Booking/Create
        public ActionResult Create(int? roomId)
        {
            if (!IsAdmin())
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.Forbidden
                );
            }

            ViewBag.IDCus = new SelectList(
                db.Customers,
                "IDCus",
                "NameCus"
            );

            ViewBag.IDRoom = new SelectList(
                db.HotelRooms,
                "RoomID",
                "NameRoom",
                roomId
            );

            return View("~/Views/Booking/Create.cshtml");
        }

        // Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "DateBooking,CheckOut,IDCus,AddressDelivery")]
            Booking booking,
            int? IDRoom,
            int? Quantity)
        {
            if (!IsAdmin())
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.Forbidden
                );
            }

            if (Quantity != 1)
            {
                ModelState.AddModelError(
                    "Quantity",
                    "Mỗi mã phòng là một phòng vật lý, số lượng phải bằng 1."
                );
            }

            Customer customer = db.Customers
                .FirstOrDefault(x => x.IDCus == booking.IDCus);

            if (customer == null)
            {
                ModelState.AddModelError(
                    "IDCus",
                    "Vui lòng chọn khách hàng."
                );
            }

            HotelRoom room = null;

            if (IDRoom.HasValue)
            {
                room = db.HotelRooms
                    .FirstOrDefault(x => x.RoomID == IDRoom.Value);
            }

            if (room == null)
            {
                ModelState.AddModelError(
                    "IDRoom",
                    "Vui lòng chọn phòng."
                );
            }

            if (!booking.CheckOut.HasValue)
            {
                ModelState.AddModelError("CheckOut", "Vui lòng chọn ngày trả phòng.");
            }

            if (ModelState.IsValid)
            {
                booking.DateBooking = booking.DateBooking ?? DateTime.Now;

                if (booking.CheckOut.Value.Date <= booking.DateBooking.Value.Date)
                {
                    ModelState.AddModelError("CheckOut", "Ngày trả phòng phải sau ngày nhận phòng.");
                }
                else
                {
                    using (var transaction = db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
                    {
                        try
                        {
                            if (HasRoomConflict(new[] { IDRoom.Value }, booking.DateBooking.Value.Date, booking.CheckOut.Value.Date))
                            {
                                ModelState.AddModelError("IDRoom", "Phòng đã có đơn đặt trong khoảng ngày này.");
                                transaction.Rollback();
                            }
                            else
                            {
                                booking.DateBooking = booking.DateBooking.Value.Date;
                                booking.CheckOut = booking.CheckOut.Value.Date;
                                booking.Status = "Đã đặt";
                                db.Bookings.Add(booking);
                                db.SaveChanges();

                                BookingDetail detail = new BookingDetail
                                {
                                    IDBooking = booking.ID,
                                    IDRoom = IDRoom.Value,
                                    Quantity = 1,
                                    UnitPrice = room.Price.HasValue ? Convert.ToDouble(room.Price.Value) : 0
                                };
                                db.BookingDetails.Add(detail);
                                db.SaveChanges();
                                transaction.Commit();
                                return RedirectToAction("Details", new { id = booking.ID });
                            }
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }

            }

            ViewBag.IDCus = new SelectList(
                db.Customers,
                "IDCus",
                "NameCus",
                booking.IDCus
            );

            ViewBag.IDRoom = new SelectList(
                db.HotelRooms,
                "RoomID",
                "NameRoom",
                IDRoom
            );

            return View("~/Views/Booking/Create.cshtml", booking);
        }

        // Booking/Edit/5
        public ActionResult Edit(int? id)
        {
            if (!IsAdmin())
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.Forbidden
                );
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest
                );
            }

            Booking booking = db.Bookings.Find(id);

            if (booking == null)
            {
                return HttpNotFound();
            }

            ViewBag.IDCus = new SelectList(
                db.Customers,
                "IDCus",
                "NameCus",
                booking.IDCus
            );

            return View("~/Views/Booking/Edit.cshtml", booking);
        }

        // Booking/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "ID,DateBooking,CheckOut,IDCus,AddressDelivery")]
            Booking booking)
        {
            if (!IsAdmin())
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.Forbidden
                );
            }

            if (ModelState.IsValid)
            {
                Booking oldBooking = db.Bookings.Find(booking.ID);

                if (oldBooking == null)
                {
                    return HttpNotFound();
                }

                if (!booking.DateBooking.HasValue || !booking.CheckOut.HasValue)
                {
                    ModelState.AddModelError("CheckOut", "Vui lòng nhập ngày nhận và trả phòng.");
                }
                else if (booking.CheckOut.Value.Date <= booking.DateBooking.Value.Date)
                {
                    ModelState.AddModelError("CheckOut", "Ngày trả phòng phải sau ngày nhận phòng.");
                }
               //kiểm tra nếu trùng

                Customer customer = db.Customers
                    .FirstOrDefault(x => x.IDCus == booking.IDCus);

                if (customer == null)
                {
                    ModelState.AddModelError(
                        "IDCus",
                        "Khách hàng không hợp lệ."
                    );
                }
                else if (ModelState.IsValid)
                {
                    using (var transaction = db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
                    {
                        try
                        {
                            var currentRoomIds = db.BookingDetails
                                .Where(detail => detail.IDBooking == booking.ID && detail.IDRoom.HasValue)
                                .Select(detail => detail.IDRoom.Value)
                                .Distinct()
                                .ToList();

                            if (HasRoomConflict(currentRoomIds, booking.DateBooking.Value.Date, booking.CheckOut.Value.Date, booking.ID))
                            {
                                ModelState.AddModelError("CheckOut", "Khoảng ngày mới bị trùng với một đơn đặt phòng khác.");
                            }
                            else
                            {
                                oldBooking.DateBooking = booking.DateBooking.Value.Date;
                                oldBooking.CheckOut = booking.CheckOut.Value.Date;
                                oldBooking.IDCus = booking.IDCus;
                                oldBooking.AddressDelivery = booking.AddressDelivery;
                                db.SaveChanges();
                                transaction.Commit();
                                return RedirectToAction("History");
                            }
                            transaction.Rollback();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }

            ViewBag.IDCus = new SelectList(
                db.Customers,
                "IDCus",
                "NameCus",
                booking.IDCus
            );

            return View("~/Views/Booking/Edit.cshtml", booking);
        }

        // POST: Booking/UpdateStatus/ quyền admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string status)
        {
            if (!IsAdmin())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            string[] allowed = { "Đã đặt", "Đã nhận phòng", "Đã trả phòng" };
            if (!allowed.Contains(status))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Trạng thái không hợp lệ.");
            }

            Booking booking = db.Bookings.Find(id);
            if (booking == null) return HttpNotFound();

            if (booking.Status == "Chưa thanh toán")
            {
                TempData["Error"] = "Khách hàng chưa xác nhận thanh toán. Không thể cập nhật trạng thái lưu trú.";
                return RedirectToAction("History");
            }

            string current = String.IsNullOrWhiteSpace(booking.Status) ? "Đã đặt" : booking.Status;
            int currentIndex = Array.IndexOf(allowed, current);
            int nextIndex = Array.IndexOf(allowed, status);

           
            if (currentIndex < 0 || nextIndex < currentIndex || nextIndex > currentIndex + 1)
            {
                TempData["Error"] = "Không thể chuyển trạng thái. Vui lòng thực hiện đúng thứ tự quy trình.";
                return RedirectToAction("History");
            }

            booking.Status = status;
            db.SaveChanges();
            TempData["Success"] = "Đã cập nhật trạng thái đơn đặt phòng #" + id + ".";
            return RedirectToAction("History");
        }

        // GET: Booking/Delete/5
        public ActionResult Delete(int? id)
        {
            if (!IsAdmin())
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.Forbidden
                );
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest
                );
            }

            Booking booking = db.Bookings
                .Include("Customer")
                .FirstOrDefault(x => x.ID == id);

            if (booking == null)
            {
                return HttpNotFound();
            }

            return View("~/Views/Booking/Delete.cshtml", booking);
        }

        // POST: Booking/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!IsAdmin())
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.Forbidden
                );
            }

            Booking booking = db.Bookings.Find(id);

            if (booking == null)
            {
                return HttpNotFound();
            }

            var details = db.BookingDetails
                .Where(x => x.IDBooking == id)
                .ToList();

            foreach (BookingDetail detail in details)
            {
                db.BookingDetails.Remove(detail);
            }

            db.Bookings.Remove(booking);
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