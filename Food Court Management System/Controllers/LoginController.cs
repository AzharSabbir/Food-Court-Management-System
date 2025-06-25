using System.Web.Mvc;
using Food_Court_Management_System.Models;

namespace Food_Court_Management_System.Controllers
{
    public class LoginController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RestaurantLogin(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Replace with actual authentication logic
                if (model.Email == "restaurant@example.com" && model.Password == "1234")
                    return RedirectToAction("RestaurantDashboard", "RestaurantDashboard");

                ModelState.AddModelError("", "Invalid restaurant credentials.");
            }
            ViewBag.LoginType = "Restaurant";
            return View("Index", model);
        }

        [HttpPost]
        public ActionResult CustomerLogin(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Replace with actual authentication logic
                if (model.Email == "customer@example.com" && model.Password == "1234")
                    return RedirectToAction("CustomerDashboard", "CustomerDashboard");

                ModelState.AddModelError("", "Invalid customer credentials.");
            }
            ViewBag.LoginType = "Customer";
            return View("Index", model);
        }
    }
}
