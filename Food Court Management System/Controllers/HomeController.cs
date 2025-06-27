using System;
using System.Collections.Generic;
using System.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Food_Court_Management_System.Controllers
{
    public class Restaurant
    {
        public int RestaurantID { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
    }
    [AllowAnonymous]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated && User.Identity.Name.Contains("@"))
            {
                // Confirming it is restaurant
                return RedirectToAction("Index", "RestaurantDashboard");
            }

            List<Restaurant> restaurants = new List<Restaurant>();

            try
            {
                using (OracleConnection con = new OracleConnection(ConfigurationManager.ConnectionStrings["con"].ConnectionString))
                {
                    con.Open();
                    string query = "SELECT * FROM Restaurant";

                    using (OracleCommand cmd = new OracleCommand(query, con))
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            restaurants.Add(new Restaurant
                            {
                                RestaurantID = reader.GetInt32(reader.GetOrdinal("RestaurantID")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                Password = reader.GetString(reader.GetOrdinal("Password")),
                                Phone = reader.GetString(reader.GetOrdinal("Phone")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Address = reader.GetString(reader.GetOrdinal("Address"))
                            });
                        }
                    }
                }

                ViewBag.Result = "Success";
            }
            catch (Exception e)
            {
                ViewBag.Result = "Error: " + e.Message;
            }

            return View(restaurants);
        }

        public ActionResult About()
        {
            if (User.Identity.IsAuthenticated && User.Identity.Name.Contains("@"))
            {
                // Confirming it is restaurant
                return RedirectToAction("Index", "RestaurantDashboard");
            }
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            if (User.Identity.IsAuthenticated && User.Identity.Name.Contains("@"))
            {
                // Confirming it is restaurant
                return RedirectToAction("Index", "RestaurantDashboard");
            }
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}