using System.ComponentModel.DataAnnotations;

namespace RestaurantAppServer.Models.auth.admin
{
    public class ResetPasswordAdmin
    {
        [Required]
        public string Password { get; set; }
        [Compare("Password", ErrorMessage = "The password and the confirm password doesnt match!")]
        public string ConfirmPassword { get; set; }
        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }

    }
}
