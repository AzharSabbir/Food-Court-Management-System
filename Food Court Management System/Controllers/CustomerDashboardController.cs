using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Cryptography;
using System.Web.Mvc;
using Food_Court_Management_System.Models;
using Food_Court_Management_System.Models.Custom;
using Oracle.ManagedDataAccess.Client;

namespace Food_Court_Management_System.Controllers
{
    public class CustomerDashboardController : Controller
    {
        public ActionResult Index()
        {
            var dashboard = new CustomerDashboardViewModel();

            using (var con = new OracleConnection(ConfigurationManager.ConnectionStrings["con"].ConnectionString))
            {
                con.Open();

                string username = User.Identity.Name;
                int customerId = GetCustomerIdByUsername(con, username);

                dashboard.CustomerName = GetCustomerNameById(con, customerId);
                dashboard.WalletBalance = GetWalletBalance(con, customerId);
                dashboard.ActiveOrdersCount = GetActiveOrdersCount(con, customerId);
                dashboard.RecentOrders = GetRecentOrders(con, customerId);
                dashboard.FoodItems = GetFoodItems(con);
            }

            return View(dashboard);
        }

        private int GetCustomerIdByUsername(OracleConnection con, string username)
        {
            using (var cmd = new OracleCommand("SELECT CustomerID FROM Customer WHERE Username = :uname", con))
            {
                cmd.Parameters.Add(new OracleParameter("uname", username));
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private string GetCustomerNameById(OracleConnection con, int customerId)
        {
            using (var cmd = new OracleCommand("SELECT Name FROM Customer WHERE CustomerID = :cid", con))
            {
                cmd.Parameters.Add(new OracleParameter("cid", customerId));
                return cmd.ExecuteScalar()?.ToString() ?? "";
            }
        }

        private decimal GetWalletBalance(OracleConnection con, int customerId)
        {
            using (var cmd = new OracleCommand("SELECT NVL(Balance,0) FROM Wallet WHERE CustomerID = :cid", con))
            {
                cmd.Parameters.Add(new OracleParameter("cid", customerId));
                return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }

        private int GetActiveOrdersCount(OracleConnection con, int customerId)
        {
            using (var cmd = new OracleCommand("SELECT COUNT(*) FROM \"Order\" WHERE CustomerID = :cid AND OrderStatus != 'Delivered'", con))
            {
                cmd.Parameters.Add(new OracleParameter("cid", customerId));
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private List<FoodItemInfo> GetFoodItems(OracleConnection con)
        {
            var list = new List<FoodItemInfo>();
            string query = "SELECT ItemID, Name, Description FROM FoodItem";

            using (var cmd = new OracleCommand(query, con))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(new FoodItemInfo
                    {
                        ItemID = Convert.ToInt32(reader["ItemID"]),
                        Name = reader["Name"].ToString(),
                        Description = reader["Description"]?.ToString()
                    });
                }
            }

            return list;
        }

        private List<OrderInfo> GetRecentOrders(OracleConnection con, int customerId)
        {
            var list = new List<OrderInfo>();
            string query = @"
            SELECT * FROM (
                SELECT OrderID, OrderTime, OrderAmount, OrderStatus
                FROM ""Order""
                WHERE CustomerID = :cid
                ORDER BY OrderTime DESC
            )
            WHERE ROWNUM <= 5";

            using (var cmd = new OracleCommand(query, con))
            {
                cmd.Parameters.Add(new OracleParameter("cid", customerId));

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new OrderInfo
                        {
                            OrderID = Convert.ToInt32(reader["OrderID"]),
                            OrderTime = Convert.ToDateTime(reader["OrderTime"]),
                            OrderAmount = Convert.ToDecimal(reader["OrderAmount"]),
                            OrderStatus = reader["OrderStatus"].ToString()
                        });
                    }
                }
            }

            return list;
        }

