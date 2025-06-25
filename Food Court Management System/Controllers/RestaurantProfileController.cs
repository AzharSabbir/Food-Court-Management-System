using System.Web.Mvc;
using Food_Court_Management_System.Models;

namespace Food_Court_Management_System.Controllers
{
    public class RestaurantProfileController : Controller
    {
        [HttpGet]
        public ActionResult RestaurantProfile()
        {
            var model = new RestaurantProfileViewModel
            {
                Name = "Restaurant A",
                Email = "restaurant@example.com",
                Phone = "123-456-7890",
                Address = "123 Main St, City, Country",
                Password = "******"
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult RestaurantProfile(RestaurantProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Save logic here (e.g., update database)
                ViewBag.Message = "Changes saved successfully.";
            }

            return View(model);
        }
    }
}
