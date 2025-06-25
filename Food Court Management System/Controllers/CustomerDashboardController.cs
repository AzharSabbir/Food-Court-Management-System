using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Food_Court_Management_System.Models;

namespace Food_Court_Management_System.Controllers
{
    public class CustomerDashboardController : Controller
    {
        public ActionResult CustomerDashboard()
        {
            // Demo model with hardcoded values
            var model = new CustomerDashboardViewModel
            {
                CustomerName = "John Doe",
                WalletBalance = 150,
                ActiveOrders = 3,
                AvailableCoupons = 2,
                OrderHistory = new List<OrderHistoryItem>
                {
                    new OrderHistoryItem { OrderID = "#001", Restaurant = "Restaurant A", Amount = 50, Status = "Completed", Time = "12:30 PM" },
                    new OrderHistoryItem { OrderID = "#002", Restaurant = "Restaurant B", Amount = 75, Status = "In Progress", Time = "1:00 PM" },
                    new OrderHistoryItem { OrderID = "#003", Restaurant = "Restaurant C", Amount = 100, Status = "Pending", Time = "1:30 PM" }
                }
            };

            return View(model);
        }
    }

    public class CustomerDashboardViewModel
    {
        public string CustomerName { get; set; }
        public decimal WalletBalance { get; set; }
        public int ActiveOrders { get; set; }
        public int AvailableCoupons { get; set; }
        public List<OrderHistoryItem> OrderHistory { get; set; }
    }

    public class OrderHistoryItem
    {
        public string OrderID { get; set; }
        public string Restaurant { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string Time { get; set; }
    }
}
