using System.Collections.Generic;

namespace Food_Court_Management_System.Models
{
    public class FoodMenuViewModel
    {
        public List<FoodItemModel> FoodItems { get; set; } = new List<FoodItemModel>();
        public FoodItemModel NewItem { get; set; } = new FoodItemModel();
    }
}
