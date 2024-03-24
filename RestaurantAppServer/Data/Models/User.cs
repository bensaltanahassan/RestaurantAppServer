using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantAppServer.Data.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Password { get; set; }
        public bool IsVerified { get; set; }
        public string EmailVerificationToken { get; set; }
        public string ResetCode { get; set; }
        public DateTime? ResetCodeExpiry { get; set; } = null;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(image))]
        public int? ImageId { get; set; }
        public Image? image { get; set; }
    }
}
