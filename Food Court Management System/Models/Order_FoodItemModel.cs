using System;
using System.ComponentModel.DataAnnotations;
namespace Food_Court_Management_System.Models
{
    public class Order_FoodItemModel
    {
        public int OrderID { get; set; }
        public int ItemID { get; set; }
        public int RestaurantID { get; set; }
        public int OrderedQuantity { get; set; }
    }
}