using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Food_Court_Management_System.Models
{
    public class RestaurantDashboardViewModel
    {
        public string ManagerName { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSales { get; set; }
        public int ActiveCoupons { get; set; }
        public List<LatestOrder> LatestOrders { get; set; }
    }

    public class LatestOrder
    {
        public string OrderID { get; set; }
        public string Customer { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string Time { get; set; }
    }
}