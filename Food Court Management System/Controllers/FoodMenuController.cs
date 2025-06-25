using System.Collections.Generic;
using System.Web.Mvc;
using Food_Court_Management_System.Models;

namespace Food_Court_Management_System.Controllers
{
    public class FoodMenuController : Controller
    {
        // Static list for demo
        public static List<FoodItemModel> Menu = new List<FoodItemModel>
        {
            new FoodItemModel { ItemID = 1, Name = "Pizza", Description = "Cheese and pepperoni pizza" },
            new FoodItemModel { ItemID = 2, Name = "Burger", Description = "Beef burger with lettuce and tomato" },
            new FoodItemModel { ItemID = 3, Name = "Pasta", Description = "Spaghetti with marinara sauce" }
        };

        public ActionResult Index()
        {
            var model = new FoodMenuViewModel
            {
                FoodItems = Menu
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult AddItem(FoodItemModel item)
        {
            item.ItemID = Menu.Count + 1;
            Menu.Add(item);
            return RedirectToAction("Index");
        }
    }
}
