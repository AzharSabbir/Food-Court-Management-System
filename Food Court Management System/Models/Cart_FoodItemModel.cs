using System;
using System.ComponentModel.DataAnnotations;
namespace Food_Court_Management_System.Models
{
    public class Cart_FoodItemModel
    {
        public int CartID { get; set; }
        public int ItemID { get; set; }
        public int RestaurantID { get; set; }
        public int Quantity { get; set; }
    }
}