        public ActionResult Orders(string sortField = "OrderTime", string sortOrder = "desc")
        {
            var model = new OrdersViewModel();

            using (var con = new OracleConnection(ConfigurationManager.ConnectionStrings["con"].ConnectionString))
            {
                con.Open();

                string username = User.Identity.Name;
                int customerId = GetCustomerIdByUsername(con, username);
                model.CustomerName = GetCustomerNameById(con, customerId);

                string orderByClause = $"{sortField} {(sortOrder == "asc" ? "ASC" : "DESC")}";

                string query = $@"
            SELECT OrderID, OrderTime, OrderAmount, OrderStatus
            FROM ""Order""
            WHERE CustomerID = :cid
            ORDER BY {orderByClause}";

                using (var cmd = new OracleCommand(query, con))
                {
                    cmd.Parameters.Add(new OracleParameter("cid", customerId));

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.Orders.Add(new OrderInfo
                            {
                                OrderID = Convert.ToInt32(reader["OrderID"]),
                                OrderTime = Convert.ToDateTime(reader["OrderTime"]),
                                OrderAmount = Convert.ToDecimal(reader["OrderAmount"]),
                                OrderStatus = reader["OrderStatus"].ToString()
                            });
                        }
                    }
                }
            }

            if (Request.IsAjaxRequest())
                return PartialView("_OrdersTable", model);

