using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace HotelBoking.Controllers
{
    public class CartController : Controller
    {
        private DBHotelBookingEntities db = new DBHotelBookingEntities();

        private bool IsCustomer()
        {
            return Session["Role"] != null
                && Session["Role"].ToString() == "Customer"
                && Session["IDCus"] != null;
        }

        // index Cart
        public ActionResult Index()
        {
            if (!IsCustomer())
            {
                TempData["Error"] = "Vui lòng đăng nhập bằng tài khoản khách hàng để sử dụng giỏ đặt phòng.";
                return RedirectToAction("Index", "Login");
            }

            List<int> cart = Session["Cart"] as List<int>;
            if (cart == null)
            {
                cart = new List<int>();
                Session["Cart"] = cart;
            }

            cart = cart.Distinct().ToList();

            var rooms = db.HotelRooms
                .Include("RoomType1")
                .Where(x => cart.Contains(x.RoomID))
                .ToList();

            // Đồng bộ lại giỏ theo những phòng còn tồn tại.
            Session["Cart"] = rooms.Select(x => x.RoomID).ToList();

            return View(rooms);
        }

        //  Cart/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddToCart(int roomId)
        {
            if (!IsCustomer())
            {
                TempData["Error"] = "Vui lòng đăng nhập bằng tài khoản khách hàng để thêm phòng vào giỏ.";
                return RedirectToAction("Index", "Login");
            }

            var room = db.HotelRooms.FirstOrDefault(x => x.RoomID == roomId);
            if (room == null)
            {
                TempData["Error"] = "Không tìm thấy phòng cần thêm.";
                return RedirectToAction("Index", "Room");
            }

            List<int> cart = Session["Cart"] as List<int> ?? new List<int>();

            if (!cart.Contains(roomId))
            {
                cart.Add(roomId);
                TempData["Success"] = "Đã thêm phòng vào giỏ đặt phòng.";
            }
            else
            {
                TempData["Success"] = "Phòng này đã có trong giỏ đặt phòng.";
            }

            Session["Cart"] = cart;
            return RedirectToAction("Index");
        }

        //  Cart/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Remove(int roomId)
        {
            if (!IsCustomer())
            {
                return RedirectToAction("Index", "Login");
            }

            List<int> cart = Session["Cart"] as List<int> ?? new List<int>();
            cart.Remove(roomId);
            Session["Cart"] = cart;

            TempData["Success"] = "Đã xóa phòng khỏi giỏ đặt phòng.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
