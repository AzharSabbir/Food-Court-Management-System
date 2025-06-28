using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Food_Court_Management_System.Models.Custom
{
    public class AdminDashboardViewModel
    {
        public int TotalRestaurants { get; set; }
        public int TotalUsers { get; set; }
        public int TotalOrders { get; set; }
        public int TotalEmployees { get; set; }
        public decimal TotalTransactions { get; set; }
        public List<RestaurantInfo> RecentRestaurants { get; set; }
    }

    public class RestaurantInfo
    {
        public int RestaurantID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
    }

    public class RestaurantListViewModel
    {
        public List<RestaurantInfo> Restaurants { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SortOrder { get; set; }
        public string SearchTerm { get; set; }
        public string SortField { get; set; } 
    }



    public class CustomerInfo
    {
        public int CustomerID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
    }

    public class CustomerListViewModel
    {
        public List<CustomerInfo> Customers { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SortOrder { get; set; }
        public string SearchTerm { get; set; }
        public int? FilterId { get; set; }
        public string SortField { get; set; }  
    }



    public class EmployeeInfo
    {
        public int EmployeeID { get; set; }
        public int RestaurantID { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Name { get; set; }
        public string Designation { get; set; }
    }

    public class EmployeeListViewModel
    {
        public List<EmployeeInfo> Employees { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SortOrder { get; set; }
        public string SortField { get; set; }  // Add this
        public string SearchTerm { get; set; }
        public int? FilterId { get; set; }
        public int? FilterRestaurantId { get; set; }  // Add this
    }


    public class TransactionInfo
    {
        public int TransactionID { get; set; }
        public int? OrderID { get; set; }
        public int WalletID { get; set; }
        public DateTime TransactionTime { get; set; }
        public decimal TransactionAmount { get; set; }
        public string Type { get; set; }
        public string Note { get; set; }
    }

    public class TransactionListViewModel
    {
        public List<TransactionInfo> Transactions { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SortOrder { get; set; }
        public string SortField { get; set; }
        public string FilterType { get; set; }
        public string SearchTerm { get; set; }
        public int? FilterId { get; set; }
    }

    public class CouponInfo
    {
        public int CouponID { get; set; }
        public string CouponCode { get; set; }
        public decimal Discount { get; set; }
        public DateTime ExpiryDate { get; set; }
        public decimal? OrderValue { get; set; }
        public int AvailableQuantity { get; set; }
    }

    public class CouponListViewModel
    {
        public List<CouponInfo> Coupons { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SortOrder { get; set; }
        public string SortField { get; set; }
        public string SearchTerm { get; set; }
        public int? FilterId { get; set; }
        public string StatusFilter { get; set; } // "All", "Available", "Expired"
    }


}