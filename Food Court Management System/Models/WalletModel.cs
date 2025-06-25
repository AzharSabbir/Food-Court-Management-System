using System;
using System.ComponentModel.DataAnnotations;
namespace Food_Court_Management_System.Models
{
    public class WalletModel
    {
        public int WalletID { get; set; }
        public int CustomerID { get; set; }
        public string Pin { get; set; }
        public decimal Balance { get; set; }
    }
}