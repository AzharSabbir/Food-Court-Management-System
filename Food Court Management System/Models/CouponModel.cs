using System;
using System.ComponentModel.DataAnnotations;
namespace Food_Court_Management_System.Models
{
    public class CouponModel
    {
        public int CouponID { get; set; }
        public string CouponCode { get; set; }
        public decimal Discount { get; set; }
        public DateTime ExpiryDate { get; set; }
        public decimal OrderValue { get; set; }
        public int AvailableQuantity { get; set; }
    }
}