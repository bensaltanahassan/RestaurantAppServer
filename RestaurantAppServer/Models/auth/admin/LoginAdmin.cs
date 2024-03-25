using System.ComponentModel.DataAnnotations;

namespace RestaurantAppServer.Models.auth.admin
{
    public class LoginAdmin
    {
        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public string Password
        {
            get; set;
        }
    }
}
