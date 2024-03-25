using System.ComponentModel.DataAnnotations;

namespace RestaurantAppServer.Models.auth.user
{
    public class RegisterUser
    {
        [Required(ErrorMessage = "Username is required")]
        public string FullName { get; set; }
        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Phone is required")]
        public string Phone { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).{8,}$", ErrorMessage = "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, and one digit.")]
        public string Password { get; set; }
        public string Adress { get;set; }
    }
}
