using Food_Court_Management_System.Models.Custom;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Food_Court_Management_System.Controllers
{
    public class AdminDashboardController : Controller
    {
        public ActionResult Index()
        {
            var dashboard = new AdminDashboardViewModel();

            using (var con = new OracleConnection(ConfigurationManager.ConnectionStrings["con"].ConnectionString))
            {
                con.Open();

                dashboard.TotalRestaurants = ExecuteScalarInt(con, "SELECT COUNT(*) FROM Restaurant");
                dashboard.TotalUsers = ExecuteScalarInt(con, "SELECT COUNT(*) FROM Customer");
                dashboard.TotalOrders = ExecuteScalarInt(con, "SELECT COUNT(*) FROM Order_FoodItem");
                dashboard.TotalTransactions = ExecuteScalarDecimal(con, "SELECT NVL(SUM(TransactionAmount), 0) FROM Transaction");
                dashboard.TotalEmployees = ExecuteScalarInt(con, "SELECT COUNT(*) FROM Employee");
                dashboard.RecentRestaurants = GetRecentRestaurants(con);
            }

            return View(dashboard);
        }

        private int ExecuteScalarInt(OracleConnection con, string query)
        {
            using (var cmd = new OracleCommand(query, con))
            {
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private decimal ExecuteScalarDecimal(OracleConnection con, string query)
        {
            using (var cmd = new OracleCommand(query, con))
            {
                return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }

        private List<RestaurantInfo> GetRecentRestaurants(OracleConnection con)
        {
            var list = new List<RestaurantInfo>();
            string query = @"
            SELECT * FROM (
                SELECT * FROM Restaurant ORDER BY RestaurantID DESC
            ) WHERE ROWNUM <= 3";

            using (var cmd = new OracleCommand(query, con))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(new RestaurantInfo
                    {
                        Name = reader["Name"].ToString(),
                        Email = reader["Email"].ToString(),
                        Phone = reader["Phone"].ToString()
                    });
                }
            }

            return list;
        }

        public ActionResult Restaurants(string searchTerm = "", string sortField = "Name", string sortOrder = "asc", int page = 1)
        {
            const int pageSize = 10;
            var list = new List<RestaurantInfo>();
            int totalCount = 0;

            using (var con = new OracleConnection(ConfigurationManager.ConnectionStrings["con"].ConnectionString))
            {
                con.Open();

                string searchPattern = "%" + searchTerm.ToLower() + "%";

                var whereClauses = new List<string>();
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    whereClauses.Add(@"
                LOWER(Name) LIKE :search1 OR 
                LOWER(Email) LIKE :search2 OR
                LOWER(Address) LIKE :search3
            ");
                }

                string whereSql = whereClauses.Count > 0 ? "WHERE " + string.Join(" OR ", whereClauses) : "";

                string orderByField = (sortField == "RestaurantID") ? "RestaurantID" : "Name";
                string orderBy = $"ORDER BY {orderByField} {(sortOrder == "desc" ? "DESC" : "ASC")}";

                // Count query
                string countQuery = $"SELECT COUNT(*) FROM Restaurant {whereSql}";
                using (var countCmd = new OracleCommand(countQuery, con))
                {
                    if (!string.IsNullOrWhiteSpace(searchTerm))
                    {
                        countCmd.Parameters.Add(new OracleParameter("search1", searchPattern));
                        countCmd.Parameters.Add(new OracleParameter("search2", searchPattern));
                        countCmd.Parameters.Add(new OracleParameter("search3", searchPattern));
                    }
                    totalCount = Convert.ToInt32(countCmd.ExecuteScalar());
                }

                int offset = (page - 1) * pageSize;

                // Data query
                string finalQuery = $@"
            SELECT * FROM (
                SELECT a.*, ROWNUM rnum FROM (
                    SELECT RestaurantID, Name, Email, Phone, Address
                    FROM Restaurant
                    {whereSql}
                    {orderBy}
                ) a WHERE ROWNUM <= :max
            ) WHERE rnum > :min";

                using (var cmd = new OracleCommand(finalQuery, con))
                {
                    if (!string.IsNullOrWhiteSpace(searchTerm))
                    {
                        cmd.Parameters.Add(new OracleParameter("search1", searchPattern));
                        cmd.Parameters.Add(new OracleParameter("search2", searchPattern));
                        cmd.Parameters.Add(new OracleParameter("search3", searchPattern));
                    }

                    cmd.Parameters.Add(new OracleParameter("max", offset + pageSize));
                    cmd.Parameters.Add(new OracleParameter("min", offset));

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new RestaurantInfo
                            {
                                RestaurantID = Convert.ToInt32(reader["RestaurantID"]),
                                Name = reader["Name"]?.ToString(),
                                Email = reader["Email"]?.ToString(),
                                Phone = reader["Phone"]?.ToString(),
                                Address = reader["Address"]?.ToString()
                            });
                        }
                    }
                }
            }

            var model = new RestaurantListViewModel
            {
                Restaurants = list,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                SortOrder = sortOrder,
                SearchTerm = searchTerm,
                SortField = sortField  // Add this property to your ViewModel
            };

            if (Request.IsAjaxRequest())
            {
                return PartialView("_RestaurantTable", model);
            }

            return View(model);
        }


        public ActionResult Users(string searchTerm = "", string sortField = "Username", string sortOrder = "asc", int? filterId = null, int page = 1)
        {
            const int pageSize = 10;
            var list = new List<CustomerInfo>();
            int totalCount = 0;

            using (var con = new OracleConnection(ConfigurationManager.ConnectionStrings["con"].ConnectionString))
            {
                con.Open();

                string searchPattern = "%" + searchTerm.ToLower() + "%";

                var whereClauses = new List<string>();
                if (filterId.HasValue)
                    whereClauses.Add("CustomerID = :filterId");

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    whereClauses.Add(@"
            (
                LOWER(Username) LIKE :search1 OR 
                LOWER(Name) LIKE :search2 OR 
                LOWER(Address) LIKE :search3 OR 
                LOWER(Email) LIKE :search4
            )");
                }

                string whereSql = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : "";

                // Determine sorting field
                string orderByField = (sortField == "CustomerID") ? "CustomerID" : "Username";
                string orderBy = $"ORDER BY {orderByField} {(sortOrder == "desc" ? "DESC" : "ASC")}";

                // Count query
                string countQuery = $"SELECT COUNT(*) FROM Customer {whereSql}";
                using (var countCmd = new OracleCommand(countQuery, con))
                {
                    if (filterId.HasValue)
                        countCmd.Parameters.Add(new OracleParameter("filterId", filterId.Value));
                    if (!string.IsNullOrWhiteSpace(searchTerm))
                    {
                        countCmd.Parameters.Add(new OracleParameter("search1", searchPattern));
                        countCmd.Parameters.Add(new OracleParameter("search2", searchPattern));
                        countCmd.Parameters.Add(new OracleParameter("search3", searchPattern));
                        countCmd.Parameters.Add(new OracleParameter("search4", searchPattern));
                    }

                    totalCount = Convert.ToInt32(countCmd.ExecuteScalar());
                }

                int offset = (page - 1) * pageSize;

                // Data query with pagination
                string finalQuery = $@"
            SELECT * FROM (
                SELECT a.*, ROWNUM rnum FROM (
                    SELECT CustomerID, Username, Email, Phone, Name, Address 
                    FROM Customer 
                    {whereSql}
                    {orderBy}
                ) a WHERE ROWNUM <= :max
            ) WHERE rnum > :min";

                using (var cmd = new OracleCommand(finalQuery, con))
                {
                    if (filterId.HasValue)
                        cmd.Parameters.Add(new OracleParameter("filterId", filterId.Value));
                    if (!string.IsNullOrWhiteSpace(searchTerm))
                    {
                        cmd.Parameters.Add(new OracleParameter("search1", searchPattern));
                        cmd.Parameters.Add(new OracleParameter("search2", searchPattern));
                        cmd.Parameters.Add(new OracleParameter("search3", searchPattern));
                        cmd.Parameters.Add(new OracleParameter("search4", searchPattern));
                    }

                    cmd.Parameters.Add(new OracleParameter("max", offset + pageSize));
                    cmd.Parameters.Add(new OracleParameter("min", offset));

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new CustomerInfo
                            {
                                CustomerID = Convert.ToInt32(reader["CustomerID"]),
                                Username = reader["Username"]?.ToString(),
                                Email = reader["Email"]?.ToString(),
                                Phone = reader["Phone"]?.ToString(),
                                Name = reader["Name"]?.ToString(),
                                Address = reader["Address"]?.ToString()
                            });
                        }
                    }
                }
            }

            var model = new CustomerListViewModel
            {
                Customers = list,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                SortOrder = sortOrder,
                SearchTerm = searchTerm,
                FilterId = filterId,
                SortField = sortField  // Make sure you add this property to your ViewModel
            };

            if (Request.IsAjaxRequest())
            {
                return PartialView("_UserTable", model);
            }

            return View(model);
        }

        public ActionResult Employees(string searchTerm = "", string sortField = "Name", string sortOrder = "asc", int? filterId = null, int? filterRestaurantId = null, int page = 1)
        {
            const int pageSize = 10;
            var list = new List<EmployeeInfo>();
            int totalCount = 0;

            using (var con = new OracleConnection(ConfigurationManager.ConnectionStrings["con"].ConnectionString))
            {
                con.Open();
                string searchPattern = "%" + searchTerm.ToLower() + "%";

                var whereClauses = new List<string>();
                if (filterId.HasValue)
                    whereClauses.Add("EmployeeID = :filterId");

                if (filterRestaurantId.HasValue)
                    whereClauses.Add("RestaurantID = :filterRestaurantId");

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    whereClauses.Add(@"
                (
                    LOWER(Name) LIKE :search OR 
                    LOWER(Email) LIKE :search OR 
                    LOWER(Phone) LIKE :search OR 
                    LOWER(Designation) LIKE :search
                )");
                }

                string whereSql = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : "";

                string orderByField = (sortField == "EmployeeID") ? "EmployeeID" : "Name";
                string orderBy = $"ORDER BY {orderByField} {(sortOrder == "desc" ? "DESC" : "ASC")}";

                // Count query
                string countQuery = $"SELECT COUNT(*) FROM Employee {whereSql}";
                using (var countCmd = new OracleCommand(countQuery, con))
                {
                    if (filterId.HasValue)
                        countCmd.Parameters.Add(new OracleParameter("filterId", filterId.Value));
                    if (filterRestaurantId.HasValue)
                        countCmd.Parameters.Add(new OracleParameter("filterRestaurantId", filterRestaurantId.Value));
                    if (!string.IsNullOrWhiteSpace(searchTerm))
                        countCmd.Parameters.Add(new OracleParameter("search", searchPattern));

                    totalCount = Convert.ToInt32(countCmd.ExecuteScalar());
                }

                int offset = (page - 1) * pageSize;

                string finalQuery = $@"
            SELECT * FROM (
                SELECT a.*, ROWNUM rnum FROM (
                    SELECT * FROM Employee
                    {whereSql}
                    {orderBy}
                ) a WHERE ROWNUM <= :max
            ) WHERE rnum > :min";

                using (var cmd = new OracleCommand(finalQuery, con))
                {
                    if (filterId.HasValue)
                        cmd.Parameters.Add(new OracleParameter("filterId", filterId.Value));
                    if (filterRestaurantId.HasValue)
                        cmd.Parameters.Add(new OracleParameter("filterRestaurantId", filterRestaurantId.Value));
                    if (!string.IsNullOrWhiteSpace(searchTerm))
                        cmd.Parameters.Add(new OracleParameter("search", searchPattern));

                    cmd.Parameters.Add(new OracleParameter("max", offset + pageSize));
                    cmd.Parameters.Add(new OracleParameter("min", offset));

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new EmployeeInfo
                            {
                                EmployeeID = Convert.ToInt32(reader["EmployeeID"]),
                                RestaurantID = Convert.ToInt32(reader["RestaurantID"]),
                                Email = reader["Email"]?.ToString(),
                                Phone = reader["Phone"]?.ToString(),
                                Name = reader["Name"]?.ToString(),
                                Designation = reader["Designation"]?.ToString()
                            });
                        }
                    }
                }
            }

            var model = new EmployeeListViewModel
            {
                Employees = list,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                SortOrder = sortOrder,
                SearchTerm = searchTerm,
                FilterId = filterId,
                FilterRestaurantId = filterRestaurantId,
                SortField = sortField
            };

            if (Request.IsAjaxRequest())
            {
                return PartialView("_EmployeeTable", model);
            }

            return View(model);
        }

        public ActionResult Transactions(string sortField = "TransactionTime", string sortOrder = "asc", string filterType = "", int? filterId = null, int page = 1)
        {
            const int pageSize = 10;
            var list = new List<TransactionInfo>();
            int totalCount = 0;

            using (var con = new OracleConnection(ConfigurationManager.ConnectionStrings["con"].ConnectionString))
            {
                con.Open();

                var whereClauses = new List<string>();
                if (filterId.HasValue)
                    whereClauses.Add("TransactionID = :filterId");

                if (!string.IsNullOrWhiteSpace(filterType))
                    whereClauses.Add("LOWER(Type) = :filterType");

                string whereSql = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : "";

                string orderByField = sortField == "TransactionID" ? "TransactionID" : "TransactionTime";
                string orderBy = $"ORDER BY {orderByField} {(sortOrder == "desc" ? "DESC" : "ASC")}";

                // Count query
                string countQuery = $"SELECT COUNT(*) FROM Transaction {whereSql}";
                using (var countCmd = new OracleCommand(countQuery, con))
                {
                    if (filterId.HasValue)
                        countCmd.Parameters.Add(new OracleParameter("filterId", filterId.Value));
                    if (!string.IsNullOrWhiteSpace(filterType))
                        countCmd.Parameters.Add(new OracleParameter("filterType", filterType.ToLower()));

                    totalCount = Convert.ToInt32(countCmd.ExecuteScalar());
                }

                int offset = (page - 1) * pageSize;

                // Data query
                string finalQuery = $@"
            SELECT * FROM (
                SELECT a.*, ROWNUM rnum FROM (
                    SELECT * FROM Transaction 
                    {whereSql}
                    {orderBy}
                ) a WHERE ROWNUM <= :max
            ) WHERE rnum > :min";

                using (var cmd = new OracleCommand(finalQuery, con))
                {
                    if (filterId.HasValue)
                        cmd.Parameters.Add(new OracleParameter("filterId", filterId.Value));
                    if (!string.IsNullOrWhiteSpace(filterType))
                        cmd.Parameters.Add(new OracleParameter("filterType", filterType.ToLower()));

                    cmd.Parameters.Add(new OracleParameter("max", offset + pageSize));
                    cmd.Parameters.Add(new OracleParameter("min", offset));

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new TransactionInfo
                            {
                                TransactionID = Convert.ToInt32(reader["TransactionID"]),
                                OrderID = reader["OrderID"] != DBNull.Value ? Convert.ToInt32(reader["OrderID"]) : (int?)null,
                                WalletID = Convert.ToInt32(reader["WalletID"]),
                                TransactionTime = Convert.ToDateTime(reader["TransactionTime"]),
                                TransactionAmount = Convert.ToDecimal(reader["TransactionAmount"]),
                                Type = reader["Type"].ToString(),
                                Note = reader["Note"]?.ToString()
                            });
                        }
                    }
                }
            }

            var model = new TransactionListViewModel
            {
                Transactions = list,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                SortOrder = sortOrder,
                SortField = sortField,
                FilterType = filterType,
                FilterId = filterId
            };

            if (Request.IsAjaxRequest())
            {
                return PartialView("_TransactionTable", model);
            }

            return View(model);
        }

        public ActionResult Coupons(string searchTerm = "", string sortOrder = "asc", string sortField = "CouponCode", string expiryStatus = "", int? filterId = null, int page = 1)
        {
            const int pageSize = 10;
            var list = new List<CouponInfo>();
            int totalCount = 0;

            using (var con = new OracleConnection(ConfigurationManager.ConnectionStrings["con"].ConnectionString))
            {
                con.Open();
                string searchPattern = "%" + searchTerm.ToLower() + "%";
                var whereClauses = new List<string>();

                if (filterId.HasValue)
                    whereClauses.Add("CouponID = :filterId");

                if (!string.IsNullOrWhiteSpace(searchTerm))
                    whereClauses.Add("LOWER(CouponCode) LIKE :search");

                if (expiryStatus == "available")
                    whereClauses.Add("ExpiryDate >= SYSDATE");
                else if (expiryStatus == "expired")
                    whereClauses.Add("ExpiryDate < SYSDATE");

                string whereSql = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : "";

                string orderBy = "ORDER BY " + (sortField == "CouponID" ? "CouponID" : "CouponCode") + (sortOrder == "desc" ? " DESC" : " ASC");

                // Count query
                string countQuery = $"SELECT COUNT(*) FROM Coupon {whereSql}";
                using (var countCmd = new OracleCommand(countQuery, con))
                {
                    if (filterId.HasValue)
                        countCmd.Parameters.Add(new OracleParameter("filterId", filterId.Value));
                    if (!string.IsNullOrWhiteSpace(searchTerm))
                        countCmd.Parameters.Add(new OracleParameter("search", searchPattern));
                    totalCount = Convert.ToInt32(countCmd.ExecuteScalar());
                }

                int offset = (page - 1) * pageSize;

                // Final query
                string finalQuery = $@"
            SELECT * FROM (
                SELECT a.*, ROWNUM rnum FROM (
                    SELECT * FROM Coupon
                    {whereSql}
                    {orderBy}
                ) a WHERE ROWNUM <= :max
            ) WHERE rnum > :min";

                using (var cmd = new OracleCommand(finalQuery, con))
                {
                    if (filterId.HasValue)
                        cmd.Parameters.Add(new OracleParameter("filterId", filterId.Value));
                    if (!string.IsNullOrWhiteSpace(searchTerm))
                        cmd.Parameters.Add(new OracleParameter("search", searchPattern));
                    cmd.Parameters.Add(new OracleParameter("max", offset + pageSize));
                    cmd.Parameters.Add(new OracleParameter("min", offset));

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new CouponInfo
                            {
                                CouponID = Convert.ToInt32(reader["CouponID"]),
                                CouponCode = reader["CouponCode"].ToString(),
                                Discount = Convert.ToDecimal(reader["Discount"]),
                                ExpiryDate = Convert.ToDateTime(reader["ExpiryDate"]),
                                OrderValue = reader["OrderValue"] != DBNull.Value ? Convert.ToDecimal(reader["OrderValue"]) : (decimal?)null,
                                AvailableQuantity = Convert.ToInt32(reader["AvailableQuantity"])
                            });
                        }
                    }
                }
            }

            var model = new CouponListViewModel
            {
                Coupons = list,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                SortOrder = sortOrder,
                SearchTerm = searchTerm,
                FilterId = filterId
            };

            if (Request.IsAjaxRequest())
            {
                return PartialView("_CouponTable", model);
            }

            return View(model);
        }


    }
}