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
        public string Password { get; set; }
        public string Adress { get;set; }
    }
}
