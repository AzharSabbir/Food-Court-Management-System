using System;
using System.ComponentModel.DataAnnotations;
namespace Food_Court_Management_System.Models
{
    public class FoodItemModel
    {
        public int ItemID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}