using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Food_Court_Management_System.Models;

namespace Food_Court_Management_System.Controllers
{
    public class OrderManagementController : Controller
    {
        private static readonly List<OrderModel> Orders = new List<OrderModel>
        {
            new OrderModel { OrderID = 1, CustomerID = 101, CouponID = 0, OrderTime = DateTime.Now.AddMinutes(-90), OrderAmount = 50.00m, OrderStatus = "Completed" },
            new OrderModel { OrderID = 2, CustomerID = 102, CouponID = 1, OrderTime = DateTime.Now.AddMinutes(-60), OrderAmount = 75.00m, OrderStatus = "In Progress" },
            new OrderModel { OrderID = 3, CustomerID = 103, CouponID = 0, OrderTime = DateTime.Now.AddMinutes(-30), OrderAmount = 100.00m, OrderStatus = "Pending" }
        };

        // ✅ Dictionary for customer names
        private static readonly Dictionary<int, string> CustomerNames = new Dictionary<int, string>
        {
            { 101, "John Doe" },
            { 102, "Jane Smith" },
            { 103, "Michael Brown" }
        };

        // ✅ Dictionary for ordered items
        private static readonly Dictionary<int, List<string>> OrderItems = new Dictionary<int, List<string>>
        {
            { 1, new List<string> { "Pizza - $12.99", "Burger - $8.99", "Drink - $3.00" } },
            { 2, new List<string> { "Pasta - $10.99", "Salad - $5.00" } },
            { 3, new List<string> { "Steak - $25.00", "Wine - $15.00" } }
        };

        public ActionResult Index(int? id)
        {
            var selected = id.HasValue ? Orders.FirstOrDefault(o => o.OrderID == id.Value) : Orders.FirstOrDefault();

            var viewModel = new OrderManagementViewModel
            {
                Orders = Orders,
                SelectedOrder = selected,
                SelectedCustomerName = selected != null && CustomerNames.ContainsKey(selected.CustomerID)
                    ? CustomerNames[selected.CustomerID]
                    : "Unknown",
                OrderedItems = selected != null && OrderItems.ContainsKey(selected.OrderID)
                    ? OrderItems[selected.OrderID]
                    : new List<string>()
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult MarkCompleted(int id)
        {
            var order = Orders.FirstOrDefault(o => o.OrderID == id);
            if (order != null)
            {
                order.OrderStatus = "Completed";
            }

            return RedirectToAction("Index", new { id });
        }
    }
}
