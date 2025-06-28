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
    public class FoodController : Controller
    {
        public ActionResult Details(int id)
        {
            var model = new FoodItemDetailsViewModel();

            using (var con = new OracleConnection(ConfigurationManager.ConnectionStrings["con"].ConnectionString))
            {
                con.Open();

                // Get logged-in customer name for sidebar top bar
                string username = User.Identity.Name;
                string customerName = "";

                using (var cmd = new OracleCommand("SELECT Name FROM Customer WHERE Username = :username", con))
                {
                    cmd.Parameters.Add(new OracleParameter("username", username));
                    var result = cmd.ExecuteScalar();
                    customerName = result != null ? result.ToString() : "";
                }

                model.CustomerName = customerName;

                // Get Food Item Summary
                using (var cmd = new OracleCommand("SELECT Name, Description FROM FoodItem WHERE ItemID = :itemId", con))
                {
                    cmd.Parameters.Add(new OracleParameter("itemId", id));
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model.ItemID = id;
                            model.Name = reader["Name"].ToString();
                            model.Description = reader["Description"]?.ToString();
                        }
                        else
                        {
                            return HttpNotFound();
                        }
                    }
                }

                // Get available restaurant offers
                var offers = new List<RestaurantOfferInfo>();
                string query = @"
            SELECT r.RestaurantID, r.Name, r.Address, rf.Price, rf.AvailableQuantity
            FROM Restaurant_FoodItem rf
            JOIN Restaurant r ON rf.RestaurantID = r.RestaurantID
            WHERE rf.ItemID = :itemId";

                using (var cmd = new OracleCommand(query, con))
                {
                    cmd.Parameters.Add(new OracleParameter("itemId", id));
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            offers.Add(new RestaurantOfferInfo
                            {
                                RestaurantID = Convert.ToInt32(reader["RestaurantID"]),
                                RestaurantName = reader["Name"].ToString(),
                                RestaurantAddress = reader["Address"].ToString(),
                                Price = Convert.ToDecimal(reader["Price"]),
                                AvailableQuantity = Convert.ToInt32(reader["AvailableQuantity"])
                            });
                        }
                    }
                }

                model.AvailableRestaurants = offers;
            }

            return View(model);
        }


        public ActionResult PlaceOrder(int itemId, int restaurantId)
        {
            using (var con = new OracleConnection(ConfigurationManager.ConnectionStrings["con"].ConnectionString))
            {
                con.Open();
                OracleTransaction transaction = con.BeginTransaction();

                try
                {
                    // Get logged-in CustomerID
                    string username = User.Identity.Name;
                    int customerId = 0;

                    using (var cmd = new OracleCommand("SELECT CustomerID FROM Customer WHERE Username = :username", con))
                    {
                        cmd.Parameters.Add(new OracleParameter("username", username));
                        var result = cmd.ExecuteScalar();
                        if (result == null)
                            throw new Exception("Customer not found.");
                        customerId = Convert.ToInt32(result);
                    }

                    // Get price and available quantity from RESTAURANT_FOODITEM
                    decimal price = 0;
                    int availableQty = 0;

                    using (var cmd = new OracleCommand("SELECT Price, AvailableQuantity FROM Restaurant_FoodItem WHERE RestaurantID = :restId AND ItemID = :itemId", con))
                    {
                        cmd.Parameters.Add(new OracleParameter("restId", restaurantId));
                        cmd.Parameters.Add(new OracleParameter("itemId", itemId));

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                price = Convert.ToDecimal(reader["Price"]);
                                availableQty = Convert.ToInt32(reader["AvailableQuantity"]);
                            }
                            else
                                throw new Exception("Selected item not found at this restaurant.");
                        }
                    }

                    if (availableQty <= 0)
                        throw new Exception("Item is out of stock at this restaurant.");

                    // Get wallet balance
                    decimal walletBalance = 0;
                    int walletId = 0;

                    using (var cmd = new OracleCommand("SELECT WalletID, Balance FROM Wallet WHERE CustomerID = :custId", con))
                    {
                        cmd.Parameters.Add(new OracleParameter("custId", customerId));
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                walletId = Convert.ToInt32(reader["WalletID"]);
                                walletBalance = Convert.ToDecimal(reader["Balance"]);
                            }
                            else
                                throw new Exception("Wallet not found for customer.");
                        }
                    }

                    if (walletBalance < price)
                        throw new Exception("Insufficient wallet balance.");

                    // Deduct price from wallet
                    using (var cmd = new OracleCommand("UPDATE Wallet SET Balance = Balance - :price WHERE WalletID = :wid", con))
                    {
                        cmd.Parameters.Add(new OracleParameter("price", price));
                        cmd.Parameters.Add(new OracleParameter("wid", walletId));
                        cmd.ExecuteNonQuery();
                    }

                    // Decrease available quantity by 1
                    using (var cmd = new OracleCommand("UPDATE Restaurant_FoodItem SET AvailableQuantity = AvailableQuantity - 1 WHERE RestaurantID = :restId AND ItemID = :itemId", con))
                    {
                        cmd.Parameters.Add(new OracleParameter("restId", restaurantId));
                        cmd.Parameters.Add(new OracleParameter("itemId", itemId));
                        cmd.ExecuteNonQuery();
                    }

                    // Insert new Order
                    int newOrderId = 0;
                    using (var cmd = new OracleCommand("INSERT INTO \"Order\" (OrderID, CustomerID, OrderTime, OrderAmount, OrderStatus) VALUES (ORDER_SEQ.NEXTVAL, :custId, SYSTIMESTAMP, :amount, 'Pending') RETURNING OrderID INTO :newId", con))
                    {
                        cmd.Parameters.Add(new OracleParameter("custId", customerId));
                        cmd.Parameters.Add(new OracleParameter("amount", price));
                        var param = new OracleParameter("newId", OracleDbType.Int32) { Direction = System.Data.ParameterDirection.Output };
                        cmd.Parameters.Add(param);
                        cmd.ExecuteNonQuery();
                        newOrderId = Convert.ToInt32(param.Value.ToString());
                    }

                    // Insert into Order_FoodItem
                    using (var cmd = new OracleCommand("INSERT INTO Order_FoodItem (OrderID, ItemID, RestaurantID, OrderedQuantity) VALUES (:oid, :itemId, :restId, 1)", con))
                    {
                        cmd.Parameters.Add(new OracleParameter("oid", newOrderId));
                        cmd.Parameters.Add(new OracleParameter("itemId", itemId));
                        cmd.Parameters.Add(new OracleParameter("restId", restaurantId));
                        cmd.ExecuteNonQuery();
                    }

                    // Insert into Transaction table
                    using (var cmd = new OracleCommand("INSERT INTO Transaction (TransactionID, OrderID, WalletID, TransactionTime, TransactionAmount, Type, Note) VALUES (TRANSACTION_SEQ.NEXTVAL, :oid, :wid, SYSTIMESTAMP, :amount, 'Debit', 'Order Payment')", con))
                    {
                        cmd.Parameters.Add(new OracleParameter("oid", newOrderId));
                        cmd.Parameters.Add(new OracleParameter("wid", walletId));
                        cmd.Parameters.Add(new OracleParameter("amount", price));
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();

                    TempData["SuccessMessage"] = "Order placed successfully!";
                    return RedirectToAction("Orders", "CustomerDashboard");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    TempData["ErrorMessage"] = "Order failed: " + ex.Message;
                    return RedirectToAction("Details", "Food", new { id = itemId });
                }
            }
        }

    }
}