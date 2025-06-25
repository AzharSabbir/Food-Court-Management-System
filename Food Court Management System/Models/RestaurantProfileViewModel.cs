using System.ComponentModel.DataAnnotations;

namespace Food_Court_Management_System.Models
{
    public class RestaurantProfileViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Phone { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
