using System;
using System.ComponentModel.DataAnnotations;
namespace Food_Court_Management_System.Models
{
    public class GiftCardModel
    {
        public int GiftCardID { get; set; }
        public string GiftCode { get; set; }
        public decimal Amount { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}