            return View(model);
        }

        public ActionResult ViewOrderDetails(int orderId)
        {
            var model = new OrderDetailsViewModel();

            using (var con = new OracleConnection(ConfigurationManager.ConnectionStrings["con"].ConnectionString))
            {
                con.Open();

                // Fetch order info
                string orderQuery = @"SELECT o.OrderID, o.OrderTime, o.OrderAmount, o.OrderStatus, c.Name AS CustomerName
                              FROM ""Order"" o
                              JOIN Customer c ON o.CustomerID = c.CustomerID
                              WHERE o.OrderID = :orderId";

                using (var cmd = new OracleCommand(orderQuery, con))
                {
                    cmd.Parameters.Add(new OracleParameter("orderId", orderId));
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model.OrderID = Convert.ToInt32(reader["OrderID"]);
                            model.OrderTime = Convert.ToDateTime(reader["OrderTime"]);
                            model.OrderAmount = Convert.ToDecimal(reader["OrderAmount"]);
                            model.OrderStatus = reader["OrderStatus"].ToString();
                            model.CustomerName = reader["CustomerName"].ToString();
                        }
                    }
                }

                // Fetch ordered food items
                string itemsQuery = @"
                    SELECT ofi.ItemID, fi.Name AS ItemName, r.Name AS RestaurantName, ofi.OrderedQuantity
                    FROM Order_FoodItem ofi
                    JOIN FoodItem fi ON ofi.ItemID = fi.ItemID
                    JOIN Restaurant r ON ofi.RestaurantID = r.RestaurantID
                    WHERE ofi.OrderID = :orderId";

                using (var cmd = new OracleCommand(itemsQuery, con))
                {
                    cmd.Parameters.Add(new OracleParameter("orderId", orderId));
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.Items.Add(new OrderedItemInfo
                            {
                                ItemID = Convert.ToInt32(reader["ItemID"]),
                                ItemName = reader["ItemName"].ToString(),
                                RestaurantName = reader["RestaurantName"].ToString(),
                                OrderedQuantity = Convert.ToInt32(reader["OrderedQuantity"])
                            });
                        }
                    }
                }
            }

            return View(model);
        }


        public ActionResult Transactions(string sortField = "TransactionTime", string sortOrder = "desc")
        {
            var username = User.Identity.Name;

            var model = new UserTransactionsViewModel();
            var transactions = new List<UserTransactionInfo>();

            using (var con = new OracleConnection(ConfigurationManager.ConnectionStrings["con"].ConnectionString))
            {
                con.Open();

                // Get CustomerID by username
                int customerId;
                using (var cmd = new OracleCommand("SELECT CustomerID FROM Customer WHERE Username = :username", con))
                {
                    cmd.Parameters.Add(new OracleParameter("username", username));
                    var result = cmd.ExecuteScalar();
                    if (result == null)
                    {
                        // handle user not found case, maybe redirect or empty model
                        return View(model);
                    }
                    customerId = Convert.ToInt32(result);
                }

                // Get WalletID by CustomerID
                int walletId;
                using (var cmd = new OracleCommand("SELECT WalletID FROM Wallet WHERE CustomerID = :customerId", con))
                {
                    cmd.Parameters.Add(new OracleParameter("customerId", customerId));
                    var result = cmd.ExecuteScalar();
                    if (result == null)
                    {
                        return View(model);
                    }
                    walletId = Convert.ToInt32(result);
                }

                // Validate sort field and order for security (prevent SQL injection)
                var validFields = new HashSet<string> { "TransactionTime", "TransactionAmount", "Type" };
                if (!validFields.Contains(sortField)) sortField = "TransactionTime";
                sortOrder = (sortOrder?.ToLower() == "asc") ? "ASC" : "DESC";

                string query = $@"
            SELECT TransactionID, OrderID, WalletID, TransactionTime, TransactionAmount, Type, Note
            FROM Transaction
            WHERE WalletID = :walletId
            ORDER BY {sortField} {sortOrder}";

                using (var cmd = new OracleCommand(query, con))
                {
                    cmd.Parameters.Add(new OracleParameter("walletId", walletId));

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            transactions.Add(new UserTransactionInfo
                            {
                                TransactionID = Convert.ToInt32(reader["TransactionID"]),
                                OrderID = reader["OrderID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["OrderID"]),
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

            model.Transactions = transactions;
            model.CustomerName = username; // optionally fetch full name if you want

            return View(model);
        }


        public ActionResult Profile()
        {
            var username = User.Identity.Name;

            var model = new ProfileViewModel
            {
                Username = username // <-- assign Username here
            };

            using (var con = new OracleConnection(ConfigurationManager.ConnectionStrings["con"].ConnectionString))
            {
                con.Open();

                // Get Customer info
                using (var cmd = new OracleCommand("SELECT CustomerID, Name, Email, Phone, Address FROM Customer WHERE Username = :username", con))
                {
                    cmd.Parameters.Add(new OracleParameter("username", username));
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model.CustomerID = Convert.ToInt32(reader["CustomerID"]);
                            model.Name = reader["Name"].ToString();
                            model.Email = reader["Email"].ToString();
                            model.Phone = reader["Phone"]?.ToString();
                            model.Address = reader["Address"]?.ToString();
                        }
                        else
                        {
                            // Handle user not found
                            return RedirectToAction("Index");
                        }
                    }
                }

                // Get Wallet balance
                using (var cmd = new OracleCommand("SELECT Balance FROM Wallet WHERE CustomerID = :custId", con))
                {
                    cmd.Parameters.Add(new OracleParameter("custId", model.CustomerID));
                    var bal = cmd.ExecuteScalar();
                    model.WalletBalance = bal != null ? Convert.ToDecimal(bal) : 0m;
                }

                // Get total orders count
                using (var cmd = new OracleCommand("SELECT COUNT(*) FROM \"Order\" WHERE CustomerID = :custId", con))
                {
                    cmd.Parameters.Add(new OracleParameter("custId", model.CustomerID));
                    var count = cmd.ExecuteScalar();
                    model.TotalOrders = count != null ? Convert.ToInt32(count) : 0;
                }
            }

            return View(model);
        }


        [HttpPost]
        public JsonResult UpdateProfile(string Name, string Email, string Phone, string Address)
        {
            var username = User.Identity.Name;

            try
            {
                using (var con = new OracleConnection(ConfigurationManager.ConnectionStrings["con"].ConnectionString))
                {
                    con.Open();

                    // Check if Email exists for other users
                    using (var cmd = new OracleCommand("SELECT COUNT(*) FROM Customer WHERE Email=:email AND Username<>:username", con))
                    {
                        cmd.Parameters.Add(new OracleParameter("email", Email));
                        cmd.Parameters.Add(new OracleParameter("username", username));
                        int emailCount = Convert.ToInt32(cmd.ExecuteScalar());
                        if (emailCount > 0)
                            return Json(new { success = false, message = "Email already exists." });
                    }

                    // Update Customer info
                    var updateQuery = "UPDATE Customer SET Name=:Name, Email=:Email, Phone=:Phone, Address=:Address WHERE Username=:username";
                    using (var cmd = new OracleCommand(updateQuery, con))
                    {
                        cmd.Parameters.Add(new OracleParameter("Name", Name));
                        cmd.Parameters.Add(new OracleParameter("Email", Email));
                        cmd.Parameters.Add(new OracleParameter("Phone", Phone));
                        cmd.Parameters.Add(new OracleParameter("Address", Address));
                        cmd.Parameters.Add(new OracleParameter("username", username));
                        cmd.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Update failed: " + ex.Message });
            }
        }


        [HttpPost]
        public JsonResult RechargeWallet(string giftCode)
        {
            var username = User.Identity.Name;

            try
            {
                using (var con = new OracleConnection(ConfigurationManager.ConnectionStrings["con"].ConnectionString))
                {
                    con.Open();

                    // Get CustomerID and WalletID
                    int customerId;
                    int walletId;
                    using (var cmd = new OracleCommand("SELECT c.CustomerID, w.WalletID FROM Customer c JOIN Wallet w ON c.CustomerID = w.CustomerID WHERE c.Username = :username", con))
                    {
                        cmd.Parameters.Add(new OracleParameter("username", username));
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                customerId = Convert.ToInt32(reader["CustomerID"]);
                                walletId = Convert.ToInt32(reader["WalletID"]);
                            }
                            else
                            {
                                return Json(new { success = false, message = "User not found." });
                            }
                        }
                    }

                    // Check GiftCard validity
                    decimal amount;
                    int giftCardId;
                    using (var cmd = new OracleCommand("SELECT GiftCardID, Amount, ExpiryDate FROM GiftCard WHERE GiftCode=:code", con))
                    {
                        cmd.Parameters.Add(new OracleParameter("code", giftCode));
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                giftCardId = Convert.ToInt32(reader["GiftCardID"]);
                                var expiry = Convert.ToDateTime(reader["ExpiryDate"]);
                                if (expiry < DateTime.Now)
                                    return Json(new { success = false, message = "Gift card expired." });

                                amount = Convert.ToDecimal(reader["Amount"]);
                            }
                            else
                            {
                                return Json(new { success = false, message = "Invalid gift card code." });
                            }
                        }
                    }

                    // Check if already used
                    using (var cmd = new OracleCommand("SELECT COUNT(*) FROM Customer_GiftCard_Use WHERE GIFTCARDID=:giftId", con))
                    {
                        cmd.Parameters.Add(new OracleParameter("giftId", giftCardId));
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        if (count > 0)
                            return Json(new { success = false, message = "This gift card has already been redeemed." });
                    }

                    // Update Wallet balance
                    using (var cmd = new OracleCommand("UPDATE Wallet SET Balance = Balance + :amount WHERE WalletID = :walletId", con))
                    {
                        cmd.Parameters.Add(new OracleParameter("amount", amount));
                        cmd.Parameters.Add(new OracleParameter("walletId", walletId));
                        cmd.ExecuteNonQuery();
                    }

                    // Record usage in Customer_GiftCard_Use
                    using (var cmd = new OracleCommand("INSERT INTO Customer_GiftCard_Use (CustomerID, GiftCardID, RedeemTime) VALUES (:custId, :giftId, :time)", con))
                    {
                        cmd.Parameters.Add(new OracleParameter("custId", customerId));
                        cmd.Parameters.Add(new OracleParameter("giftId", giftCardId));
                        cmd.Parameters.Add(new OracleParameter("time", DateTime.Now));
                        cmd.ExecuteNonQuery();
                    }

                    // Get new wallet balance
                    decimal newBalance;
                    using (var cmd = new OracleCommand("SELECT Balance FROM Wallet WHERE WalletID = :walletId", con))
                    {
                        cmd.Parameters.Add(new OracleParameter("walletId", walletId));
                        newBalance = Convert.ToDecimal(cmd.ExecuteScalar());
                    }

                    return Json(new { success = true, newBalance });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Recharge failed: " + ex.Message });
            }
        }


    }

}
