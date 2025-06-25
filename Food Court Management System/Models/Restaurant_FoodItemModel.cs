using System;
using System.ComponentModel.DataAnnotations;
namespace Food_Court_Management_System.Models
{
    public class Restaurant_FoodItemModel
    {
        public int RestaurantID { get; set; }
        public int ItemID { get; set; }
        public int AvailableQuantity { get; set; }
        public decimal Price { get; set; }
    }
}