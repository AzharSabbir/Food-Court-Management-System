using System;
using System.Collections.Generic;
using System.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Food_Court_Management_System.Controllers
{
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


            return View();
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