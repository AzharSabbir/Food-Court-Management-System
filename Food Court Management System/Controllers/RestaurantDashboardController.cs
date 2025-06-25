using Food_Court_Management_System.Models;
using System.Collections.Generic;
using System.Web.Mvc;
using Food_Court_Management_System.Models;

namespace Food_Court_Management_System.Controllers
{
    public class RestaurantDashboardController : Controller
    {
        public ActionResult RestaurantDashboard()
        {
            var model = new RestaurantDashboardViewModel
            {
                ManagerName = "Restaurant Manager",
                TotalOrders = 120,
                TotalSales = 1500,
                ActiveCoupons = 5,
                LatestOrders = new List<LatestOrder>
                {
                    new LatestOrder { OrderID = "#001", Customer = "John Doe", Amount = 50, Status = "Completed", Time = "12:30 PM" },
                    new LatestOrder { OrderID = "#002", Customer = "Jane Smith", Amount = 75, Status = "In Progress", Time = "1:00 PM" },
                    new LatestOrder { OrderID = "#003", Customer = "Michael Brown", Amount = 100, Status = "Pending", Time = "1:30 PM" }
                }
            };

            return View(model);
        }
    }
}
