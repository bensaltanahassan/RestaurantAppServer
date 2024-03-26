using System.ComponentModel.DataAnnotations;

namespace RestaurantAppServer.Models.auth.user
{
    public class ResetPasswordUser
    {
        [Required]
        public string Password { get; set; }
        [Compare("Password", ErrorMessage = "The password and the confirm password doesnt match!")]
        public string ConfirmPassword { get; set; }
        [Required]
        public string ResetCode { get; set; }
        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
    }
}
