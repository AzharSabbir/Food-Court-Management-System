using System;
using System.ComponentModel.DataAnnotations;
namespace Food_Court_Management_System.Models
{
    public class TransactionModel
    {
        public int TransactionID { get; set; }
        public int OrderID { get; set; }
        public int WalletID { get; set; }
        public DateTime TransactionTime { get; set; }
        public decimal TransactionAmount { get; set; }
        public string Type { get; set; }
        public string Note { get; set; }
    }
}