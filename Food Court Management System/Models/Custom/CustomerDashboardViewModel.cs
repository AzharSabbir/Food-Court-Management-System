using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Food_Court_Management_System.Models.Custom
{
    public class FoodItemInfo
    {
        public int ItemID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }


    public class ProfileViewModel
    {
        public int CustomerID { get; set; }
        public string Name { get; set; } = "";
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Address { get; set; } = "";
        public decimal WalletBalance { get; set; }
        public int TotalOrders { get; set; }
    }

    public class OrderDetailsViewModel
    {
        public int OrderID { get; set; }
        public DateTime OrderTime { get; set; }
        public decimal OrderAmount { get; set; }
        public string OrderStatus { get; set; }
        public string CustomerName { get; set; }

        public List<OrderedItemInfo> Items { get; set; } = new List<OrderedItemInfo>();
    }

    public class OrderedItemInfo
    {
        public int ItemID { get; set; }              
        public string ItemName { get; set; }
        public string RestaurantName { get; set; }
        public int OrderedQuantity { get; set; }
    }


    public class OrdersViewModel
    {
        public string CustomerName { get; set; }
        public List<OrderInfo> Orders { get; set; } = new List<OrderInfo>();
    }

    public class CustomerDashboardViewModel
    {
        public string CustomerName { get; set; }
        public List<FoodItemInfo> FoodItems { get; set; }
        public decimal WalletBalance { get; set; }
        public int ActiveOrdersCount { get; set; }
        public List<OrderInfo> RecentOrders { get; set; }
    }

    public class OrderInfo
    {
        public int OrderID { get; set; }
        public DateTime OrderTime { get; set; }
        public decimal OrderAmount { get; set; }
        public string OrderStatus { get; set; }
    }

    public class UserTransactionsViewModel
    {
        public string CustomerName { get; set; }
        public List<UserTransactionInfo> Transactions { get; set; }
    }

    public class UserTransactionInfo
    {
        public int TransactionID { get; set; }
        public int? OrderID { get; set; }
        public int WalletID { get; set; }
        public DateTime TransactionTime { get; set; }
        public decimal TransactionAmount { get; set; }
        public string Type { get; set; }
        public string Note { get; set; }
    }

}