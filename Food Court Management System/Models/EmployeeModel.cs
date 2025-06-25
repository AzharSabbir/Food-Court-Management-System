using System;
using System.ComponentModel.DataAnnotations;
namespace Food_Court_Management_System.Models
{
    public class EmployeeModel
    {
        public int EmployeeID { get; set; }
        public int RestaurantID { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Name { get; set; }
        public string Designation { get; set; }
    }
}