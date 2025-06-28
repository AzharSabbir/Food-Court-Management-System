using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Food_Court_Management_System.Models.Custom
{
    public class FoodItemDetailsViewModel
    {
        public int ItemID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CustomerName { get; set; }

        public List<RestaurantOfferInfo> AvailableRestaurants { get; set; }
    }

    public class RestaurantOfferInfo
    {
        public int RestaurantID { get; set; }
        public string RestaurantName { get; set; }
        public string RestaurantAddress { get; set; }
        public decimal Price { get; set; }
        public int AvailableQuantity { get; set; }
    }

}