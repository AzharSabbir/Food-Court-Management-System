using System;
using System.Collections.Generic;

namespace Food_Court_Management_System.Models
{
    public class OrderManagementViewModel
    {
        public List<OrderModel> Orders { get; set; }
        public OrderModel SelectedOrder { get; set; }

        // Optional: add lookup/display-only fields if needed
        public string SelectedCustomerName { get; set; }
        public List<string> OrderedItems { get; set; }
    }
}
