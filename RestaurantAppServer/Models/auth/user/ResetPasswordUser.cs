using System.ComponentModel.DataAnnotations;

namespace RestaurantAppServer.Models.auth.user
{
    public class ResetPasswordUser
    {
        [Required]
        [RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).{8,}$", ErrorMessage = "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, and one digit.")]
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
