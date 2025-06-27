using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Food_Court_Management_System.Models.Custom
{
    public class CustomerModel
    {
        public int CustomerID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public decimal Balance { get; set; }

    }

}