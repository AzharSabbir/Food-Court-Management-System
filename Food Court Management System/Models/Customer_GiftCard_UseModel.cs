using System;
using System.ComponentModel.DataAnnotations;
namespace Food_Court_Management_System.Models
{
    public class Customer_GiftCard_UseModel
    {
        public int CustomerID { get; set; }
        public int GiftCardID { get; set; }
        public DateTime RedeemTime { get; set; }
    }
}