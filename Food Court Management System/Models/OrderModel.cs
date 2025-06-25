using System;
using System.ComponentModel.DataAnnotations;
namespace Food_Court_Management_System.Models
{
    public class OrderModel
    {
        public int OrderID { get; set; }
        public int CustomerID { get; set; }
        public int CouponID { get; set; }
        public DateTime OrderTime { get; set; }
        public decimal OrderAmount { get; set; }
        public string OrderStatus { get; set; }
    }
}