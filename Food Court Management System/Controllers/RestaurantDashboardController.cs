using Food_Court_Management_System.Models;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Food_Court_Management_System.Controllers
{
    public class RestaurantDashboardController : Controller
    {
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated && !User.Identity.Name.Contains("@"))
            {
                // Confirming it is restaurant
                return RedirectToAction("Index", "Home");
            }

            return View();
        }
    }
}
