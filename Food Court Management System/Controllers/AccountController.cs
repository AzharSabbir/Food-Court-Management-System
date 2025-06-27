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
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult CustomerDetails()
        {
            var model = new CustomerModel();

            using (OracleConnection con = new OracleConnection(ConfigurationManager.ConnectionStrings["con"].ConnectionString))
            {
                con.Open();

                // Fetch customer by username
                string customerQuery = "SELECT * FROM Customer WHERE Username = :username";

                using (var cmd = new OracleCommand(customerQuery, con))
                {
                    cmd.Parameters.Add(new OracleParameter("username", User.Identity.Name));

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model.CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID"));
                            model.Username = reader.GetString(reader.GetOrdinal("Username"));
                            model.Email = reader.GetString(reader.GetOrdinal("Email"));
                            model.Phone = reader.GetString(reader.GetOrdinal("Phone"));
                            model.Name = reader.GetString(reader.GetOrdinal("Name"));
                            model.Address = reader.GetString(reader.GetOrdinal("Address"));
                        }
                    }
                }

                // Fetch wallet balance using CustomerID
                string walletQuery = "SELECT Balance FROM Wallet WHERE CustomerID = :customerId";

                using (var cmd = new OracleCommand(walletQuery, con))
                {
                    cmd.Parameters.Add(new OracleParameter("customerId", model.CustomerID));
                    object result = cmd.ExecuteScalar();

                    model.Balance = result != null ? Convert.ToDecimal(result) : 0m;
                }
            }

            return View(model);
        }

    }
}