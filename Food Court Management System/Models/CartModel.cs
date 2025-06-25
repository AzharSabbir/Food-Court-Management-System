using System;
using System.ComponentModel.DataAnnotations;
namespace Food_Court_Management_System.Models
{
    public class CartModel
    {
        public int CartID { get; set; }
        public int CustomerID { get; set; }
        public decimal TotalAmount { get; set; }
    }
}